using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentGateway.Core.Enums;

namespace PaymentGateway.Core.Entities;

public sealed class TransactionEntity
{
    public TransactionEntity() { }
    
    /// <summary>
    /// Идентификатор транзакции
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Идентификатор реквизита
    /// </summary>
    public required Guid? RequisiteId { get; init; }
    
    public required RequisiteEntity? Requisite { get; init; }
    
    /// <summary>
    /// Идентификатор платежа
    /// </summary>
    public required Guid? PaymentId { get; init; }
    
    public PaymentEntity? Payment { get; init; }
    
    /// <summary>
    /// Источник транзакции
    /// </summary>
    public required TransactionSource Source { get; init; }
    
    /// <summary>
    /// Сумма, извлечённая из платежа
    /// </summary>
    [Range(0, 9999999999999999.99)]
    [Column(TypeName = "decimal(18,2)")]
    public required decimal ExtractedAmount { get; init; }
    
    /// <summary>
    /// Дата и время получения транзакции
    /// </summary>
    public required DateTime ReceivedAt { get; init; }
}