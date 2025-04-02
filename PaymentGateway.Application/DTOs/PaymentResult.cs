using PaymentGateway.Core.Entities;

namespace PaymentGateway.Application.DTOs;

public class PaymentResult
{
    public bool Success { get; set; }
    public PaymentEntity PaymentEntity { get; set; }
    public RequisiteEntity RequisiteEntity { get; set; }
    public string ErrorMessage { get; set; }
}