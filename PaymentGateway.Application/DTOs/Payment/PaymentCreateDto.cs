namespace PaymentGateway.Application.DTOs.Payment;

public class PaymentCreateDto
{
    public decimal Amount { get; set; }
    public Guid ExternalPaymentId { get; set; }
    public Guid? UserId { get; set; }
}