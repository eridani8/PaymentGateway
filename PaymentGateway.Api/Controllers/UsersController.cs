using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Application;
using PaymentGateway.Core.Entities;
using PaymentGateway.Shared.Models;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Authorize(Roles = "Admin")]
public class UsersController(
    UserManager<UserEntity> userManager,
    RoleManager<IdentityRole> roleManager,
    IValidator<ChangePasswordModel> validator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingUser = await userManager.FindByNameAsync(model.Username);
        if (existingUser != null)
        {
            return BadRequest("Пользователь с таким именем уже существует");
        }

        if (model.Roles.Count > 0)
        {
            foreach (var role in model.Roles)
            {
                var roleExists = await roleManager.RoleExistsAsync(role);
                if (!roleExists)
                {
                    return BadRequest($"Роль '{role}' не существует");
                }
            }
        }
        else
        {
            return BadRequest("Нужно указать роль");
        }

        var user = new UserEntity()
        {
            UserName = model.Username
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        await userManager.AddToRoleAsync(user, model.Roles.First());

        return Ok(new
        {
            Message = "Пользователь создан",
            UserId = user.Id,
            Roles = await userManager.GetRolesAsync(user)
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await userManager.Users.ToListAsync();
        var userList = new List<object>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            userList.Add(new
            {
                Id = user.Id,
                UserName = user.UserName,
                Roles = roles
            });
        }

        return Ok(userList);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound("Пользователь не найден");
        }

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new
        {
            Id = user.Id,
            UserName = user.UserName,
            Roles = roles
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound("Пользователь не найден");
        }

        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (id == currentUserId)
        {
            return BadRequest("Вы не можете удалить свой собственный аккаунт");
        }

        var result = await userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            return Ok(new { Message = "Пользователь успешно удален" });
        }

        return StatusCode(500);
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel? model)
    {
        if (model is null) return BadRequest("Неверные данные");

        var validation = await validator.ValidateAsync(model);
        if (!validation.IsValid)
        {
            return BadRequest(validation.Errors.GetErrors());
        }
        
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var result = await userManager.ChangePasswordAsync(
            user,
            model.CurrentPassword,
            model.NewPassword);

        if (result.Succeeded)
        {
            return Ok();
        }

        return StatusCode(500);
    }
}