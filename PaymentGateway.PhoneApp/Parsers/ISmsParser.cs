using System.Text.RegularExpressions;

namespace PaymentGateway.PhoneApp.Parsers;

public interface ISmsParser
{
    List<string> Numbers { get; }
    string Pattern { get; }
    decimal ExtractedAmount { get; set; }
    
    public bool ParseMessage(string message)
    {
        var match = Regex.Match(message, Pattern, RegexOptions.IgnoreCase);
        if (!match.Success) return false;
        var moneyGroup = match.Groups["money"];
        if (!moneyGroup.Success) return false;
        var amountStr = moneyGroup.Value.Replace(" ", "");
        if (!decimal.TryParse(amountStr, out var amount)) return false;
        if (amount == 0) return false;
        ExtractedAmount = amount;
        return true;
    }
}