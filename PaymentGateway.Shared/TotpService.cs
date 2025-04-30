using System.Security.Cryptography;
using OtpNet;
using QRCoder;

namespace PaymentGateway.Shared;

public static class TotpService
{
    private const int totpSize = 6;
    private const int stepSeconds = 30;
    private const int validationWindow = 1;
    
    public static string GenerateSecretKey()
    {
        var secretKeyBytes = RandomNumberGenerator.GetBytes(20);
        return Base32Encoding.ToString(secretKeyBytes);
    }
    
    public static string GenerateTotpUri(string secretKey, string accountName, string issuer)
    {
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedAccountName = Uri.EscapeDataString(accountName);
        
        return $"otpauth://totp/{encodedIssuer}:{encodedAccountName}?secret={secretKey}&issuer={encodedIssuer}&algorithm=SHA1&digits={totpSize}&period={stepSeconds}";
    }
    
    public static string GenerateQrCodeBase64(string totpUri)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(totpUri, QRCodeGenerator.ECCLevel.Q);
        
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(20);
        
        return Convert.ToBase64String(qrCodeBytes);
    }
    
    public static bool VerifyTotpCode(string secretKey, string totpCode)
    {
        if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(totpCode))
        {
            return false;
        }
        
        try
        {
            var keyBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(keyBytes, step: stepSeconds, totpSize: totpSize);
            return totp.VerifyTotp(totpCode, out _, new VerificationWindow(validationWindow));
        }
        catch
        {
            return false;
        }
    }
} 