using AutoMapper;

namespace PaymentGateway.Application.Mappings.Converters;

public class LocalToUtcTimeOnlyConverter : IValueConverter<TimeOnly, TimeOnly>
{
    public TimeOnly Convert(TimeOnly sourceMember, ResolutionContext context)
    {
        return sourceMember.LocalToUtcTimeOnly();
    }
}