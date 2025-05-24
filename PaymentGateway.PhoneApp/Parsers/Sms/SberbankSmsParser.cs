namespace PaymentGateway.PhoneApp.Parsers.Sms;

public class SberbankSmsParser : ISmsParser
{
    public List<string> Numbers { get; } = ["900"];
    public string Pattern => @"Перевод\s+(?<money>\d+)р";
    public decimal ExtractedAmount { get; set; }
}