using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared.Models;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Produces("application/json")]
public class AuthController(
    UserManager<UserEntity> userManager,
    SignInManager<UserEntity> signInManager,
    ITokenService tokenService,
    IValidator<LoginModel> validator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginModel? model)
    {
        if (model is null) return BadRequest("Неверные данные");

        var validation = await validator.ValidateAsync(model);
        if (!validation.IsValid)
        {
            return BadRequest(validation.Errors.GetErrors());
        }
            
        var user = await userManager.FindByNameAsync(model.Username);
        if (user == null)
        {
            return Unauthorized("Неверное имя пользователя или пароль");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
        {
            return Unauthorized("Неверное имя пользователя или пароль");
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = tokenService.GenerateJwtToken(user, roles);
        
        return Ok(token);
    }
}