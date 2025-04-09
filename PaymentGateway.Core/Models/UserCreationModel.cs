using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Core.Models;

public class UserCreationModel
{
    [Required]
    [MinLength(3)]
    [MaxLength(50)]
    public string Username { get; set; }
    
    [Required]
    [MinLength(6)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public List<string> Roles { get; set; } = [];
}