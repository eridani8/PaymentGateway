using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Core.Entities;

public abstract class BaseEntity
{
    [Key]
    public Guid Id { get; init; }
} 