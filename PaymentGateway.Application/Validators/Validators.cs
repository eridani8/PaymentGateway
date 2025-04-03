using System.Text.RegularExpressions;
using FluentValidation;

namespace PaymentGateway.Application.Validators;

public static partial class Validators
{
    public static IRuleBuilderOptions<T, string> ValidFullName<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty()
            .WithMessage("Требуется указать имя")
            .Length(10, 70).WithMessage("ФИО должно быть от 10 до 70 символов")
            .Matches(@"^[А-Яа-яЁёA-Za-z\s]+$").WithMessage("ФИО должно содержать только буквы и пробелы")
            .Must(name => name.Split(' ').Length == 3).WithMessage("ФИО должно состоять из 3 слов");
    }

    public static IRuleBuilderOptions<T, TEnum> ValidEnumValue<T, TEnum>(this IRuleBuilder<T, TEnum> rule) where TEnum : Enum
    {
        return rule.Must(x =>
            Enum.IsDefined(typeof(TEnum), x) &&
            (int)(object)x >= 0 &&
            (int)(object)x < Enum.GetValues(typeof(TEnum)).Length
        ).WithMessage("Значение должно быть значением из перечисления и быть числом");
    }

    private static (bool isValid, string? errorMessage) ValidateAsPhoneNumber(string value)
    {
        if (IsPhoneNumber(value))
        {
            return (true, null);
        }

        return (false,
            "Некорректный формат номера телефона. Номер должен содержать от 10 до 15 цифр и может начинаться с '+'");
    }

    public static IRuleBuilderOptions<T, string> ValidPhoneNumber<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty()
            .WithMessage("Требуется указать номер телефона")
            .Must((_, value) =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    return true;
                }

                var (isValid, _) = ValidateAsPhoneNumber(value);
                return isValid;
            })
            .WithMessage((_, value) =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    return "Требуется указать номер телефона";
                }

                var (_, errorMessage) = ValidateAsPhoneNumber(value);
                return errorMessage;
            });
    }

    private static (bool isValid, string? errorMessage) ValidateAsCreditCard(string value)
    {
        if (IsCreditCardNumber(value))
        {
            return (true, null);
        }

        return (false, "Номер банковской карты должен содержать только цифры и быть длиной от 13 до 19 символов");
    }

    public static IRuleBuilderOptions<T, string> ValidCreditCard<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty()
            .WithMessage("Требуется указать номер банковской карты")
            .Must((_, value) =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    return true;
                }

                var (isValid, _) = ValidateAsCreditCard(value);
                return isValid;
            })
            .WithMessage((_, value) =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    return "Требуется указать номер банковской карты";
                }

                var (_, errorMessage) = ValidateAsCreditCard(value);
                return errorMessage;
            });
    }

    public static IRuleBuilderOptions<T, string> ValidPaymentData<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty()
            .WithMessage("Требуется указать данные для оплаты")
            .Must((_, value) =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    return true;
                }

                if (value.StartsWith('+') || value.Length is >= 10 and <= 15)
                {
                    var (isValid, _) = ValidateAsPhoneNumber(value);
                    return isValid;
                }

                if (value.Length is < 13 or > 19) return false;
                {
                    var (isValid, _) = ValidateAsCreditCard(value);
                    return isValid;
                }
            })
            .WithMessage((_, value) =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    return "Требуется указать данные для оплаты";
                }

                if (value.StartsWith('+') || value.Length is >= 10 and <= 15)
                {
                    var (isValid, errorMessage) = ValidateAsPhoneNumber(value);
                    if (!isValid)
                    {
                        return errorMessage;
                    }
                }
                else if (value.Length >= 13 && value.Length <= 19)
                {
                    var (isValid, errorMessage) = ValidateAsCreditCard(value);
                    if (!isValid)
                    {
                        return errorMessage;
                    }
                }

                return "Данные для оплаты должны быть номером телефона или номером карты";
            });
    }

    public static IRuleBuilderOptions<T, decimal> ValidMoneyAmount<T>(this IRuleBuilder<T, decimal> rule)
    {
        const decimal maxAmount = 9999999999999999.99m;
        return rule
            .GreaterThan(0).WithMessage("Сумма должна быть больше 0")
            .LessThanOrEqualTo(maxAmount).WithMessage($"Сумма должна быть меньше или равна {maxAmount:N2}")
            .Must(amount => amount == Math.Round(amount, 2))
            .WithMessage("Сумма должна быть округлена до двух знаков после запятой");
    }

    public static IRuleBuilderOptions<T, int> ValidCooldown<T>(this IRuleBuilder<T, int> rule)
    {
        const int maxCooldown = 999999999;
        return rule
            .GreaterThan(0).WithMessage("Задержка должна быть больше 0")
            .LessThanOrEqualTo(maxCooldown).WithMessage($"Задержка должна быть меньше или равна {maxCooldown}");
    }

    public static IRuleBuilderOptions<T, int> ValidPriority<T>(this IRuleBuilder<T, int> rule)
    {
        const int maxCooldown = 999999999;
        return rule
            .GreaterThan(0).WithMessage("Задержка должна быть больше 0")
            .LessThanOrEqualTo(maxCooldown).WithMessage($"Задержка должна быть меньше или равна {maxCooldown}");
    }

    [GeneratedRegex(@"^\+?\d{10,15}$")]
    public static partial Regex PhoneRegex();

    [GeneratedRegex(@"^\d{13,19}$")]
    public static partial Regex CreditCardRegex();

    public static bool BePhoneNumberOrCard(string paymentData)
    {
        return IsPhoneNumber(paymentData) || IsCreditCardNumber(paymentData);
    }

    private static bool IsPhoneNumber(string data)
    {
        return PhoneRegex().IsMatch(data);
    }

    private static bool IsCreditCardNumber(string data)
    {
        return CreditCardRegex().IsMatch(data);
    }
}