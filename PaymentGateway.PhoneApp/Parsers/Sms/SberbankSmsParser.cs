using System.Text.RegularExpressions;

namespace PaymentGateway.PhoneApp.Parsers.Sms;

public class SberbankSmsParser : ISmsParser
{
    public List<string> Numbers { get; } = ["900"];

    public Regex[] Patterns { get; } =
    [
        new(@"перевел\(а\) вам\s*(?<money>\d{1,3}(?:\s\d{3})*)р", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"зачисление\s*(?<money>\d+)\s*р", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"Перевод\s*(?<money>\d+)\s*р от", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"\+\s*(?<money>\d+)\s*р от", RegexOptions.Compiled | RegexOptions.IgnoreCase)
    ];

    public decimal ExtractedAmount { get; set; }
}