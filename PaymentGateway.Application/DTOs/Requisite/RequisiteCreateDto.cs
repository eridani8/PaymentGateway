using System.ComponentModel;

namespace PaymentGateway.Application.DTOs.Requisite;

public class RequisiteCreateDto
{
    public required string FullName { get; init; }
    [DefaultValue("+1234567890")] public required string PhoneNumber { get; init; } // ~
    [DefaultValue("12345678901234")] public required string CardNumber { get; init; } // ~
    [DefaultValue("1234567890123456789012345")] public required string BankAccountNumber { get; init; } // ~
    [DefaultValue(true)] public bool IsActive { get; init; } // ~
    [DefaultValue(5000)] public decimal? MaxAmount { get; init; } // ~
    [DefaultValue(100)] public int? CooldownMinutes { get; init; } // ~
    [DefaultValue(1)] public int? Priority { get; init; } // ~
}