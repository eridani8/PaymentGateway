using PaymentGateway.Core.Enums;

namespace PaymentGateway.Application.DTOs.Requisite;

public class RequisiteCreateDto
{
    public RequisiteType Type { get; init; }
    public required string PaymentData { get; init; }
    public required string FullName { get; init; }
    public decimal? MaxAmount { get; init; }
    public int? CooldownMinutes { get; init; }
    public int? Priority { get; init; }
}