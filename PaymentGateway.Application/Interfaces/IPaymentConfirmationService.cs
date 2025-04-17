using PaymentGateway.Core.Entities;

namespace PaymentGateway.Application.Interfaces;

public interface IPaymentConfirmationService
{
    Task ProcessPaymentConfirmation(PaymentEntity payment, RequisiteEntity requisite, decimal amount);
} 