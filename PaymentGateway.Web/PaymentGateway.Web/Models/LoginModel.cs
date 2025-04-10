using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Web.Models;

public class LoginModel
{
    [Required(ErrorMessage = "Введите логин")]
    [StringLength(50, MinimumLength = 4, ErrorMessage = "Логин должен содержать от 4 до 50 символов")]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Введите пароль")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
    public string Password { get; set; } = string.Empty;
}