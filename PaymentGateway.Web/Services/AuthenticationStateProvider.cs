﻿using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace PaymentGateway.Web.Services;

public class CustomAuthStateProvider(IHttpClientFactory httpClientFactory, ILocalStorageService localStorage) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await GetTokenFromLocalStorageAsync();

        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        if (IsTokenExpired(token))
        {
            await RemoveTokenFromLocalStorageAsync();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var principal = new ClaimsPrincipal(identity);

        return new AuthenticationState(principal);
    }

    private static bool IsTokenExpired(string token)
    {
        try
        {
            var claims = ParseClaimsFromJwt(token);
            var expiry = claims.FirstOrDefault(c => c.Type.Equals("exp"))?.Value;
            
            if (string.IsNullOrEmpty(expiry) || !long.TryParse(expiry, out var expiryTimeStamp))
                return true;
                
            var expiryDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(expiryTimeStamp);
            
            return expiryDateTimeOffset <= DateTimeOffset.UtcNow;
        }
        catch
        {
            return true;
        }
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        await SetTokenInLocalStorageAsync(token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task MarkUserAsLoggedOut()
    {
        await RemoveTokenFromLocalStorageAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        var claims = new List<Claim>();
        if (keyValuePairs == null) return claims;
        
        foreach (var (key, value) in keyValuePairs)
        {
            if (key.Equals("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", StringComparison.OrdinalIgnoreCase))
            {
                switch (value)
                {
                    case JsonElement { ValueKind: JsonValueKind.Array } rolesElement:
                    {
                        claims.AddRange(rolesElement.EnumerateArray().Select(role => new Claim(ClaimTypes.Role, role.GetString() ?? string.Empty)));

                        break;
                    }
                    case string rolesString:
                    {
                        var roles = rolesString.Split(';');
                        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                        break;
                    }
                    default:
                    {
                        if (value is string singleRole)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, singleRole));
                        }

                        break;
                    }
                }
            }
            else
            {
                claims.Add(new Claim(key, value.ToString() ?? string.Empty));
            }
        }
        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }

    private const string TokenKey = "authToken";

    public async Task<string?> GetTokenFromLocalStorageAsync()
    {
        return await localStorage.GetItemAsync<string>(TokenKey);
    }

    private async Task SetTokenInLocalStorageAsync(string token)
    {
        await localStorage.SetItemAsync(TokenKey, token);
    }

    private async Task RemoveTokenFromLocalStorageAsync()
    {
        await localStorage.RemoveItemAsync(TokenKey);
    }
}