﻿namespace PaymentGateway.Core.Configs;

public class CryptographyConfig
{
    public required string Key { get; set; }
    
    // ReSharper disable once InconsistentNaming
    public required string IV { get; set; }
}