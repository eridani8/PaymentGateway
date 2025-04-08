using PaymentGateway.Core.Enums;

namespace PaymentGateway.Application.DTOs.Requisite;

public class RequisiteUpdateDto
{
    public required string FullName { get; init; }
    public PaymentType PaymentType { get; init; }
    public required string PaymentData { get; init; }
    public required string BankNumber { get; init; }
    public bool IsActive { get; init; }
    public decimal? MaxAmount { get; init; }
    public int? CooldownMinutes { get; init; }
    public int? Priority { get; init; }
    public TimeOnly WorkFrom { get; init; }
    public TimeOnly WorkTo { get; init; }
}