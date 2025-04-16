using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Shared.Interfaces;

public interface IHubClient
{
    Task RequisiteUpdated(RequisiteDto requisite);
    Task RequisiteDeleted(Guid requisiteId);
    Task UserUpdated(UserDto user);
    Task UserDeleted(Guid userId);
    Task PaymentUpdated(PaymentDto payment);
    Task PaymentDeleted(Guid paymentId);
} 