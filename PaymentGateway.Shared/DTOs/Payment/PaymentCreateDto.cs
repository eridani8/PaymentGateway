﻿namespace PaymentGateway.Shared.DTOs.Payment;

public class PaymentCreateDto
{
    public decimal Amount { get; set; }
    public Guid UserId { get; set; }
}