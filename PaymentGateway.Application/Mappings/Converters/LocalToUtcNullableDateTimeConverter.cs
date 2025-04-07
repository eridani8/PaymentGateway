using AutoMapper;

namespace PaymentGateway.Application.Mappings.Converters;

public class LocalToUtcNullableDateTimeConverter : IValueConverter<DateTime?, DateTime?>
{
    public DateTime? Convert(DateTime? sourceMember, ResolutionContext context)
    {
        if (sourceMember?.Kind is DateTimeKind.Local or DateTimeKind.Unspecified)
        {
            return sourceMember.Value.ToUniversalTime();
        }
        return sourceMember;
    }
}