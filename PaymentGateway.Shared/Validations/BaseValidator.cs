﻿using FluentValidation;

namespace PaymentGateway.Shared.Validations;

public abstract class BaseValidator<T> : AbstractValidator<T>
{
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue
    {
        get
        {
            return async (model, propertyName) =>
            {
                var result =
                    await ValidateAsync(ValidationContext<T>.CreateWithOptions((T)model,
                        x => x.IncludeProperties(propertyName)));
                return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
            };
        }
    }
}