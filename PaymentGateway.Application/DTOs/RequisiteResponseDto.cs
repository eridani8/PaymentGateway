using PaymentGateway.Core.Enums;

namespace PaymentGateway.Application.DTOs;

public class RequisiteResponseDto
{
    public Guid Id { get; set; }
    public required string FullName { get; set; }
    public required RequisiteType Type { get; set; }
    public required string PaymentData { get; init; }
    public bool IsActive { get; set; }
}