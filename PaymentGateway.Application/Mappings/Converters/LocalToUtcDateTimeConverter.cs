﻿using AutoMapper;

namespace PaymentGateway.Application.Mappings.Converters;

public class LocalToUtcDateTimeConverter : IValueConverter<DateTime, DateTime>
{
    public DateTime Convert(DateTime sourceMember, ResolutionContext context)
    {
        if (sourceMember.Kind is DateTimeKind.Local or DateTimeKind.Unspecified)
        {
            return sourceMember.ToUniversalTime();
        }
        return sourceMember;
    }
}