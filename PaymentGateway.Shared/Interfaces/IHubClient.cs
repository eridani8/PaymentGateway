namespace PaymentGateway.Shared.Interfaces;

public interface IHubClient
{
    Task UserUpdated();
    Task PaymentUpdated();
    Task RequisiteUpdated();
    Task PaymentStatusChanged(string paymentId);
} 