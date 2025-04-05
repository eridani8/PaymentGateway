using System.ComponentModel;

namespace PaymentGateway.Application.DTOs.Requisite;

public class RequisiteUpdateDto
{
    public required string FullName { get; init; }
    public required string PhoneNumber { get; init; }
    public required string CardNumber { get; init; }
    public required string BankAccountNumber { get; init; }
    public bool IsActive { get; init; }
    [DefaultValue(5000)] public decimal? MaxAmount { get; init; } // ~
    [DefaultValue(100)] public int? CooldownMinutes { get; init; } // ~
    [DefaultValue(1)] public int? Priority { get; init; } // ~
}