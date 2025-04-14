using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Shared.Interfaces;

public interface INotificationService
{
    Task NotifyUserUpdated(UserDto user);
    Task NotifyPaymentUpdated(PaymentDto payment);
    Task NotifyRequisiteUpdated(RequisiteDto requisite);
    Task NotifyPaymentStatusChanged(PaymentDto payment);
} 