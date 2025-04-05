using System.Text.RegularExpressions;
using FluentValidation;

namespace PaymentGateway.Application.Validators;

public static partial class ValidatorRules
{
    public static IRuleBuilderOptions<T, Guid> ValidGuid<T>(this IRuleBuilder<T, Guid> rule)
    {
        return rule.Must(x => x != Guid.Empty)
            .WithMessage("Значение должно быть Guid");
    }
    
    public static IRuleBuilderOptions<T, string> ValidFullName<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty()
            .WithMessage("Требуется указать имя")
            .Length(10, 70).WithMessage("ФИО должно быть от 10 до 70 символов")
            .Matches(FullNameRegex()).WithMessage("ФИО должно содержать только буквы и пробелы")
            .Must(name => name.Split(' ').Length == 3).WithMessage("ФИО должно состоять из 3 слов");
    }
    
    public static IRuleBuilderOptions<T, string> ValidPhoneNumber<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .Matches(PhoneRegex())
            .WithMessage("Неверный формат номера телефона. Номер должен содержать от 10 до 15 цифр и может начинаться с '+'");
    }
    
    public static IRuleBuilderOptions<T, string> ValidCreditCardNumber<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .Matches(CreditCardRegex())
            .WithMessage("Неверный формат номера банковской карты. Номер должен содержать от 13 до 19 цифр");
    }
    
    public static IRuleBuilderOptions<T, string> ValidBankAccount<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .Matches(BankAccountRegex())
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

    [GeneratedRegex(@"^[А-Яа-яЁёA-Za-z\s]+$")]
    private static partial Regex FullNameRegex();
    
    [GeneratedRegex(@"^\+?\d{10,15}$")]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"^\d{13,19}$")]
    private static partial Regex CreditCardRegex();
    
    [GeneratedRegex(@"^[0-9]{8,34}$")]
    private static partial Regex BankAccountRegex();
    
}