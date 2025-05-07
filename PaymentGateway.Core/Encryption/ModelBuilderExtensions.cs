using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Core.Interfaces;

namespace PaymentGateway.Core.Encryption;

public static class ModelBuilderExtensions
{
    public static void ApplyEncryptedProperties(this ModelBuilder modelBuilder, ICryptographyService crypto)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            foreach (var property in clrType.GetProperties())
            {
                var isEncrypted = property.GetCustomAttribute<EncryptedAttribute>() != null;
                if (!isEncrypted || property.PropertyType != typeof(string)) continue;
                var prop = entityType?.FindProperty(property.Name);
                prop?.SetValueConverter(new StringEncryptionConverter(crypto));
            }
        }
    }
}