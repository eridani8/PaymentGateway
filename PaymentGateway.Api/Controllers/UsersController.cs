﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Models;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Authorize(Roles = "Admin")]
public class UsersController(UserManager<UserEntity> userManager, RoleManager<IdentityRole> roleManager) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserCreationModel model)
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
        
        return Ok(new { 
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
}