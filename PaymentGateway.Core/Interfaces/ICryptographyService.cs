using System.Security.Cryptography;

namespace PaymentGateway.Core.Interfaces;

public interface ICryptographyService
{
    string Encrypt(string text);
    string Decrypt(string encrypted);
    Aes CreateAes();
}