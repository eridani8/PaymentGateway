using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentGateway.Core.Encryption;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Core.Entities;

public class RequisiteEntity : BaseEntity
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    // ReSharper disable once EmptyConstructor
    public RequisiteEntity() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Пользователь
    /// </summary>
    public UserEntity User { get; set; }

    /// <summary>
    /// Имя и фамилия
    /// </summary>
    [MaxLength(100)]
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Тип реквизита
    /// </summary>
    public PaymentType PaymentType { get; init; }

    /// <summary>
    /// Данные для платежа
    /// </summary>
    [MaxLength(255)]
    [Encrypted]
    public string PaymentData { get; init; } = string.Empty;

    /// <summary>
    /// Номер банковского счета
    /// </summary>
    [MaxLength(255)]
    [Encrypted]
    public string BankNumber { get; init; } = string.Empty;

    /// <summary>
    /// Дата и время создания
    /// </summary>
    public DateTime CreatedAt { get; init; }

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
    public RequisiteStatus Status { get; set; }

    public bool IsActive { get; set; }

    /// <summary>
    /// Количество операций за день
    /// </summary>
    public int DayOperationsCount { get; set; }
    
    /// <summary>
    /// Полученные за день средства
    /// </summary>
    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(20,0)")]
    public decimal DayReceivedFunds { get; set; }

    /// <summary>
    /// Дневной лимит
    /// </summary>
    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(20,0)")]
    public decimal DayLimit { get; set; }

    /// <summary>
    /// Дата последнего сброса полученных средств
    /// </summary>
    public DateTime LastDayFundsResetAt { get; set; }

    /// <summary>
    /// Полученные за месяц средства
    /// </summary>
    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(20,0)")]
    public decimal MonthReceivedFunds { get; set; }

    /// <summary>
    /// Месячный лимит
    /// </summary>
    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(20,0)")]
    public decimal MonthLimit { get; set; }

    /// <summary>
    /// Дата последнего сброса полученных за месяц средств
    /// </summary>
    public DateTime LastMonthlyFundsResetAt { get; set; }

    /// <summary>
    /// Задержка перед следующей операцией
    /// </summary>
    public TimeSpan Cooldown { get; set; }

    /// <summary>
    /// Приоритет использования
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Временное ограничение от
    /// </summary>
    public TimeOnly WorkFrom { get; set; }

    /// <summary>
    /// Временное ограничение до
    /// </summary>
    public TimeOnly WorkTo { get; set; }

    /// <summary>
    /// ИД привязанного устройства
    /// </summary>
    public Guid? DeviceId { get; set; }

    public DeviceEntity? Device { get; set; }
    
    public void AssignToPayment(PaymentEntity payment)
    {
        PaymentId = payment.Id;
        Payment = payment;
        Status = RequisiteStatus.Pending;
    }

    public void ReleaseWithoutPayment()
    {
        Payment = null;
        PaymentId = null;
        Status = RequisiteStatus.Active;
    }

    public void ReleaseAfterPayment(decimal amount, out RequisiteStatus status)
    {
        DayReceivedFunds += amount;
        MonthReceivedFunds += amount;
        DayOperationsCount++;
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
        return DayReceivedFunds >= DayLimit || MonthReceivedFunds >= MonthLimit;
    }

    public RequisiteStatus DetermineStatus(DateTime now, TimeOnly nowTimeOnly)
    {
        if (DeviceId is null)
        {
            return RequisiteStatus.Frozen;
        }

        if (LimitReached() || !IsWorkingTime(nowTimeOnly))
        {
            return RequisiteStatus.Inactive;
        }

        return IsCooldownOver(now)
            ? RequisiteStatus.Active
            : RequisiteStatus.Cooldown;
    }

    public bool ProcessStatus(DateTime now, TimeOnly nowTimeOnly, out RequisiteStatus status)
    {
        status = Status;
        if (Status == RequisiteStatus.Pending && PaymentId is not null) return false;
        
        status = !IsActive ? RequisiteStatus.Inactive : DetermineStatus(now, nowTimeOnly);

        return status != Status;
    }
}