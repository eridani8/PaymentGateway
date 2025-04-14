using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Shared.DTOs.Transaction;
using PaymentGateway.Shared.Interfaces;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Produces("application/json")]
public class TransactionController(ITransactionService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TransactionDto>> Create([FromBody] TransactionCreateDto? dto)
    {
        if (dto is null) return BadRequest();

        try
        {
            var transaction = await service.CreateTransaction(dto);
            return Ok(transaction);
        }
        catch (RequisiteNotFound)
        {
            return NotFound();
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors.GetErrors());
        }
    }
}