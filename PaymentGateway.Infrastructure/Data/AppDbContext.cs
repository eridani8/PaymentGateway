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
        
        modelBuilder.Entity<RequisiteEntity>(entity =>
        {
            entity.HasIndex(e => e.Id).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.PaymentData);
            
            entity.Property(x => x.PaymentData).HasConversion(stringEncryptionConverter);
            entity.Property(x => x.BankNumber).HasConversion(stringEncryptionConverter);
            
            entity.HasOne(e => e.Payment)
                .WithOne()
                .HasForeignKey<RequisiteEntity>(e => e.PaymentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        modelBuilder.Entity<PaymentEntity>(entity =>
        {
            entity.HasIndex(e => e.Id).IsUnique();
            entity.HasIndex(e => e.ExternalPaymentId).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);

            entity
                .HasOne(e => e.Transaction)
                .WithOne(e => e.Payment)
                .HasForeignKey<TransactionEntity>(e => e.PaymentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        modelBuilder.Entity<TransactionEntity>(entity =>
        {
            entity.HasIndex(e => e.Id).IsUnique();
            entity.HasIndex(e => e.PaymentId);
            
            entity
                .HasOne(e => e.Payment)
                .WithOne(e => e.Transaction)
                .HasForeignKey<TransactionEntity>(e => e.PaymentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity
                .HasOne(e => e.Requisite)
                .WithOne()
                .HasForeignKey<TransactionEntity>(e => e.RequisiteId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}