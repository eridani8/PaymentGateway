using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, ICryptographyService cryptographyService) : DbContext(options)
{
    public DbSet<PaymentEntity> Payments { get; set; }
    public DbSet<RequisiteEntity> Requisites { get; set; }
    public DbSet<TransactionEntity> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var stringEncryptionConverter = new StringEncryptionConverter(cryptographyService);
        
        modelBuilder.Entity<PaymentEntity>(entity =>
        {
            entity.HasIndex(e => e.Id).IsUnique();
            entity.HasIndex(e => e.ExternalPaymentId).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);

            entity
                .HasOne(e => e.Transaction)
                .WithOne(e => e.Payment)
                .HasForeignKey<TransactionEntity>(p => p.PaymentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        modelBuilder.Entity<TransactionEntity>(entity =>
        {
            entity.HasIndex(e => e.Id).IsUnique();
            entity.HasIndex(e => e.PaymentId);
        });
        
        modelBuilder.Entity<RequisiteEntity>(entity =>
        {
            entity.HasIndex(e => e.Id).IsUnique();
            
            entity.Property(x => x.PaymentData).HasConversion(stringEncryptionConverter);
            entity.Property(x => x.BankNumber).HasConversion(stringEncryptionConverter);
            
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Priority);
            
            entity.HasOne(r => r.CurrentPayment)
                .WithOne()
                .HasForeignKey<RequisiteEntity>(r => r.CurrentPaymentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}