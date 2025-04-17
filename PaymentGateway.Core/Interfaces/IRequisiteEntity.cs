using PaymentGateway.Core.Entities;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Core.Interfaces;

public interface IRequisiteEntity
{
    Guid UserId { get; init; }
    UserEntity User { get; set; }
    string FullName { get; init; }
    PaymentType PaymentType { get; init; }
    string PaymentData { get; init; }
    string BankNumber { get; init; }
    DateTime CreatedAt { get; init; }
    DateTime? LastOperationTime { get; set; }
    Guid? PaymentId { get; set; }
    PaymentEntity? Payment { get; set; }
    RequisiteStatus Status { get; set; }
    bool IsActive { get; set; }
    decimal DayReceivedFunds { get; set; }
    decimal DayLimit { get; set; }
    DateTime LastDayFundsResetAt { get; set; }
    decimal MonthReceivedFunds { get; set; }
    decimal MonthLimit { get; set; }
    DateTime LastMonthlyFundsResetAt { get; set; }
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
    bool ProcessStatus(DateTime now, TimeOnly nowTimeOnly, out RequisiteStatus status);
}