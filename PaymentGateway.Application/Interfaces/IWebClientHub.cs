using PaymentGateway.Shared.DTOs.Chat;
using PaymentGateway.Shared.DTOs.Device;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Application.Interfaces;

public interface IWebClientHub
{
    Task WalletUpdated(WalletDto wallet);
    Task RequisiteUpdated(RequisiteDto requisite);
    Task RequisiteDeleted(Guid requisiteId);
    Task UserUpdated(UserDto user);
    Task UserDeleted(Guid userId);
    Task PaymentUpdated(PaymentDto payment);
    Task PaymentDeleted(Guid paymentId);
    Task ChatMessageReceived(ChatMessageDto message);
    Task UserConnected(UserState state);
    Task UserDisconnected(UserState state);
    Task ChangeRequisiteAssignmentAlgorithm(RequisiteAssignmentAlgorithm algorithm);
    Task DeviceUpdated(DeviceDto device);
    Task DeviceDeleted(Guid deviceId);
    Task ChangeUsdtExchangeRate(decimal rate);
} 