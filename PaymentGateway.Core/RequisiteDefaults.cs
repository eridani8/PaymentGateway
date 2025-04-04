﻿namespace PaymentGateway.Core;

public class RequisiteDefaults
{
    public bool IsActive { get; init; }
    public decimal MaxAmount { get; init; }
    public int CooldownMinutes { get; init; }
    public int Priority { get; init; }
}