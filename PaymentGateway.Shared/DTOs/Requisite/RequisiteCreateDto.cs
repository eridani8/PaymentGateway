using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Shared.DTOs.Requisite;

public class RequisiteCreateDto
{
    public string FullName { get; set; } = string.Empty;
    public PaymentType PaymentType { get; set; }
    public string PaymentData { get; set; } = string.Empty;
    public string BankNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal MaxAmount { get; set; }
    public TimeSpan Cooldown { get; set; }
    public int Priority { get; set; }
    public TimeOnly WorkFrom { get; set; }
    public TimeOnly WorkTo { get; set; }
}