using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.DTOs.Chat;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Shared.Interfaces;

public interface IHubClient
{
    Task RequisiteUpdated(RequisiteDto requisite);
    Task RequisiteDeleted(Guid requisiteId);
    Task UserUpdated(UserDto user);
    Task UserDeleted(Guid userId);
    Task PaymentUpdated(PaymentDto payment);
    Task PaymentDeleted(Guid paymentId);
    Task KeepAlive();
    Task ChatMessageReceived(ChatMessageDto message);
    Task UserConnected(UserState state);
    Task UserDisconnected(UserState state);
    Task ChangeRequisiteAssignmentAlgorithm(RequisiteAssignmentAlgorithm algorithm);
} 