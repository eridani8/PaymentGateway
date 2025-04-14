using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Shared.Interfaces;

public interface IHubClient
{
    Task UserUpdated(UserDto user);
    Task PaymentUpdated(PaymentDto payment);
    Task RequisiteUpdated(RequisiteDto requisite);
    Task RequisiteDeleted(Guid requisiteId);
    Task PaymentStatusChanged(PaymentDto payment);
} 