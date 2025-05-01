using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Results;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Enums;
using PaymentGateway.Shared.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
[SwaggerTag("Административные методы управления пользователями и системой")]
public class AdminController(
    IAdminService service,
    ILogger<AdminController> logger,
    INotificationService notificationService) : ControllerBase
{
    [HttpPost("users")]
    [SwaggerOperation(
        Summary = "Создание нового пользователя",
        Description = "Создает нового пользователя в системе",
        OperationId = "CreateUser"
    )]
    [SwaggerResponse(200, "Пользователь успешно создан", typeof(UserDto))]
    [SwaggerResponse(400, "Неверные входные данные")]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    [SwaggerResponse(409, "Пользователь с таким именем уже существует")]
    public async Task<ActionResult<Guid>> CreateUser([FromBody] CreateUserDto? dto)
    {
        if (dto is null) return BadRequest();

        var result = await service.CreateUser(dto);
        
        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.Validation => BadRequest(result.Error.Message),
                ErrorCode.UserAlreadyExists => Conflict(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };
        }
        
        logger.LogInformation("Создание пользователя {username} [{User}]", dto.Username, User.GetCurrentUsername());
        return Ok(result.Value.Id);
    }

    [HttpGet("users")]
    [SwaggerOperation(
        Summary = "Получение списка всех пользователей",
        Description = "Возвращает список всех пользователей системы",
        OperationId = "GetAllUsers"
    )]
    [SwaggerResponse(200, "Список пользователей успешно получен", typeof(List<UserDto>))]
    [SwaggerResponse(400, "Ошибка при получении списка пользователей")]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        var result = await service.GetAllUsers();
        
        if (result.IsFailure)
        {
            return BadRequest(result.Error.Message);
        }
        
        return Ok(result.Value);
    }

    [HttpGet("users/{id:guid}")]
    [SwaggerOperation(
        Summary = "Получение пользователя по ID",
        Description = "Возвращает информацию о пользователе по его идентификатору",
        OperationId = "GetUserById"
    )]
    [SwaggerResponse(200, "Пользователь успешно найден", typeof(UserDto))]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    [SwaggerResponse(404, "Пользователь не найден")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id)
    {
        var result = await service.GetUserById(id);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.UserNotFound => NotFound(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };
        }

        return Ok(result.Value);
    }

    [HttpDelete("users/{id:guid}")]
    [SwaggerOperation(
        Summary = "Удаление пользователя",
        Description = "Удаляет пользователя из системы по его идентификатору",
        OperationId = "DeleteUser"
    )]
    [SwaggerResponse(200, "Пользователь успешно удален")]
    [SwaggerResponse(400, "Неверные входные данные")]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    [SwaggerResponse(404, "Пользователь не найден")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await service.DeleteUser(id, currentUserId);
        
        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.UserNotFound => NotFound(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };
        }
        
        logger.LogInformation("Удаление пользователя {username} [{User}]", result.Value.UserName,
            User.GetCurrentUsername());
        return Ok();
    }

    [HttpPut("users")]
    [SwaggerOperation(
        Summary = "Обновление данных пользователя",
        Description = "Обновляет информацию о пользователе",
        OperationId = "UpdateUser"
    )]
    [SwaggerResponse(200, "Данные пользователя успешно обновлены", typeof(UserDto))]
    [SwaggerResponse(400, "Неверные входные данные")]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    [SwaggerResponse(404, "Пользователь не найден")]
    public async Task<ActionResult> UpdateUser([FromBody] UpdateUserDto? dto)
    {
        if (dto is null) return BadRequest();

        var result = await service.UpdateUser(dto);
        
        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.Validation => BadRequest(result.Error.Message),
                ErrorCode.UserNotFound => NotFound(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };
        }
        
        logger.LogInformation("Обновление пользователя {username} [{User}]", result.Value.Username,
            User.GetCurrentUsername());
        return Ok();
    }

    [HttpGet("users/roles")]
    [SwaggerOperation(
        Summary = "Получение ролей пользователей",
        Description = "Возвращает роли для списка пользователей по их идентификаторам",
        OperationId = "GetUsersRoles"
    )]
    [SwaggerResponse(200, "Роли пользователей успешно получены", typeof(Dictionary<Guid, string>))]
    [SwaggerResponse(400, "Ошибка при получении ролей пользователей")]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    public async Task<ActionResult<Dictionary<Guid, string>>> GetUsersRoles([FromQuery] string userIds)
    {
        var ids = userIds.Split(',')
            .Select(Guid.Parse)
            .ToList();

        var result = await service.GetUsersRoles(ids);
            
        if (result.IsFailure)
        {
            return BadRequest(result.Error.Message);
        }
            
        return Ok(result.Value);
    }

    [HttpPut("users/{userId:guid}/reset-2fa")]
    [SwaggerOperation(
        Summary = "Сброс двухфакторной аутентификации",
        Description = "Сбрасывает настройки двухфакторной аутентификации для указанного пользователя",
        OperationId = "ResetTwoFactor"
    )]
    [SwaggerResponse(200, "Двухфакторная аутентификация успешно сброшена")]
    [SwaggerResponse(400, "Неверные входные данные")]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    [SwaggerResponse(404, "Пользователь не найден")]
    public async Task<ActionResult> ResetTwoFactor(Guid userId)
    {
        var result = await service.ResetTwoFactor(userId);
        
        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                ErrorCode.UserNotFound => NotFound(result.Error.Message),
                _ => BadRequest(result.Error.Message)
            };
        }
        
        logger.LogInformation("Сброс двухфакторной аутентификации для пользователя {userId} [{User}]", userId,
            User.GetCurrentUsername());
        return Ok();
    }
    
    [HttpGet("requisite-assignment-algorithm")]
    [SwaggerOperation(
        Summary = "Получение текущего алгоритма назначения реквизитов",
        Description = "Возвращает текущий алгоритм, используемый для назначения реквизитов",
        OperationId = "GetCurrentRequisiteAssignmentAlgorithm"
    )]
    [SwaggerResponse(200, "Алгоритм успешно получен")]
    [SwaggerResponse(400, "Ошибка при получении алгоритма")]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    public ActionResult<int> GetCurrentRequisiteAssignmentAlgorithm()
    {
        var result = service.GetCurrentRequisiteAssignmentAlgorithm();
        
        if (result.IsFailure)
        {
            return BadRequest(result.Error.Message);
        }
        
        return Ok(result.Value);
    }

    [HttpPut]
    [SwaggerOperation(
        Summary = "Изменение алгоритма назначения реквизитов",
        Description = "Устанавливает новый алгоритм для назначения реквизитов",
        OperationId = "SetRequisiteAssignmentAlgorithm"
    )]
    [SwaggerResponse(200, "Алгоритм успешно изменен")]
    [SwaggerResponse(400, "Неверные входные данные")]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    public ActionResult SetRequisiteAssignmentAlgorithm([FromBody] int algorithm)
    {
        var result = service.SetRequisiteAssignmentAlgorithm(algorithm);
        
        if (result.IsFailure)
        {
            return BadRequest(result.Error.Message);
        }

        var oldAlgorithm = (RequisiteAssignmentAlgorithm)service.GetCurrentRequisiteAssignmentAlgorithm().Value;
        notificationService.NotifyRequisiteAssignmentAlgorithmChanged((RequisiteAssignmentAlgorithm)algorithm);
        
        logger.LogInformation("Изменение алгоритма подбора реквизитов. С {old} на {new} [{User}]", oldAlgorithm, (RequisiteAssignmentAlgorithm)algorithm, User.GetCurrentUsername());

        return Ok();
    }
}