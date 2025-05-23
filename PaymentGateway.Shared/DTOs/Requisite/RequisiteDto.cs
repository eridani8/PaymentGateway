using PaymentGateway.Shared.DTOs.Device;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Shared.DTOs.Requisite;

public class RequisiteDto // TODO cannot be activated if the administrator is deactivated
{
    public Guid Id { get; set; }
    public UserDto? User { get; set; }
    public string FullName { get; set; } = string.Empty;
    public PaymentType PaymentType { get; set; }
    public string PaymentData { get; set; } = string.Empty;
    public string BankNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public PaymentDto? Payment { get; set; }
    public RequisiteStatus Status { get; set; }
    public DateTime? LastOperationTime { get; set; }
    public decimal DayReceivedFunds { get; set; }
    public decimal DayLimit { get; set; }
    public decimal MonthReceivedFunds { get; set; }
    public decimal MonthLimit { get; set; }
    public TimeSpan Cooldown { get; set; }
    public int Priority { get; set; }
    public TimeOnly WorkFrom { get; set; }
    public TimeOnly WorkTo { get; set; }
    public DateTime? LastFundsResetAt { get; set; }
    public DateTime? LastMonthlyFundsResetAt { get; set; }
    public int DayOperationsCount { get; set; }
    public DeviceDto? Device { get; set; }
}