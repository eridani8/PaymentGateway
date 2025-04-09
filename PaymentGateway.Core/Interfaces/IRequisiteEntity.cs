using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Enums;

namespace PaymentGateway.Core.Interfaces;

public interface IRequisiteEntity
{
    string FullName { get; init; }
    PaymentType PaymentType { get; init; }
    string PaymentData { get; init; }
    string BankNumber { get; init; }
    DateTime CreatedAt { get; init; }
    DateTime? LastOperationTime { get; set; }
    Guid? PaymentId { get; set; }
    PaymentEntity? Payment { get; set; }
    RequisiteStatus Status { get; set; }
    decimal ReceivedFunds { get; set; }
    decimal MaxAmount { get; set; }
    TimeSpan Cooldown { get; set; }
    int Priority { get; set; }
    TimeOnly WorkFrom { get; set; }
    TimeOnly WorkTo { get; set; }
    void AssignToPayment(PaymentEntity payment);
    void ReleaseWithoutPayment();
    void ReleaseAfterPayment(decimal amount, out RequisiteStatus status);
    bool IsWorkingTime(TimeOnly currentTime);
    bool IsCooldownOver(DateTime currentTime);
    bool LimitReached();
    RequisiteStatus DetermineStatus(DateTime now, TimeOnly nowTimeOnly);
}