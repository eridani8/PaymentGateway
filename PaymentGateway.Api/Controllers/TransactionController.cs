using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Shared.DTOs.Transaction;

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
            if (transaction is null) throw new ApplicationException("Ошибка обработки транзакции");
            return Ok(transaction);
        }
        catch (RequisiteNotFound)
        {
            return NotFound();
        }
        catch (WrongPaymentAmount e)
        {
            return BadRequest(e.Message);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors.GetErrors());
        }
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<TransactionDto>>> GetAll()
    {
        try
        {
            var transactions = await service.GetAllTransactions();
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<TransactionDto>>> GetUserTransactions()
    {
        try
        {
            var userId = User.GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();
            var transactions = await service.GetUserTransactions(userId);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}