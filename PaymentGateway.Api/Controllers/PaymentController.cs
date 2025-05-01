using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Results;
using PaymentGateway.Shared.DTOs.Payment;
using Swashbuckle.AspNetCore.Annotations;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]/[action]")]
[Produces("application/json")]
[SwaggerTag("Управление платежами")]
public class PaymentController(
    IPaymentService service,
    ILogger<PaymentController> logger)
    : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Создание платежа",
        Description = "Создает новый платеж в системе",
        OperationId = "CreatePayment"
    )]
    [SwaggerResponse(200, "Платеж успешно создан", typeof(PaymentDto))]
    [SwaggerResponse(400, "Неверные входные данные")]
    [SwaggerResponse(409, "Дублирующийся платеж")]
    public async Task<ActionResult<Guid>> Create([FromBody] PaymentCreateDto? dto)
    {
        if (dto is null) return BadRequest();

        var result = await service.CreatePayment(dto);
        
        if (result.IsFailure)
        {
            if (result.Error.Code == ErrorCode.DuplicatePayment) return Conflict(result.Error.Message);
                
            return BadRequest(result.Error.Message);
        }
        
        logger.LogInformation("Создание платежа {paymentId} на сумму {amount}", result.Value.Id, result.Value.Amount);
        return Ok(result.Value.Id);
    }

    [HttpPut]
    [SwaggerOperation(
        Summary = "Ручное подтверждение платежа",
        Description = "Подтверждает платеж вручную",
        OperationId = "ManualConfirmPayment"
    )]
    [SwaggerResponse(200, "Платеж успешно подтвержден")]
    [SwaggerResponse(400, "Неверные входные данные")]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(404, "Платеж не найден")]
    public async Task<ActionResult> ManualConfirmPayment([FromBody] PaymentManualConfirmDto? dto)
    {
        if (dto is null) return BadRequest();

        var userId = User.GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();
        
        var result = await service.ManualConfirmPayment(dto, userId);
        
        if (result.IsFailure)
        {
            if (result.Error.Code == ErrorCode.PaymentNotFound) return NotFound(result.Error.Message);
                
            return BadRequest(result.Error.Message);
        }
        
        logger.LogInformation("Ручное подтверждение платежа {paymentId} [{User}]", result.Value.Id, User.GetCurrentUsername());
        return Ok();
    }
    
    [HttpPut]
    [Authorize(Roles = "Admin,Support")]
    [SwaggerOperation(
        Summary = "Отмена платежа",
        Description = "Отменяет платеж (только для администраторов и поддержки)",
        OperationId = "CancelPayment"
    )]
    [SwaggerResponse(200, "Платеж успешно отменен")]
    [SwaggerResponse(400, "Неверные входные данные")]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    [SwaggerResponse(404, "Платеж не найден")]
    public async Task<ActionResult> CancelPayment([FromBody] PaymentCancelDto? dto)
    {
        if (dto is null) return BadRequest();

        var userId = User.GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();
        
        var result = await service.CancelPayment(dto, userId);
        
        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.PaymentNotFound => NotFound(result.Error.Message),
                ErrorCode.InsufficientPermissions => Forbid(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };
        }
        
        logger.LogInformation("Отмена платежа {paymentId} [{User}]", result.Value.Id, User.GetCurrentUsername());
        return Ok();
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin,Support")]
    [SwaggerOperation(
        Summary = "Получение всех платежей",
        Description = "Возвращает список всех платежей в системе (только для администраторов и поддержки)",
        OperationId = "GetAllPayments"
    )]
    [SwaggerResponse(200, "Список платежей успешно получен", typeof(IEnumerable<PaymentDto>))]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll()
    {
        var result = await service.GetAllPayments();
        
        if (result.IsFailure) return BadRequest(result.Error.Message);
            
        return Ok(result.Value);
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Получение платежей пользователя",
        Description = "Возвращает список платежей текущего пользователя",
        OperationId = "GetUserPayments"
    )]
    [SwaggerResponse(200, "Список платежей успешно получен", typeof(IEnumerable<PaymentDto>))]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetUserPayments()
    {
        var userId = User.GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();
        
        var result = await service.GetUserPayments(userId);
        
        if (result.IsFailure) return BadRequest(result.Error.Message);
            
        return Ok(result.Value);
    }
    
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Получение платежа по ID",
        Description = "Возвращает информацию о платеже по его идентификатору",
        OperationId = "GetPaymentById"
    )]
    [SwaggerResponse(200, "Платеж успешно найден", typeof(PaymentDto))]
    [SwaggerResponse(404, "Платеж не найден")]
    public async Task<ActionResult<PaymentDto>> GetById(Guid id)
    {
        var result = await service.GetPaymentById(id);
        
        if (result.IsFailure)
        {
            if (result.Error.Code == ErrorCode.PaymentNotFound) return NotFound(result.Error.Message);
                
            return BadRequest(result.Error.Message);
        }
        
        return Ok(result.Value);
    }
    
    [Authorize(Roles = "Admin,Support")]
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Удаление платежа",
        Description = "Удаляет платеж по его идентификатору (только для администраторов и поддержки)",
        OperationId = "DeletePayment"
    )]
    [SwaggerResponse(200, "Платеж успешно удален", typeof(PaymentDto))]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    [SwaggerResponse(404, "Платеж не найден")]
    public async Task<ActionResult<PaymentDto>> Delete(Guid id)
    {
        var result = await service.DeletePayment(id);
        
        if (result.IsFailure)
        {
            if (result.Error.Code == ErrorCode.PaymentNotFound) return NotFound(result.Error.Message);
                
            return BadRequest(result.Error.Message);
        }
        
        logger.LogInformation("Удаление платежа {paymentId} [{UserId}]", result.Value.Id, User.GetCurrentUserId());
        return Ok(result.Value);
    }
}