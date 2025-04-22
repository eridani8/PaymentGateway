using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentGateway.Core.Entities;

public class UserEntity : IdentityUser<Guid>
{
    public bool IsActive { get; set; }
    public int RequisitesCount { get; set; }
    public int MaxRequisitesCount { get; set; }
    
    [Range(0, 9999999999999999)]
    [Column(TypeName = "decimal(20,0)")]
    public decimal MaxDailyMoneyReceptionLimit { get; set; }
    
    [Range(0, 9999999999999999)]
    [Column(TypeName = "decimal(20,0)")]
    public decimal ReceivedDailyFunds { get; set; }
    
    public DateTime LastFundsResetAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    [MaxLength(255)]
    
    [Encrypted]
    public string? TwoFactorSecretKey { get; set; }
}