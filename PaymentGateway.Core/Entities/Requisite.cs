using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentGateway.Core.Enums;

namespace PaymentGateway.Core.Entities;

public class Requisite
{
    public Requisite(RequisiteType type, string paymentData, string ownerName, decimal maxAmount = 5000, int cooldownMinutes = 100, int priority = 1, bool isActive = true, DateTime? inactiveAt = null)
    {
        Id = Guid.NewGuid();
        Type = type;
        PaymentData = paymentData;
        OwnerName = ownerName;
        MaxAmount = maxAmount;
        CooldownMinutes = cooldownMinutes;
        Priority = priority;
        IsActive = isActive;
        InactiveAt = inactiveAt;
        CreatedAt = DateTime.UtcNow;
    }
    
    private Requisite() { }
    
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
    public required string OwnerName { get; init; }
    
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
    
    [ForeignKey(nameof(CurrentPaymentId))]
    public Payment? CurrentPayment { get; set; }
    
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