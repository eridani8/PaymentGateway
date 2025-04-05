using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentGateway.Core.Enums;

namespace PaymentGateway.Core.Entities;

public class RequisiteEntity
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
    public RequisiteType PaymentType { get; init; }
    
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
    public Guid? CurrentPaymentId { get; set; }

    public PaymentEntity? CurrentPayment { get; set; }

    /// <summary>
    /// Статус реквизита
    /// </summary>
    public required RequisiteStatus Status { get; set; }

    /// <summary>
    /// Дата и время отключения (если реквизит неактивен)
    /// </summary>
    public DateTime? InactiveAt { get; set; }

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
    public required int CooldownMinutes { get; set; }

    /// <summary>
    /// Приоритет использования
    /// </summary>
    public required int Priority { get; set; }
}