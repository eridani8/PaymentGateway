using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Shared.Interfaces;

public interface INotificationService
{
    Task NotifyUserUpdated(UserDto user);
    Task NotifyUserDeleted(Guid id);
    Task NotifyRequisiteUpdated(RequisiteDto requisite);
    Task NotifyRequisiteDeleted(Guid requisiteId, Guid userId);
    Task NotifyPaymentUpdated(PaymentDto payment);
    Task NotifyPaymentDeleted(Guid id, Guid? userId);
    Task NotifySpecificPaymentUpdated(PaymentDto payment);
} 