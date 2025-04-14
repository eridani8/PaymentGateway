namespace PaymentGateway.Shared.Interfaces;

public interface INotificationService
{
    Task NotifyUserUpdated();
    Task NotifyPaymentUpdated();
    Task NotifyRequisiteUpdated();
    Task NotifyPaymentStatusChanged(string paymentId);
} 