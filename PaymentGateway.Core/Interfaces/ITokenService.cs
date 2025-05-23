﻿using PaymentGateway.Core.Entities;

namespace PaymentGateway.Core.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(UserEntity user, IList<string> roles);
    string GenerateDeviceJwtToken(UserEntity user, string role);
}