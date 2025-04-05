namespace PaymentGateway.Core;

public class CryptographyConfig
{
    public required string Key { get; set; }
    
    public required string IV { get; set; }
}