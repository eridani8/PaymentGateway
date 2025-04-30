using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Exceptions;
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

        try
        {
            var payment = await service.CreatePayment(dto);
            if (payment is null) return BadRequest();
            logger.LogInformation("Создание платежа {paymentId} на сумму {amount}", payment.Id, payment.Amount);
            return Ok(payment.Id);
        }
        catch (DuplicatePaymentException)
        {
            return Conflict();
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors.GetErrors());
        }
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

        try
        {
            var userId = User.GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();
            var payment = await service.ManualConfirmPayment(dto, userId);
            logger.LogInformation("Ручное подтверждение платежа {paymentId} [{User}]", payment.Id, User.GetCurrentUsername());
            return Ok();
        }
        catch (PaymentNotFound)
        {
            return NotFound();
        }
        catch (ManualConfirmException e)
        {
            return BadRequest(e.Message);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors.GetErrors());
        }
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

        try
        {
            var userId = User.GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();
            var payment = await service.CancelPayment(dto, userId);
            logger.LogInformation("Отмена платежа {paymentId} [{User}]", payment.Id, User.GetCurrentUsername());
            return Ok();
        }
        catch (PaymentNotFound)
        {
            return NotFound();
        }
        catch (ManualConfirmException e)
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
        Summary = "Получение всех платежей",
        Description = "Возвращает список всех платежей в системе (только для администраторов и поддержки)",
        OperationId = "GetAllPayments"
    )]
    [SwaggerResponse(200, "Список платежей успешно получен", typeof(IEnumerable<PaymentDto>))]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll()
    {
        var payments = await service.GetAllPayments();
        return Ok(payments);
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
        var payments = await service.GetUserPayments(userId);
        return Ok(payments);
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
        var payment = await service.GetPaymentById(id);
        if (payment is null) return NotFound();
        return Ok(payment);
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
        var payment = await service.DeletePayment(id);
        if (payment is null) return NotFound();
        logger.LogInformation("Удаление платежа {paymentId} [{UserId}]", payment.Id, User.GetCurrentUserId());
        return Ok(payment);
    }
}