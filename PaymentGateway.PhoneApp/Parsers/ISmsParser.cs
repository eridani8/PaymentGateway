using System.Text.RegularExpressions;

namespace PaymentGateway.PhoneApp.Parsers;

public interface ISmsParser
{
    List<string> Numbers { get; }
    Regex[] Patterns { get; }
    decimal ExtractedAmount { get; set; }
    
    public bool ParseMessage(string message)
    {
        foreach (var regex in Patterns)
        {
            var match = regex.Match(message);
            if (!match.Success) continue;

            var moneyGroup = match.Groups["money"];
            if (!moneyGroup.Success) continue;

            var amountStr = moneyGroup.Value.Replace(" ", "");
            if (!decimal.TryParse(amountStr, out var amount)) continue;

            if (amount == 0) continue;

            ExtractedAmount = amount;
            return true;
        }
        return false;
    }
}