using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentGateway.Core.Enums;

namespace PaymentGateway.Core.Entities;

public sealed class Transaction
{
    public Transaction(Guid paymentId, TransactionSource source, decimal extractedAmount)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        Source = source;
        ExtractedAmount = extractedAmount;
        ReceivedAt = DateTime.UtcNow;
    }
    
    private Transaction() { }
    
    /// <summary>
    /// Идентификатор транзакции
    /// </summary>
    [Key]
    public Guid Id { get; init; }
    
    /// <summary>
    /// Идентификатор связанного платежа
    /// </summary>
    [Required]
    public Guid PaymentId { get; init; }
    
    [ForeignKey(nameof(PaymentId))]
    public Payment? Payment { get; init; }
    
    /// <summary>
    /// Источник транзакции
    /// </summary>
    [Required]
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
    [Required]
    public DateTime ReceivedAt { get; init; }
    
}