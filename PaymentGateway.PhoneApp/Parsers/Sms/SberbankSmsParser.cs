namespace PaymentGateway.PhoneApp.Parsers.Sms;

public class SberbankSmsParser : ISmsParser
{
    public List<string> Numbers { get; } = ["900"];

    public string Pattern => @"(?:(?:перевел(?:\(а\)|а)?\s+вам)|зачислени(?:е)?|перевод(?:\s+из\s+[^\s]+)?)\s+(?<money>[+]?\d[\d\s]*(?:[.,]\d+)?)р";
    
    public decimal ExtractedAmount { get; set; }
}