using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Results;
using PaymentGateway.Shared.DTOs.Requisite;
using Swashbuckle.AspNetCore.Annotations;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]/[action]")]
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
    [SwaggerResponse(409, "Реквизит с такими платежными данными уже существует")]
    public async Task<ActionResult<Guid>> Create([FromBody] RequisiteCreateDto? dto)
    {
        if (dto is null) return BadRequest();

        var userId = User.GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();
        
        var result = await service.CreateRequisite(dto, userId);
        
        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.DuplicateRequisite => Conflict(result.Error.Message),
                ErrorCode.RequisiteLimitExceeded => BadRequest(result.Error.Message),
                ErrorCode.Validation => BadRequest(result.Error.Message),
                ErrorCode.UserNotFound => NotFound(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };
        }
        
        logger.LogInformation("Создание реквизита {requisiteId} [{User}]", result.Value.Id, User.GetCurrentUsername());
        return Ok(result.Value.Id);
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
        var result = await service.GetAllRequisites();
        
        if (result.IsFailure) return BadRequest(result.Error.Message);
        
        return Ok(result.Value);
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
        
        var result = await service.GetUserRequisites(userId);
        
        if (result.IsFailure) return BadRequest(result.Error.Message);
        
        return Ok(result.Value);
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
        
        var result = await service.GetRequisiteById(id);
        
        if (result.IsFailure)
        {
            if (result.Error.Code == ErrorCode.RequisiteNotFound) return NotFound(result.Error.Message);
            
            return BadRequest(result.Error.Message);
        }
        
        return Ok(result.Value);
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
    [SwaggerResponse(404, "Реквизит не найден")]
    public async Task<ActionResult> Update(Guid id, [FromBody] RequisiteUpdateDto? dto)
    {
        if (dto is null) return BadRequest();
        
        var result = await service.UpdateRequisite(id, dto);
        
        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.RequisiteNotFound => NotFound(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };
        }
        
        logger.LogInformation("Обновление реквизита {requisiteId} [{User}]", result.Value.Id, User.GetCurrentUsername());
        return Ok();
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
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await service.DeleteRequisite(id);
        
        if (result.IsFailure)
        {
            if (result.Error.Code == ErrorCode.RequisiteNotFound) return NotFound(result.Error.Message);
            
            return BadRequest(result.Error.Message);
        }
        
        logger.LogInformation("Удаление реквизита {requisiteId} [{User}]", result.Value.Id, User.GetCurrentUsername());
        return Ok(result.Value);
    }
}