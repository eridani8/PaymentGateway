﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentGateway.Core.Entities;

public class UserEntity : IdentityUser<Guid>
{
    public bool IsActive { get; set; }
    public int RequisitesCount { get; set; }
    public int MaxRequisitesCount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal MaxDailyMoneyReceptionLimit { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ReceivedDailyFunds { get; set; }
    
    public DateTime LastFundsResetAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
}