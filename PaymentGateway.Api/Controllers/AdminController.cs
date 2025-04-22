using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Authorize(Roles = "Admin")]
public class AdminController(IAdminService service, ILogger<AdminController> logger) : ControllerBase
{
    [HttpPost]
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
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        var users = await service.GetAllUsers();
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
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
    public async Task<ActionResult<Dictionary<Guid, string>>> GetUsersRoles([FromQuery] string userIds)
    {
        var ids = userIds.Split(',')
            .Select(Guid.Parse)
            .ToList();

        var roles = await service.GetUsersRoles(ids);
        return Ok(roles);
    }

    [HttpPost("{userId:guid}")]
    public async Task<ActionResult> ResetTwoFactor(Guid userId)
    {
        var result = await service.ResetTwoFactorAsync(userId);
        if (!result) return BadRequest();
        logger.LogInformation("Сброс двухфакторной аутентификации для пользователя {userId} [{User}]", userId, User.GetCurrentUsername());
        
        return Ok();
    }
}