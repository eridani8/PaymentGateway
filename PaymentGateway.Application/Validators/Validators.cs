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
    
    public static IRuleBuilderOptions<T, RequisiteType> ValidRequisiteType<T>(IRuleBuilder<T, RequisiteType> rule)
    {
        return rule.Must(x => Enum.IsDefined(x) && (int)x >= 0 && (int)x < Enum.GetValues<RequisiteType>().Length)
            .WithMessage("Тип реквизита должен быть валидным значением из перечисления");
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
}