using PaymentGateway.Core.Enums;

namespace PaymentGateway.Application.DTOs;

public class RequisiteCreateDto
{
    public required string FullName { get; set; }
    public required RequisiteType Type { get; set; }
    public required string PaymentData { get; init; }
}