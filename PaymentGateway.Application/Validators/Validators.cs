using FluentValidation;
using PaymentGateway.Core.Enums;

namespace PaymentGateway.Application.Validators;

public static class Validators
{
    public static IRuleBuilderOptions<T, string> ValidFullName<T>(IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty()
            .WithMessage("Требуется указать имя")
            .Length(10, 70).WithMessage("Имя должно быть от 10 до 70 символов");
    }
    
    public static IRuleBuilderOptions<T, TEnum> ValidEnumValue<T, TEnum>(IRuleBuilder<T, TEnum> rule) where TEnum : Enum
    {
        return rule.Must(x => Enum.IsDefined(typeof(TEnum), x) && (int)(object)x >= 0 && (int)(object)x < Enum.GetValues(typeof(TEnum)).Length)
            .WithMessage("Значение должно быть валидным значением из перечисления");
    }

    public static IRuleBuilderOptions<T, string> ValidPaymentData<T>(IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty()
            .WithMessage("Требуются указать данные для оплаты")
            .Length(10, 19).WithMessage("Данные о платежах должны быть от 10 до 19 символов");
    }
    
    public static IRuleBuilderOptions<T, decimal> ValidAmount<T>(IRuleBuilder<T, decimal> rule)
    {
        return rule
            .GreaterThan(0).WithMessage("Сумма должна быть больше 0")
            .LessThanOrEqualTo(9999999999999999.99m).WithMessage("Сумма должна быть меньше или равна 9999999999999999,99");
    }
    
    public static IRuleBuilderOptions<T, int> ValidAmount<T>(IRuleBuilder<T, int> rule)
    {
        return rule
            .GreaterThan(0).WithMessage("Сумма должна быть больше 0")
            .LessThanOrEqualTo(999999999).WithMessage("Сумма должна быть меньше или равна 999999999");
    }
}