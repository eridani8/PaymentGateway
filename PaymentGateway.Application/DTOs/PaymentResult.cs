using PaymentGateway.Core.Entities;

namespace PaymentGateway.Application.DTOs;

public class PaymentResult
{
    public bool Success { get; set; }
    public Payment Payment { get; set; }
    public Requisite Requisite { get; set; }
    public string ErrorMessage { get; set; }
}