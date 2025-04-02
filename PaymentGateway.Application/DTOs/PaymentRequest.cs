namespace PaymentGateway.Application.DTOs;

public class PaymentRequest
{
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
}