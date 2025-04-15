using Microsoft.AspNetCore.Identity;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Application.Interfaces;

public interface IGatewayHandler
{
    Task HandleRequisites(IUnitOfWork unit);
    Task HandleUnprocessedPayments(IUnitOfWork unit);
    Task HandleExpiredPayments(IUnitOfWork unit);
    Task HandleUserFundsReset(UserManager<UserEntity> userManager);
}