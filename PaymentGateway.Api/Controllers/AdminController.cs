using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Core.Exceptions;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Interfaces;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Authorize(Roles = "Admin")]
public class UsersController(IAdminService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto? dto)
    {
        if (dto is null) return BadRequest();

        try
        {
            var user = await service.CreateUser(dto);
            if (user is null) return BadRequest();
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
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var user = await service.DeleteUser(id, currentUserId);
        if (user is null) return NotFound();
        return Ok(true);
    }

    [HttpPut]
    public async Task<ActionResult<UserDto>> UpdateUser([FromBody] UpdateUserDto? dto)
    {
        if (dto is null) return BadRequest();

        try
        {
            var user = await service.UpdateUser(dto);
            if (user is null) return NotFound();
            return Ok(user);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Errors.GetErrors());
        }
    }
}