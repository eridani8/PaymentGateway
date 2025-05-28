using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Core.Entities;

public class ConfigEntity : BaseEntity
{
    [MaxLength(255)] public required string Key { get; set; }
    [MaxLength(9999)] public required string Value { get; set; }
}