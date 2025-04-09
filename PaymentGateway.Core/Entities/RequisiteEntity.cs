using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Core.Entities;

public class RequisiteEntity : IRequisiteEntity, ICacheable
{
    public RequisiteEntity()
    {
    }

    /// <summary>
    /// Идентификатор реквизита
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Имя и фамилия владельца
    /// </summary>
    [MaxLength(40)]
    public required string FullName { get; init; }

    /// <summary>
    /// Тип реквизита
    /// </summary>
    public PaymentType PaymentType { get; init; }

    /// <summary>
    /// Данные для платежа
    /// </summary>
    [MaxLength(255)]
    [Encrypted]
    public required string PaymentData { get; init; }

    /// <summary>
    /// Номер банковского счета
    /// </summary>
    [MaxLength(255)]
    [Encrypted]
    public required string BankNumber { get; init; }

    /// <summary>
    /// Дата и время создания
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Дата и время последней операции
    /// </summary>
    public DateTime? LastOperationTime { get; set; }

    /// <summary>
    /// Идентификатор текущего платежа, если реквизит используется
    /// </summary>
    public Guid? PaymentId { get; set; }

    public PaymentEntity? Payment { get; set; }

    /// <summary>
    /// Статус реквизита
    /// </summary>
    public required RequisiteStatus Status { get; set; }

    public required bool IsActive { get; set; }

    /// <summary>
    /// Полученные средства
    /// </summary>
    [Range(0, 9999999999999999.99)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ReceivedFunds { get; set; }

    /// <summary>
    /// Максимальная сумма платежа
    /// </summary>
    [Range(0, 9999999999999999.99)]
    [Column(TypeName = "decimal(18,2)")]
    public required decimal MaxAmount { get; set; }

    /// <summary>
    /// Дата последнего сброса полученных средств
    /// </summary>
    public DateTime LastFundsResetAt { get; set; }

    /// <summary>
    /// Задержка перед следующей операцией
    /// </summary>
    public required TimeSpan Cooldown { get; set; }

    /// <summary>
    /// Приоритет использования
    /// </summary>
    public required int Priority { get; set; }

    /// <summary>
    /// Временное ограничение от
    /// </summary>
    public required TimeOnly WorkFrom { get; set; }

    /// <summary>
    /// Временное ограничение до
    /// </summary>
    public required TimeOnly WorkTo { get; set; }

    public void AssignToPayment(PaymentEntity payment)
    {
        PaymentId = payment.Id;
        Payment = payment;
        Status = RequisiteStatus.Pending;
    }

    public void ReleaseWithoutPayment()
    {
        Status = RequisiteStatus.Active;
    }

    public void ReleaseAfterPayment(decimal amount, out RequisiteStatus status)
    {
        ReceivedFunds += amount;
        PaymentId = null;
        Payment = null;
        LastOperationTime = DateTime.UtcNow;
        status = Status = Cooldown > TimeSpan.Zero
            ? RequisiteStatus.Cooldown
            : RequisiteStatus.Active;
    }

    public bool IsWorkingTime(TimeOnly currentTime)
    {
        if (WorkFrom == TimeOnly.MinValue && WorkTo == TimeOnly.MinValue) return true;

        if (WorkFrom <= WorkTo)
        {
            return currentTime >= WorkFrom && currentTime <= WorkTo;
        }

        return currentTime >= WorkFrom || currentTime <= WorkTo;
    }

    public bool IsCooldownOver(DateTime currentTime)
    {
        if (!LastOperationTime.HasValue) return true;

        return currentTime >= LastOperationTime.Value.Add(Cooldown);
    }

    public bool LimitReached()
    {
        return ReceivedFunds >= MaxAmount;
    }

    public RequisiteStatus DetermineStatus(DateTime now, TimeOnly nowTimeOnly)
    {
        if (LimitReached())
            return RequisiteStatus.Inactive;

        if (!IsWorkingTime(nowTimeOnly))
            return RequisiteStatus.Inactive;

        return IsCooldownOver(now)
            ? RequisiteStatus.Active
            : RequisiteStatus.Cooldown;
    }
}