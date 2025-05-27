using PaymentGateway.Shared.DTOs.Device;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Application.Interfaces;

public interface INotificationService
{
    Task NotifyWalletUpdated(WalletDto wallet);
    Task NotifyUserUpdated(UserDto user);
    Task NotifyUserDeleted(Guid id);
    Task NotifyRequisiteUpdated(RequisiteDto requisite);
    Task NotifyRequisiteDeleted(Guid requisiteId, Guid userId);
    Task NotifyPaymentUpdated(PaymentDto payment);
    Task NotifyPaymentDeleted(Guid id, Guid? userId);
    Task NotifyRequisiteAssignmentAlgorithmChanged(RequisiteAssignmentAlgorithm algorithm);
    Task NotifyDeviceUpdated(DeviceDto device);
} 