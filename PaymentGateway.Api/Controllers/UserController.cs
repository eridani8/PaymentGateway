using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application;
using PaymentGateway.Core.Entities;
using PaymentGateway.Core.Interfaces;
using PaymentGateway.Shared;
using PaymentGateway.Shared.DTOs.User;
using Swashbuckle.AspNetCore.Annotations;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]/[action]")]
[Produces("application/json")]
[SwaggerTag("Пользовательские методы и аутентификация")]
public class UserController(
    UserManager<UserEntity> userManager,
    SignInManager<UserEntity> signInManager,
    ITokenService tokenService,
    IValidator<LoginDto> loginValidator, 
    IValidator<ChangePasswordDto> changePasswordValidator,
    IValidator<TwoFactorVerifyDto> twoFactorVerifyValidator) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Аутентификация пользователя",
        Description = "Выполняет вход пользователя в систему. При включенной двухфакторной аутентификации требует дополнительного кода.",
        OperationId = "Login"
    )]
    [SwaggerResponse(200, "Успешная аутентификация, возвращает JWT-токен", typeof(string))]
    [SwaggerResponse(400, "Неверные входные данные")]
    [SwaggerResponse(401, "Неверные учетные данные")]
    [SwaggerResponse(428, "Требуется двухфакторная аутентификация")]
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

        switch (user.TwoFactorEnabled)
        {
            case true when string.IsNullOrEmpty(model.TwoFactorCode):
                return StatusCode(428);
            case true when !string.IsNullOrEmpty(model.TwoFactorCode):
            {
                var isValid = TotpService.VerifyTotpCode(user.TwoFactorSecretKey ?? string.Empty, model.TwoFactorCode);
                if (!isValid)
                {
                    return Unauthorized("Неверный код аутентификации");
                }

                break;
            }
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = tokenService.GenerateJwtToken(user, roles);
        
        return Ok(token);
    }
    
    [HttpPut]
    [Authorize]
    [SwaggerOperation(
        Summary = "Изменение пароля",
        Description = "Позволяет пользователю изменить свой пароль",
        OperationId = "ChangePassword"
    )]
    [SwaggerResponse(200, "Пароль успешно изменен")]
    [SwaggerResponse(400, "Неверные входные данные")]
    [SwaggerResponse(401, "Пользователь не авторизован")]
    [SwaggerResponse(404, "Пользователь не найден")]
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
    [SwaggerOperation(
        Summary = "Статус двухфакторной аутентификации",
        Description = "Возвращает информацию о состоянии двухфакторной аутентификации пользователя",
        OperationId = "TwoFactorStatus"
    )]
    [SwaggerResponse(200, "Успешное получение статуса", typeof(TwoFactorStatusDto))]
    [SwaggerResponse(404, "Пользователь не найден")]
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
    
    [HttpPut]
    [SwaggerOperation(
        Summary = "Включение двухфакторной аутентификации",
        Description = "Генерирует QR-код и секретный ключ для настройки двухфакторной аутентификации",
        OperationId = "EnableTwoFactor"
    )]
    [SwaggerResponse(200, "Успешная генерация данных", typeof(TwoFactorDto))]
    [SwaggerResponse(404, "Пользователь не найден")]
    public async Task<ActionResult<TwoFactorDto>> EnableTwoFactor()
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
    
    [HttpPut]
    [SwaggerOperation(
        Summary = "Проверка кода двухфакторной аутентификации",
        Description = "Проверяет введенный код двухфакторной аутентификации",
        OperationId = "VerifyTwoFactor"
    )]
    [SwaggerResponse(200, "Код успешно проверен")]
    [SwaggerResponse(400, "Неверный код подтверждения")]
    [SwaggerResponse(404, "Пользователь не найден")]
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