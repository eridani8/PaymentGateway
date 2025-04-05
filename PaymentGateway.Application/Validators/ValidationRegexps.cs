using System.Text.RegularExpressions;

namespace PaymentGateway.Application.Validators;

public partial class ValidationRegexps
{
    [GeneratedRegex(@"^[А-Яа-яЁёA-Za-z\s]+$")]
    public static partial Regex FullNameRegex();
    
    [GeneratedRegex(@"^\+\d{10,15}$")]
    public static partial Regex PhoneRegex();

    [GeneratedRegex(@"^\d{13,19}$")]
    public static partial Regex CreditCardRegex();
    
    [GeneratedRegex(@"^[0-9]{8,34}$")]
    public static partial Regex BankAccountRegex();
}