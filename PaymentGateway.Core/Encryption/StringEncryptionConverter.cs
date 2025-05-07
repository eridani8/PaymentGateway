using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Core.Encryption;

public class StringEncryptionConverter(ICryptographyService cryptography) : ValueConverter<string, string>(
    convertToProviderExpression: v => cryptography.Encrypt(v),
    convertFromProviderExpression: v => cryptography.Decrypt(v))
{
}