﻿using AutoMapper;

namespace PaymentGateway.Application.Mappings.Converters;

public class UtcToLocalNullableDateTimeConverter : IValueConverter<DateTime?, DateTime?>
{
    public DateTime? Convert(DateTime? sourceMember, ResolutionContext context)
    {
        if (sourceMember?.Kind == DateTimeKind.Utc)
        {
            return sourceMember.Value.ToLocalTime();
        }
        return sourceMember;
    }
}