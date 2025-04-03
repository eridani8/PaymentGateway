﻿using System.ComponentModel;
using PaymentGateway.Core.Enums;

namespace PaymentGateway.Application.DTOs.Requisite;

public class RequisiteCreateDto
{
    public RequisiteType Type { get; init; }
    public required string PaymentData { get; init; }
    public required string FullName { get; init; }
    [DefaultValue(true)] public bool IsActive { get; init; } // ~
    [DefaultValue(5000)] public decimal? MaxAmount { get; init; } // ~
    [DefaultValue(100)] public int? CooldownMinutes { get; init; } // ~
    [DefaultValue(1)] public int? Priority { get; init; } // ~
}