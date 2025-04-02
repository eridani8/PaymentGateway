using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;

namespace PaymentGateway.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Requisite> Requisites { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(e => e.ExternalPaymentId).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            
            entity.HasOne(e => e.Transaction)
                .WithOne()
                .HasForeignKey<Payment>(e => e.TransactionId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        modelBuilder.Entity<Requisite>(entity =>
        {
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.Type);
        });
        
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasIndex(e => e.PaymentId);
            entity.HasIndex(e => e.ReceivedAt);

            entity.HasOne(e => e.Payment)
                .WithOne(p => p.Transaction)
                .HasForeignKey<Transaction>(e => e.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}