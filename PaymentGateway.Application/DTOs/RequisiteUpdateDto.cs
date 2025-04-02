namespace PaymentGateway.Application.DTOs;

public class RequisiteUpdateDto
{
    public decimal MaxAmount { get; set; }
    public bool IsActive { get; set; }
    public int Cooldown { get; set; }
    public int Priority { get; set; }
}