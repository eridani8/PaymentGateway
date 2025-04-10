﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentGateway.Core.Enums;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Core.Entities;

public sealed class PaymentEntity : IPaymentEntity, ICacheable
{
    public PaymentEntity() { }
    
    /// <summary>
    /// Идентификатор платежа
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Идентификатор платежа во внешней системе
    /// </summary>
    public required Guid ExternalPaymentId { get; init; }
    
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// Сумма платежа
    /// </summary>
    [Range(0, 9999999999999999.99)]
    [Column(TypeName = "decimal(18,2)")] 
    public required decimal Amount { get; init; }

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
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Дата и время обработки платежа
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>
    /// Дата и время истечения срока действия платежа (null для подтвержденных)
    /// </summary>
    public required DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Идентификатор связанной транзакции
    /// </summary>
    public Guid? TransactionId { get; set; }
    
    public TransactionEntity? Transaction { get; set; }
    
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
        ProcessedAt = DateTime.UtcNow;
        ExpiresAt = null;
    }
}