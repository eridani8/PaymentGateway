using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;

namespace PaymentGateway.Application;

public static class ExceptionExtensions
{
    public static string GetErrors(this IEnumerable<ValidationFailure> errors)
    {
        return string.Join(", ", errors.Select(f => f.ErrorMessage));
    }
    
    public static string GetErrors(this IEnumerable<IdentityError> errors)
    {
        return string.Join(", ", errors.Select(f => f.Description));
    }
}