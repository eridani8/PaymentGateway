using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentGateway.Core.Enums;

namespace PaymentGateway.Core.Entities;

public sealed class PaymentEntity
{
    public PaymentEntity(Guid paymentId, decimal amount)
    {
        Id = Guid.NewGuid();
        ExternalPaymentId = paymentId;
        Amount = amount;
        CreatedAt = DateTime.UtcNow;
    }

    private PaymentEntity() { }
    
    /// <summary>
    /// Идентификатор платежа
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Идентификатор платежа во внешней системе
    /// </summary>
    public Guid ExternalPaymentId { get; init; }

    /// <summary>
    /// Сумма платежа
    /// </summary>
    [Range(0, 9999999999999999.99)]
    [Column(TypeName = "decimal(18,2)")] 
    public decimal Amount { get; init; }

    /// <summary>
    /// Идентификатор связанного реквизита
    /// </summary>
    public Guid? RequisiteId { get; set; }
    
    public RequisiteEntity? Requisite { get; set; }
    
    /// <summary>
    /// Текущий статус платежа
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Created;
    
    /// <summary>
    /// Дата и время создания платежа
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Дата и время обработки платежа
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>
    /// Дата и время истечения срока действия платежа
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Идентификатор связанной транзакции
    /// </summary>
    public int? TransactionId { get; set; }
    
    public TransactionEntity? Transaction { get; set; }
}