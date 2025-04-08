using System.ComponentModel;
using PaymentGateway.Core.Enums;

namespace PaymentGateway.Application.DTOs.Requisite;

public class RequisiteCreateDto
{
    [DefaultValue("Name N")] public required string FullName { get; init; }
    public PaymentType PaymentType { get; init; }
    [DefaultValue("123456789012345")] public required string PaymentData { get; init; }
    [DefaultValue("1234567890123456789012345")] public required string BankNumber { get; init; }
    public bool IsActive { get; init; }
    [DefaultValue(5000)] public decimal MaxAmount { get; init; }
    [DefaultValue("03:00:00")] public TimeSpan Cooldown { get; init; }
    [DefaultValue(1)] public int Priority { get; init; }
    [DefaultValue("09:00")] public TimeOnly WorkFrom { get; init; }
    [DefaultValue("18:00")] public TimeOnly WorkTo { get; init; }
}