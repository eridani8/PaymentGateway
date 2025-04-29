using Faker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.Enums;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]/[action]")]
[Authorize(Roles = "Admin")]
public class TestsController(IRequisiteService requisiteService, IPaymentService paymentService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> CreateManyRequisites()
    {
        for (var i = 0; i < 10000; i++)
        {
            try
            {
                var requisiteDto = new RequisiteCreateDto()
                {
                    FullName = $"{Name.First()} {Name.Last()}",
                    BankNumber = "12345678909876554432123",
                    Cooldown = TimeSpan.FromSeconds(10),
                    IsActive = true,
                    MaxAmount = 999999999,
                    MonthLimit = 99999999999999,
                    PaymentType = PaymentType.PhoneNumber,
                    PaymentData = "+" + RandomNumber.Next(100, 999) + Phone.Number().Split(' ').First()
                        .Replace(".", "").Replace("(", "").Replace(")", "").Replace("-", ""),
                    Priority = 1
                };

                await requisiteService.CreateRequisite(requisiteDto, User.GetCurrentUserId());
            }
            catch
            {
                // ignore
            }
        }

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> CreateManyPayments()
    {
        for (var i = 0; i < 100000; i++)
        {
            var paymentDto = new PaymentCreateDto()
            {
                Amount = RandomNumber.Next(100, 1000),
                ExternalPaymentId = Guid.NewGuid()
            };

            var payment = await paymentService.CreatePayment(paymentDto);
            
            await Task.Delay(100);
        }

        return Ok();
    }
}