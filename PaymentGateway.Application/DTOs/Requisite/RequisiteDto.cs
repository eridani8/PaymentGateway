using PaymentGateway.Core.Enums;

namespace PaymentGateway.Application.DTOs.Requisite;

public class RequisiteDto
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; }
    public required PaymentType PaymentType { get; init; }
    public required string PaymentData { get; init; }
    public required string BankNumber { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required Guid? PaymentId { get; init; }
    public required RequisiteStatus Status { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime? LastOperationTime { get; init; }
    public required decimal ReceivedFunds { get; init; }
    public required decimal MaxAmount { get; init; }
    public required TimeSpan Cooldown { get; init; }
    public required int Priority { get; init; }
    public required TimeOnly WorkFrom { get; init; }
    public required TimeOnly WorkTo { get; init; }
    public required DateTime? LastFundsResetAt { get; init; }
}