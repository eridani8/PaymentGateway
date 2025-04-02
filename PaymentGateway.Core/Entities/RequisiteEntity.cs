using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentGateway.Core.Enums;

namespace PaymentGateway.Core.Entities;

public class RequisiteEntity
{
    public RequisiteEntity(RequisiteType type, string paymentData, string fullName, decimal maxAmount = 5000,
        int cooldownMinutes = 100, int priority = 1, bool isActive = true)
    {
        Id = Guid.NewGuid();
        Type = type;
        PaymentData = paymentData;
        FullName = fullName;
        MaxAmount = maxAmount;
        CooldownMinutes = cooldownMinutes;
        Priority = priority;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
    }

    private RequisiteEntity()
    {
    }

    /// <summary>
    /// Идентификатор реквизита
    /// </summary>
    [Key]
    public Guid Id { get; init; }

    /// <summary>
    /// Тип реквизита
    /// </summary>
    [Required]
    public RequisiteType Type { get; init; }

    /// <summary>
    /// Данные для платежа
    /// </summary>
    [Required]
    public required string PaymentData { get; init; }

    /// <summary>
    /// ФИО владельца
    /// </summary>
    [Required]
    [StringLength(70)]
    public required string FullName { get; init; }

    /// <summary>
    /// Дата и время создания
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Дата и время обновления
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Дата и время последней операции
    /// </summary>
    public DateTime? LastOperationTime { get; set; }

    /// <summary>
    /// Идентификатор текущего платежа, если реквизит используется
    /// </summary>
    public Guid? CurrentPaymentId { get; set; }

    [ForeignKey(nameof(CurrentPaymentId))] public PaymentEntity? CurrentPayment { get; set; }

    /// <summary>
    /// Активен ли реквизит
    /// </summary>
    [Required]
    public bool IsActive { get; set; }

    /// <summary>
    /// Дата и время отключения (если реквизит неактивен)
    /// </summary>
    public DateTime? InactiveAt { get; set; }

    /// <summary>
    /// Максимальная сумма платежа
    /// </summary>
    [Required]
    [Range(0, 9999999999999999.99)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MaxAmount { get; set; }

    /// <summary>
    /// Задержка перед следующей операцией
    /// </summary>
    [Required]
    public int CooldownMinutes { get; set; }

    /// <summary>
    /// Приоритет использования
    /// </summary>
    [Required]
    public int Priority { get; set; }
}