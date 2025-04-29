using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Enums;
using PaymentGateway.Shared.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Authorize(Roles = "Admin")]
[SwaggerTag("Административные методы управления пользователями и системой")]
public class AdminController(
    IAdminService service,
    ILogger<AdminController> logger,
    INotificationService notificationService) : ControllerBase
{
    [HttpPost]
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
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto? dto)
    {
        if (dto is null) return BadRequest();

        try
        {
            var user = await service.CreateUser(dto);
            if (user is null) return BadRequest();
            logger.LogInformation("Создание пользователя {username} [{User}]", dto.Username, User.GetCurrentUsername());
            return Ok(user);
        }
        catch (DuplicateUserException)
        {
            return Conflict();
        }
        catch (CreateUserException e)
        {
            return BadRequest(e.Message);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors.GetErrors());
        }
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Получение списка всех пользователей",
        Description = "Возвращает список всех пользователей системы",
        OperationId = "GetAllUsers"
    )]
    [SwaggerResponse(200, "Список пользователей успешно получен", typeof(List<UserDto>))]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        var users = await service.GetAllUsers();
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
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
        var user = await service.GetUserById(id);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Удаление пользователя",
        Description = "Удаляет пользователя из системы по его идентификатору",
        OperationId = "DeleteUser"
    )]
    [SwaggerResponse(200, "Пользователь успешно удален")]
    [SwaggerResponse(400, "Неверные входные данные")]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    public async Task<ActionResult<bool>> DeleteUser(Guid id)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await service.DeleteUser(id, currentUserId);
            if (result is null) return BadRequest();
            logger.LogInformation("Удаление пользователя {username} [{User}]", result.UserName,
                User.GetCurrentUsername());
            return Ok(true);
        }
        catch (DeleteUserException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut]
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
    public async Task<ActionResult<UserDto>> UpdateUser([FromBody] UpdateUserDto? dto)
    {
        if (dto is null) return BadRequest();

        try
        {
            var user = await service.UpdateUser(dto);
            if (user is null) return NotFound();
            logger.LogInformation("Обновление пользователя {username} [{User}]", user.Username,
                User.GetCurrentUsername());
            return Ok(user);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors.GetErrors());
        }
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Получение ролей пользователей",
        Description = "Возвращает роли для списка пользователей по их идентификаторам",
        OperationId = "GetUsersRoles"
    )]
    [SwaggerResponse(200, "Роли пользователей успешно получены", typeof(Dictionary<Guid, string>))]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    public async Task<ActionResult<Dictionary<Guid, string>>> GetUsersRoles([FromQuery] string userIds)
    {
        var ids = userIds.Split(',')
            .Select(Guid.Parse)
            .ToList();

        var roles = await service.GetUsersRoles(ids);
        return Ok(roles);
    }

    [HttpPut("{userId:guid}")]
    [SwaggerOperation(
        Summary = "Сброс двухфакторной аутентификации",
        Description = "Сбрасывает настройки двухфакторной аутентификации для указанного пользователя",
        OperationId = "ResetTwoFactor"
    )]
    [SwaggerResponse(200, "Двухфакторная аутентификация успешно сброшена")]
    [SwaggerResponse(400, "Неверные входные данные")]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    public async Task<ActionResult> ResetTwoFactor(Guid userId)
    {
        var result = await service.ResetTwoFactorAsync(userId);
        if (!result) return BadRequest();
        logger.LogInformation("Сброс двухфакторной аутентификации для пользователя {userId} [{User}]", userId,
            User.GetCurrentUsername());

        return Ok();
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Получение текущего алгоритма назначения реквизитов",
        Description = "Возвращает текущий алгоритм, используемый для назначения реквизитов",
        OperationId = "GetCurrentRequisiteAssignmentAlgorithm"
    )]
    [SwaggerResponse(200, "Алгоритм успешно получен")]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(403, "Недостаточно прав")]
    public ActionResult<int> GetCurrentRequisiteAssignmentAlgorithm()
    {
        var algorithm = service.GetCurrentRequisiteAssignmentAlgorithm();
        return Ok(algorithm);
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
    public ActionResult<bool> SetRequisiteAssignmentAlgorithm([FromBody] int algorithm)
    {
        if (!Enum.IsDefined(typeof(RequisiteAssignmentAlgorithm), algorithm))
        {
            return BadRequest("Указан недопустимый алгоритм");
        }

        var old = (RequisiteAssignmentAlgorithm)service.GetCurrentRequisiteAssignmentAlgorithm();

        service.SetRequisiteAssignmentAlgorithm(algorithm);

        notificationService.NotifyRequisiteAssignmentAlgorithmChanged((RequisiteAssignmentAlgorithm)algorithm);

        logger.LogInformation("Изменение алгоритма подбора реквизитов. С {old} на {new} [{User}]", old, (RequisiteAssignmentAlgorithm)algorithm, User.GetCurrentUsername());

        return Ok(true);
    }
}