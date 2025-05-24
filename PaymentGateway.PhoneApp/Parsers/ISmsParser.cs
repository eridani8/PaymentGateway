using System.Text.RegularExpressions;

namespace PaymentGateway.PhoneApp.Parsers;

public interface ISmsParser
{
    List<string> Numbers { get; }
    string Pattern { get; }
    decimal ExtractedAmount { get; set; }
    
    public bool ParseMessage(string message)
    {
        var match = Regex.Match(message, Pattern);
        if (!match.Success) return false;
        var moneyGroup = match.Groups["money"];
        if (!moneyGroup.Success) return false;
        if (!decimal.TryParse(moneyGroup.Value, out var amount)) return false;
        ExtractedAmount = amount;
        return true;
    }
}