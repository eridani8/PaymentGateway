using FluentValidation;
using PaymentGateway.Core.Enums;

namespace PaymentGateway.Application.Validators;

public static class Validators
{
    public static IRuleBuilderOptions<T, string> ValidFullName<T>(IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty()
            .WithMessage("Требуется указать имя")
            .Length(10, 70).WithMessage("Имя должно быть от 10 до 70 символов")
            .Matches(@"^[А-Яа-яЁёA-Za-z\s]+$").WithMessage("Имя должно содержать только буквы и пробелы")
            .Must(name => name.Split(' ').Length == 3).WithMessage("ФИО должно состоять из 3 слов");
    }
    
    public static IRuleBuilderOptions<T, TEnum> ValidEnumValue<T, TEnum>(IRuleBuilder<T, TEnum> rule) where TEnum : Enum
    {
        return rule.Must(x => 
                Enum.IsDefined(typeof(TEnum), x) &&
                (int)(object)x >= 0 &&
                (int)(object)x < Enum.GetValues(typeof(TEnum)).Length &&
                IsInteger(x)
            ).WithMessage("Значение должно быть значением из перечисления и быть числом");
    }

    private static bool IsInteger<TEnum>(TEnum value)
    {
        return value is int;
    }

    public static IRuleBuilderOptions<T, string> ValidPaymentData<T>(IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty()
            .WithMessage("Требуются указать данные для оплаты")
            .Length(10, 19).WithMessage("Данные о платежах должны быть от 10 до 19 символов")
            .Matches(@"^\+?\d{10,15}$").WithMessage("Данные для оплаты должны быть числовыми и могут начинаться с символа '+' для номера телефона")
            .Matches(@"^\d{13,19}$").WithMessage("Номер банковской карты должен содержать только цифры и быть длиной от 13 до 19 символов");
    }
    
    public static IRuleBuilderOptions<T, decimal> ValidMoneyAmount<T>(IRuleBuilder<T, decimal> rule)
    {
        const decimal maxAmount = 9999999999999999.99m;
        return rule
            .GreaterThan(0).WithMessage("Сумма должна быть больше 0")
            .LessThanOrEqualTo(maxAmount).WithMessage($"Сумма должна быть меньше или равна {maxAmount:N2}")
            .Must(amount => amount == Math.Round(amount, 2)).WithMessage("Сумма должна быть округлена до двух знаков после запятой");
    }
    
    public static IRuleBuilderOptions<T, int> ValidCooldown<T>(IRuleBuilder<T, int> rule)
    {
        const int maxCooldown = 999999999;
        return rule
            .GreaterThan(0).WithMessage("Задержка должна быть больше 0")
            .LessThanOrEqualTo(maxCooldown).WithMessage($"Задержка должна быть меньше или равна {maxCooldown}");
    }
    
    public static IRuleBuilderOptions<T, int> ValidPriority<T>(IRuleBuilder<T, int> rule)
    {
        const int maxCooldown = 999999999;
        return rule
            .GreaterThan(0).WithMessage("Задержка должна быть больше 0")
            .LessThanOrEqualTo(maxCooldown).WithMessage($"Задержка должна быть меньше или равна {maxCooldown}");
    }
}