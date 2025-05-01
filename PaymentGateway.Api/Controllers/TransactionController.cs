using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Results;
using PaymentGateway.Shared.DTOs.Transaction;
using Swashbuckle.AspNetCore.Annotations;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/transactions")]
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

        var result = await service.CreateTransaction(dto);
        
        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.RequisiteNotFound => NotFound(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };
        }
        
        return StatusCode(StatusCodes.Status201Created, result.Value);
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
        var result = await service.GetAllTransactions();
        
        if (result.IsFailure)
        {
            return StatusCode(500, result.Error.Message);
        }
        
        return Ok(result.Value);
    }
    
    [HttpGet("user")]
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
        var userId = User.GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();
        
        var result = await service.GetUserTransactions(userId);
        
        if (result.IsFailure)
        {
            return StatusCode(500, result.Error.Message);
        }
        
        return Ok(result.Value);
    }
}