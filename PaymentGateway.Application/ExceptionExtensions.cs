using FluentValidation.Results;

namespace PaymentGateway.Application;

public static class ExceptionExtensions
{
    public static string GetErrors(this IEnumerable<ValidationFailure> errors)
    {
        return string.Join(", ", errors.Select(f => f.ErrorMessage));
    }
}