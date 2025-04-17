﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Core.Entities;

public class RequisiteEntity : IRequisiteEntity, ICacheable
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    // ReSharper disable once EmptyConstructor
    public RequisiteEntity() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Идентификатор реквизита
    /// </summary>
    public Guid Id { get; init; }

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
    public decimal MaxAmount { get; set; }

    /// <summary>
    /// Дата последнего сброса полученных средств
    /// </summary>
    public DateTime LastFundsResetAt { get; set; }

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

    public void AssignToPayment(PaymentEntity payment)
    {
        PaymentId = payment.Id;
        Payment = payment;
        Status = RequisiteStatus.Pending;
    }

    public void ReleaseWithoutPayment()
    {
        Status = RequisiteStatus.Active;
        Payment = null;
        PaymentId = null;
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

    public bool ProcessStatus(DateTime now, TimeOnly nowTimeOnly, out RequisiteStatus status)
    {
        status = Status;
        if (Status == RequisiteStatus.Pending && PaymentId is not null) return false;
        
        status = !IsActive ? RequisiteStatus.Inactive : DetermineStatus(now, nowTimeOnly);

        return status != Status;
    }
}