using PaymentGateway.Core.Enums;

namespace PaymentGateway.Application.DTOs.Requisite;

public class RequisiteResponseDto
{
    public Guid Id { get; init; }
    public RequisiteType Type { get; init; }
    public required string PaymentData { get; init; }
    public required string FullName { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid? CurrentPaymentId { get; init; }
    public bool IsActive { get; init; }
    public DateTime? InactiveAt { get; init; }
    public decimal MaxAmount { get; init; }
    public int CooldownMinutes { get; init; }
    public int Priority { get; init; }
}