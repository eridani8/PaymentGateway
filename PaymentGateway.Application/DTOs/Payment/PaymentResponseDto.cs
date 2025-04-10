﻿using PaymentGateway.Core.Enums;

namespace PaymentGateway.Application.DTOs.Payment;

public class PaymentResponseDto
{
    public Guid Id { get; init; }
    public Guid ExternalPaymentId { get; init; }
    public decimal Amount { get; init; }
    public Guid? RequisiteId { get; init; }
    public PaymentStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public Guid? TransactionId { get; init; }
}