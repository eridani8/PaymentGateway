using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared;
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
    IValidator<ChangePasswordDto> changePasswordValidator,
    IValidator<TwoFactorVerifyDto> twoFactorVerifyValidator) : ControllerBase
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
            return Unauthorized("Неверный логин или пароль");
        }

        if (user.TwoFactorEnabled && string.IsNullOrEmpty(model.TwoFactorCode))
        {
            return StatusCode(428);
        }

        if (user.TwoFactorEnabled && !string.IsNullOrEmpty(model.TwoFactorCode))
        {
            var isValid = TotpService.VerifyTotpCode(user.TwoFactorSecretKey ?? string.Empty, model.TwoFactorCode);
            if (!isValid)
            {
                return Unauthorized("Неверный код аутентификации");
            }
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
    
    [HttpGet]
    public async Task<IActionResult> TwoFactorStatus()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }
        
        var roles = await userManager.GetRolesAsync(user);
        var isAdmin = roles.Contains("Admin");
        
        var result = new TwoFactorStatusDto
        {
            IsEnabled = user.TwoFactorEnabled,
            IsSetupRequired = isAdmin && !user.TwoFactorEnabled
        };
        
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> EnableTwoFactor()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }
        
        var secretKey = TotpService.GenerateSecretKey();
        
        const string issuer = "PaymentGateway";
        var totpUri = TotpService.GenerateTotpUri(secretKey, user.UserName ?? user.Email ?? user.Id.ToString(), issuer);
        
        var qrCodeImage = TotpService.GenerateQrCodeBase64(totpUri);
        
        user.TwoFactorSecretKey = secretKey;
        await userManager.UpdateAsync(user);
        
        return Ok(new TwoFactorDto
        {
            QrCodeUri = qrCodeImage,
            SharedKey = secretKey
        });
    }
    
    [HttpPost]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] TwoFactorVerifyDto? model)
    {
        if (model is null) return BadRequest();
        
        var validation = await twoFactorVerifyValidator.ValidateAsync(model);
        if (!validation.IsValid)
        {
            return BadRequest(validation.Errors.GetErrors());
        }
        
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }
        
        var isValid = TotpService.VerifyTotpCode(user.TwoFactorSecretKey ?? string.Empty, model.Code);
        if (!isValid)
        {
            return BadRequest("Неверный код подтверждения");
        }
        
        user.TwoFactorEnabled = true;
        await userManager.UpdateAsync(user);
        
        return Ok();
    }
}