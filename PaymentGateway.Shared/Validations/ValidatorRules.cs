using FluentValidation;

namespace PaymentGateway.Shared.Validations;

public static class ValidatorRules
{
    public static IRuleBuilderOptions<T, Guid> ValidGuid<T>(this IRuleBuilder<T, Guid> rule)
    {
        return rule.Must(x => x != Guid.Empty)
            .WithMessage("Нужно задать значение");
    }

    public static IRuleBuilderOptions<T, Guid?> ValidGuid<T>(this IRuleBuilder<T, Guid?> rule)
    {
        return rule
            .Must(x => !x.HasValue || x.Value != Guid.Empty)
            .WithMessage("Значение должно быть Guid");
    }

    public static IRuleBuilderOptions<T, string> ValidFullName<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("Введите имя")
            .Length(4, 40).WithMessage("Имя и фамилия должны быть от 4 до 40 символов")
            .Matches(ValidationRegexps.FullNameRegex())
            .WithMessage("Имя и фамилия должна содержать только буквы и пробелы");
    }

    public static IRuleBuilderOptions<T, string> ValidPhoneNumber<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .Matches(ValidationRegexps.PhoneRegex())
            .WithMessage(
                "Неверный формат номера телефона. Номер должен содержать от 10 до 15 цифр и может начинаться с '+'");
    }

    public static IRuleBuilderOptions<T, string> ValidCreditCardNumber<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .Matches(ValidationRegexps.CreditCardRegex())
            .WithMessage("Неверный формат номера банковской карты. Номер должен содержать от 13 до 19 цифр");
    }

    public static IRuleBuilderOptions<T, string> ValidBankAccount<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .Matches(ValidationRegexps.BankAccountRegex())
            .WithMessage("Неверный формат банковского счета. Счет должен содержать от 8 до 34 цифр.");
    }

    public static IRuleBuilderOptions<T, TEnum> ValidEnumValue<T, TEnum>(this IRuleBuilder<T, TEnum> rule)
        where TEnum : Enum
    {
        return rule.Must(x =>
            Enum.IsDefined(typeof(TEnum), x) &&
            (int)(object)x >= 0 &&
            (int)(object)x < Enum.GetValues(typeof(TEnum)).Length
        ).WithMessage("Значение должно быть значением из перечисления и быть числом");
    }

    public static IRuleBuilderOptions<T, decimal> ValidMoneyAmount<T>(this IRuleBuilder<T, decimal> rule)
    {
        const decimal maxAmount = 9999999999999999m;
        return rule
            .GreaterThan(0).WithMessage("Сумма должна быть больше 0")
            .LessThanOrEqualTo(maxAmount).WithMessage($"Сумма должна быть меньше или равна {maxAmount:N0}")
            .Must(amount => amount == Math.Floor(amount))
            .WithMessage("Сумма должна быть целым числом без десятичной части");
    }

    public static IRuleBuilderOptions<T, TimeSpan> ValidCooldown<T>(this IRuleBuilder<T, TimeSpan> rule)
    {
        var maxCooldown = TimeSpan.FromHours(24);
        return rule
            .LessThanOrEqualTo(maxCooldown).WithMessage($"Задержка должна быть меньше или равна {maxCooldown}")
            .WithMessage("Задержка должна быть больше 0 или равна нулю");
    }

    public static IRuleBuilderOptions<T, int> ValidPriority<T>(this IRuleBuilder<T, int> rule)
    {
        const int maxPriority = 100;
        return rule
            .GreaterThan(0).WithMessage("Приоритет должен быть больше 0")
            .LessThanOrEqualTo(maxPriority).WithMessage($"Приоритет должен быть меньше или равен {maxPriority}");
    }

    public static IRuleBuilderOptions<T, int> ValidNumber<T>(this IRuleBuilder<T, int> rule)
    {
        return rule
            .LessThanOrEqualTo(int.MaxValue).WithMessage($"Число должно быть меньше или равно {int.MaxValue}");
    }

    public static IRuleBuilderOptions<T, string> ValidUsername<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("Введите логин")
            .MinimumLength(4).WithMessage("Минимальная длина логина 4 символа")
            .MaximumLength(50).WithMessage("Максимальная длина логина 50 символов")
            .Matches(ValidationRegexps.LoginRegex())
            .WithMessage("Логин должен начинаться с буквы и содержать только латинские буквы, цифры и подчёркивания");
    }

    public static IRuleBuilderOptions<T, string> ValidPassword<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("Введите пароль")
            .MinimumLength(6).WithMessage("Минимальная длина пароля 6 символов")
            .MaximumLength(100).WithMessage("Максимальная длина пароля 100 символов")
            .Matches("[A-Z]").WithMessage("Пароль должен содержать хотя бы одну заглавную букву")
            .Matches("[a-z]").WithMessage("Пароль должен содержать хотя бы одну строчную букву")
            .Matches("[0-9]").WithMessage("Пароль должен содержать хотя бы одну цифру")
            .Matches("[^a-zA-Z0-9]").WithMessage("Пароль должен содержать хотя бы один специальный символ");
    }

    public static IRuleBuilderOptions<T, List<string>> ValidRoles<T>(this IRuleBuilder<T, List<string>> rule)
    {
        var allowedValues = new[] { "User", "Admin", "Support" };

        return rule
            .NotEmpty().WithMessage("Обязательное поле")
            .Must(roles => roles.All(role => allowedValues.Contains(role)))
            .WithMessage("Роли содержат недопустимые значения")
            .Must(roles => roles.Intersect(allowedValues).Any())
            .WithMessage("Требуется хотя бы одна роль");
    }

    public static IRuleBuilderOptions<T, string> ValidAuthCode<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("Введите код аутентификации")
            .Length(6).WithMessage("Код должен содержать 6 цифр");
    }
}