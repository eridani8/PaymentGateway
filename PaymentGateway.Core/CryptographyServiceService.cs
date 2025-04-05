using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Core;

public class CryptographyServiceService(IOptions<CryptographyConfig> config) : ICryptographyService
{
    public string Encrypt(string text)
    {
        using var aes = CreateAes();
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(text);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        return Convert.ToBase64String(encryptedBytes);
    }

    public string Decrypt(string encrypted)
    {
        using var aes = CreateAes();
        using var decryptor = aes.CreateDecryptor();
        var encryptedBytes = Convert.FromBase64String(encrypted);
        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public Aes CreateAes()
    {
        var aes = Aes.Create();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Convert.FromBase64String(config.Value.Key); 
        aes.IV = Convert.FromBase64String(config.Value.IV);
        return aes;
    }
}