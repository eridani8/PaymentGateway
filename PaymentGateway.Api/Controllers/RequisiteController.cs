using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Shared.DTOs.Requisite;
using Swashbuckle.AspNetCore.Annotations;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Produces("application/json")]
[Authorize]
[SwaggerTag("Управление реквизитами")]
public class RequisiteController(IRequisiteService service, ILogger<RequisiteController> logger)
    : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Создание реквизита",
        Description = "Создает новый реквизит для пользователя",
        OperationId = "CreateRequisite"
    )]
    [SwaggerResponse(200, "Реквизит успешно создан", typeof(RequisiteDto))]
    [SwaggerResponse(400, "Неверные входные данные")]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    public async Task<ActionResult<RequisiteDto>> Create([FromBody] RequisiteCreateDto? dto)
    {
        if (dto is null) return BadRequest();

        try
        {
            var userId = User.GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();
            var requisite = await service.CreateRequisite(dto, userId);
            if (requisite is null) return BadRequest();
            logger.LogInformation("Создание реквизита {requisiteId} [{User}]", requisite.Id, User.GetCurrentUsername());
            return Ok(requisite);
        }
        catch (DuplicateRequisiteException e)
        {
            return BadRequest(e.Message);
        }
        catch (RequisiteLimitExceededException e)
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
        Summary = "Получение всех реквизитов",
        Description = "Возвращает список всех реквизитов в системе (только для администраторов и поддержки)",
        OperationId = "GetAllRequisites"
    )]
    [SwaggerResponse(200, "Список реквизитов успешно получен", typeof(IEnumerable<RequisiteDto>))]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    public async Task<ActionResult<IEnumerable<RequisiteDto>>> GetAll()
    {
        var requisites = await service.GetAllRequisites();
        return Ok(requisites);
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Получение реквизитов пользователя",
        Description = "Возвращает список реквизитов текущего пользователя",
        OperationId = "GetUserRequisites"
    )]
    [SwaggerResponse(200, "Список реквизитов успешно получен", typeof(IEnumerable<RequisiteDto>))]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    public async Task<ActionResult<IEnumerable<RequisiteDto>>> GetUserRequisites()
    {
        var userId = User.GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();
        var requisites = await service.GetUserRequisites(userId);
        return Ok(requisites);
    }
    
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Получение реквизита по ID",
        Description = "Возвращает информацию о реквизите по его идентификатору",
        OperationId = "GetRequisiteById"
    )]
    [SwaggerResponse(200, "Реквизит успешно найден", typeof(RequisiteDto))]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(404, "Реквизит не найден")]
    public async Task<ActionResult<RequisiteDto>> GetById(Guid id)
    {
        var userId = User.GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();
        var requisite = await service.GetRequisiteById(id);
        if (requisite is null) return NotFound();
        return Ok(requisite);
    }
    
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        Summary = "Обновление реквизита",
        Description = "Обновляет информацию о реквизите",
        OperationId = "UpdateRequisite"
    )]
    [SwaggerResponse(200, "Реквизит успешно обновлен", typeof(RequisiteDto))]
    [SwaggerResponse(400, "Неверные входные данные")]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    public async Task<ActionResult<RequisiteDto>> Update(Guid id, [FromBody] RequisiteUpdateDto? dto)
    {
        if (dto is null) return BadRequest();

        try
        {
            var requisite = await service.UpdateRequisite(id, dto);
            if (requisite is null) return BadRequest();
            logger.LogInformation("Обновление реквизита {requisiteId} [{User}]", requisite.Id, User.GetCurrentUsername());
            return Ok(requisite);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors.GetErrors());
        }
    }
    
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Удаление реквизита",
        Description = "Удаляет реквизит по его идентификатору",
        OperationId = "DeleteRequisite"
    )]
    [SwaggerResponse(200, "Реквизит успешно удален", typeof(RequisiteDto))]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(404, "Реквизит не найден")]
    public async Task<ActionResult<RequisiteDto>> Delete(Guid id)
    {
        var requisite = await service.DeleteRequisite(id);
        if (requisite is null) return NotFound();
        logger.LogInformation("Удаление реквизита {requisiteId} [{User}]", id, User.GetCurrentUsername());
        return Ok(requisite);
    }
}