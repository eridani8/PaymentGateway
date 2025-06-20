﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Core.Entities;

public sealed class PaymentEntity : BaseEntity
{
    // ReSharper disable once EmptyConstructor
    public PaymentEntity() { }
    
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; init; }

    public UserEntity User { get; set; }

    /// <summary>
    /// Сумма платежа
    /// </summary>
    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(20,0)")] 
    public decimal Amount { get; init; }

    /// <summary>
    /// Идентификатор связанного реквизита
    /// </summary>
    public Guid? RequisiteId { get; set; }
    
    public RequisiteEntity? Requisite { get; set; }
    
    /// <summary>
    /// Текущий статус платежа
    /// </summary>
    public PaymentStatus Status { get; set; }
    
    /// <summary>
    /// Дата и время создания платежа
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Дата и время обработки платежа
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>
    /// Дата и время истечения срока действия платежа (null для подтвержденных)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Идентификатор связанной транзакции
    /// </summary>
    public Guid? TransactionId { get; set; }
    
    public TransactionEntity? Transaction { get; set; }
    
    public Guid? ManualConfirmUserId { get; set; }
    
    public UserEntity? ManualConfirmUser { get; set; }
    
    public Guid? CanceledByUserId { get; set; }
    
    public UserEntity? CanceledByUser { get; set; }
    
    public void MarkAsPending(RequisiteEntity requisite)
    {
        RequisiteId = requisite.Id;
        Requisite = requisite;
        Status = PaymentStatus.Pending;
    }

    public void ConfirmTransaction(TransactionEntity transaction)
    {
        Status = PaymentStatus.Confirmed;
        TransactionId = transaction.Id;
        Transaction = transaction;
        
        if (RequisiteId.HasValue)
        {
            transaction.RequisiteId = RequisiteId;
            transaction.Requisite = Requisite;
        }
        
        ProcessedAt = DateTime.UtcNow;
        ExpiresAt = null;
    }

    public void ManualConfirm(Guid confirmUserId)
    {
        Status = PaymentStatus.ManualConfirm;
        ManualConfirmUserId = confirmUserId;
        ProcessedAt = DateTime.UtcNow;
        ExpiresAt = null;
    }
}