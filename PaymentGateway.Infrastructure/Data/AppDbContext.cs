using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, ICryptographyService cryptographyService, ICache cache) : IdentityDbContext<UserEntity, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<PaymentEntity> Payments { get; set; }
    public DbSet<RequisiteEntity> Requisites { get; set; }
    public DbSet<TransactionEntity> Transactions { get; set; }
    
    // public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    // {
    //     var cacheUpdates = new List<ICacheable>();
    //     var cacheRemovals = new List<ICacheable>();
    //     
    //     foreach (var entry in ChangeTracker.Entries<ICacheable>())
    //     {
    //         switch (entry.State)
    //         {
    //             case EntityState.Added:
    //             case EntityState.Modified:
    //                 cacheUpdates.Add(entry.Entity);
    //                 break;
    //
    //             case EntityState.Deleted:
    //                 cacheRemovals.Add(entry.Entity);
    //                 break;
    //             case EntityState.Detached:
    //             case EntityState.Unchanged:
    //             default:
    //                 continue;
    //         }
    //     }
    //     
    //     var result = await base.SaveChangesAsync(cancellationToken);
    //
    //     if (result > 0)
    //     {
    //         foreach (var entity in cacheRemovals)
    //         {
    //             cache.Remove(entity.GetType(), entity.Id);
    //         }
    //     
    //         foreach (var entity in cacheUpdates)
    //         {
    //             cache.Set(entity.GetType(), entity.Id, entity);
    //         }
    //     }
    //     
    //     return result;
    // } // TODO cache
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyEncryptedProperties(cryptographyService);
        
        modelBuilder.Entity<RequisiteEntity>(entity =>
        {
            entity.HasIndex(e => e.Id).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.PaymentData);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.PaymentType);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.LastOperationTime);
            
            entity.HasOne(e => e.Payment)
                .WithOne()
                .HasForeignKey<RequisiteEntity>(e => e.PaymentId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        modelBuilder.Entity<PaymentEntity>(entity =>
        {
            entity.HasIndex(e => e.Id).IsUnique();
            entity.HasIndex(e => e.ExternalPaymentId).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.RequisiteId);
            entity.HasIndex(e => e.TransactionId);
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => e.ProcessedAt);
            entity.HasIndex(e => e.ManualConfirmUserId);

            entity
                .HasOne(e => e.Transaction)
                .WithOne(e => e.Payment)
                .HasForeignKey<TransactionEntity>(e => e.PaymentId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // entity
            //     .HasOne(e => e.Requisite)
            //     .WithOne(e => e.Payment)
            //     .HasForeignKey<PaymentEntity>(e => e.RequisiteId)
            //     .OnDelete(DeleteBehavior.SetNull);
        });
        
        modelBuilder.Entity<TransactionEntity>(entity =>
        {
            entity.HasIndex(e => e.Id).IsUnique();
            entity.HasIndex(e => e.PaymentId);
            entity.HasIndex(e => e.RequisiteId);
            entity.HasIndex(e => e.Source);
            entity.HasIndex(e => e.ReceivedAt);
            
            entity
                .HasOne(e => e.Payment)
                .WithOne(e => e.Transaction)
                .HasForeignKey<TransactionEntity>(e => e.PaymentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity
                .HasOne(e => e.Requisite)
                .WithMany()
                .HasForeignKey(e => e.RequisiteId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}