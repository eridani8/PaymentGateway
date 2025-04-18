﻿using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared.DTOs.User;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Produces("application/json")]
public class UserController(
    UserManager<UserEntity> userManager,
    SignInManager<UserEntity> signInManager,
    ITokenService tokenService,
    IValidator<LoginDto> loginValidator, 
    IValidator<ChangePasswordDto> changePasswordValidator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginDto? model)
    {
        if (model is null) return BadRequest();

        var validation = await loginValidator.ValidateAsync(model);
        if (!validation.IsValid)
        {
            return BadRequest(validation.Errors.GetErrors());
        }
            
        var user = await userManager.FindByNameAsync(model.Username);
        if (user == null)
        {
            return Unauthorized("Неверный логин или пароль");
        }

        if (!user.IsActive)
        {
            return Unauthorized("Пользователь деактивирован");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
        {
            return Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = tokenService.GenerateJwtToken(user, roles);
        
        return Ok(token);
    }
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto? model)
    {
        if (model is null) return BadRequest();

        var validation = await changePasswordValidator.ValidateAsync(model);
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

        return BadRequest(result.Errors.GetErrors());
    }
}