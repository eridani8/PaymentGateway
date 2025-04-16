﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Core.Entities;

public sealed class TransactionEntity : ITransactionEntity, ICacheable
{
    // ReSharper disable once EmptyConstructor
    public TransactionEntity() { }
    
    /// <summary>
    /// Идентификатор транзакции
    /// </summary>
    public Guid Id { get; init; }
    
    /// <summary>
    /// Идентификатор реквизита
    /// </summary>
    public Guid? RequisiteId { get; init; }
    
    public RequisiteEntity? Requisite { get; init; }
    
    /// <summary>
    /// Идентификатор платежа
    /// </summary>
    public Guid? PaymentId { get; init; }
    
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
    
    /// <summary>
    /// Оригинальный текст сообщения
    /// </summary>
    [MaxLength(300)]
    public string? RawMessage { get; init; }
}