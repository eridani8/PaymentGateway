namespace PaymentGateway.Application.DTOs.Payment;

public class PaymentCreateDto
{
    public decimal Amount { get; set; }
    public Guid PaymentId { get; set; }
    public Guid? UserId { get; set; }
}