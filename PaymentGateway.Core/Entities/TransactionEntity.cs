using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Core.Entities;

public sealed class TransactionEntity : BaseEntity
{
    // ReSharper disable once EmptyConstructor
    public TransactionEntity() { }
    
    /// <summary>
    /// Идентификатор реквизита
    /// </summary>
    public Guid? RequisiteId { get; set; }
    
    public RequisiteEntity? Requisite { get; set; }
    
    /// <summary>
    /// Идентификатор платежа
    /// </summary>
    public Guid? PaymentId { get; set; }
    
    public PaymentEntity? Payment { get; set; }
    
    /// <summary>
    /// Источник транзакции
    /// </summary>
    public TransactionSource Source { get; set; }
    
    /// <summary>
    /// Сумма, извлечённая из платежа
    /// </summary>
    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(20,0)")]
    public decimal ExtractedAmount { get; set; }
    
    /// <summary>
    /// Дата и время получения транзакции
    /// </summary>
    public DateTime ReceivedAt { get; set; }
    
    /// <summary>
    /// Оригинальный текст сообщения
    /// </summary>
    [MaxLength(300)]
    public string? RawMessage { get; set; }
}