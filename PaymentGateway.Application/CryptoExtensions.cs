using System.Security.Cryptography;
using System.Text;

namespace PaymentGateway.Application;

public class CryptoExtensions
{
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("your-32-char-long-key-123456789012"); // 32 символа = 256 бит
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("your-16-char-iv456"); // 16 символов = 128 бит

    public static string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Key;
        aes.IV = IV;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        return Convert.ToBase64String(encryptedBytes);
    }

    public static string Decrypt(string cipherText)
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Key;
        aes.IV = IV;

        using var decoder = aes.CreateDecryptor();
        var cipherBytes = Convert.FromBase64String(cipherText);
        var decryptedBytes = decoder.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}