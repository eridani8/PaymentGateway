﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Core.Entities;

public class RequisiteEntity : IRequisiteEntity, ICacheable
{
    public RequisiteEntity() { }

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
    public required string PaymentData { get; init; }
    
    /// <summary>
    /// Номер банковского счета
    /// </summary>
    [MaxLength(255)]
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
    
    public void AssignToPayment(Guid paymentId)
    {
        PaymentId = paymentId;
        Status = RequisiteStatus.Pending;
        LastOperationTime = DateTime.UtcNow;
    }

    public void ReleaseAfterPayment(decimal amount)
    {
        ReceivedFunds += amount;
        PaymentId = null;
        LastOperationTime = DateTime.UtcNow;
        Status = Cooldown > TimeSpan.Zero
            ? RequisiteStatus.Cooldown
            : RequisiteStatus.Active;
    }

    public bool IsWorkingTime(TimeOnly currentTime)
    {
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
}