using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Shared.DTOs.Transaction;
using Swashbuckle.AspNetCore.Annotations;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]/[action]")]
[Produces("application/json")]
[SwaggerTag("Управление транзакциями")]
public class TransactionController(ITransactionService service) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Создание транзакции",
        Description = "Создает новую транзакцию в системе",
        OperationId = "CreateTransaction"
    )]
    [SwaggerResponse(201, "Транзакция успешно создана", typeof(TransactionDto))]
    [SwaggerResponse(400, "Неверные входные данные")]
    [SwaggerResponse(404, "Реквизит не найден")]
    public async Task<ActionResult<TransactionDto>> Create([FromBody] TransactionCreateDto? dto)
    {
        if (dto is null) return BadRequest();

        try
        {
            var transaction = await service.CreateTransaction(dto);
            if (transaction is null) throw new ApplicationException("Ошибка обработки транзакции");
            return StatusCode(StatusCodes.Status201Created, transaction);
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
    [Authorize(Roles = "Admin,Support")]
    [SwaggerOperation(
        Summary = "Получение всех транзакций",
        Description = "Возвращает список всех транзакций в системе (только для администраторов и поддержки)",
        OperationId = "GetAllTransactions"
    )]
    [SwaggerResponse(200, "Список транзакций успешно получен", typeof(List<TransactionDto>))]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    [SwaggerResponse(500, "Внутренняя ошибка сервера")]
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
    [SwaggerOperation(
        Summary = "Получение транзакций пользователя",
        Description = "Возвращает список транзакций текущего пользователя",
        OperationId = "GetUserTransactions"
    )]
    [SwaggerResponse(200, "Список транзакций успешно получен", typeof(List<TransactionDto>))]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(500, "Внутренняя ошибка сервера")]
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