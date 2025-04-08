using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Enums;

namespace PaymentGateway.Core.Interfaces;

public interface IRequisiteEntity
{
    string FullName { get; init; }
    RequisiteType PaymentType { get; init; }
    string PaymentData { get; init; }
    string BankNumber { get; init; }
    DateTime CreatedAt { get; init; }
    DateTime? LastOperationTime { get; set; }
    Guid? PaymentId { get; set; }
    PaymentEntity? Payment { get; set; }
    RequisiteStatus Status { get; set; }
    decimal ReceivedFunds { get; set; }
    decimal MaxAmount { get; set; }
    int CooldownMinutes { get; set; }
    int Priority { get; set; }
    TimeOnly WorkFrom { get; set; }
    TimeOnly WorkTo { get; set; }
    void AssignToPayment(Guid paymentId);
    void ReleaseAfterPayment(decimal amount);
}