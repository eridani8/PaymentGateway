using System.ComponentModel;
using PaymentGateway.Core.Enums;

namespace PaymentGateway.Application.DTOs.Requisite;

public class RequisiteUpdateDto
{
    public required string FullName { get; init; }
    public RequisiteType RequisiteType { get; init; }
    public required string PaymentData { get; init; }
    [DefaultValue("1234567890123456789012345")] public required string BankNumber { get; init; } // ~
    public bool IsActive { get; init; }
    [DefaultValue(5000)] public decimal? MaxAmount { get; init; } // ~
    [DefaultValue(100)] public int? CooldownMinutes { get; init; } // ~
    [DefaultValue(1)] public int? Priority { get; init; } // ~
}