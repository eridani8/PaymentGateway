﻿using AutoMapper;
using PaymentGateway.Application.Extensions;

namespace PaymentGateway.Application.Mappings.Converters;

public class UtcToLocalTimeOnlyConverter : IValueConverter<TimeOnly, TimeOnly>
{
    public TimeOnly Convert(TimeOnly sourceMember, ResolutionContext context)
    {
        return sourceMember.UtcToLocalTimeOnly();
    }
}