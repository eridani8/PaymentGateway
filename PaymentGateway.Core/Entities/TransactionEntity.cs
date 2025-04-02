﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentGateway.Core.Enums;

namespace PaymentGateway.Core.Entities;

public sealed class TransactionEntity
{
    public TransactionEntity(Guid paymentId, TransactionSource source, decimal extractedAmount)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        Source = source;
        ExtractedAmount = extractedAmount;
        ReceivedAt = DateTime.UtcNow;
    }
    
    private TransactionEntity() { }
    
    /// <summary>
    /// Идентификатор транзакции
    /// </summary>
    public Guid Id { get; init; }
    
    /// <summary>
    /// Идентификатор связанного платежа
    /// </summary>
    public Guid PaymentId { get; init; }
    
    [ForeignKey(nameof(PaymentId))]
    public PaymentEntity? Payment { get; init; }
    
    /// <summary>
    /// Источник транзакции
    /// </summary>
    public TransactionSource Source { get; init; }
    
    /// <summary>
    /// Сумма, извлечённая из платежа
    /// </summary>
    [Range(0, 9999999999999999.99)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ExtractedAmount { get; init; }
    
    /// <summary>
    /// Дата и время получения транзакции
    /// </summary>
    public DateTime ReceivedAt { get; init; }
    
}