using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Shared.DTOs.Requisite;

public class RequisiteDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public PaymentType PaymentType { get; set; }
    public string PaymentData { get; set; } = string.Empty;
    public string BankNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? PaymentId { get; init; }
    public PaymentDto? Payment { get; init; }
    public RequisiteStatus Status { get; init; }
    public DateTime? LastOperationTime { get; init; }
    public decimal ReceivedFunds { get; init; }
    public decimal MaxAmount { get; init; }
    public TimeSpan Cooldown { get; init; }
    public int Priority { get; init; }
    public TimeOnly WorkFrom { get; init; }
    public TimeOnly WorkTo { get; init; }
    public DateTime? LastFundsResetAt { get; init; }
}