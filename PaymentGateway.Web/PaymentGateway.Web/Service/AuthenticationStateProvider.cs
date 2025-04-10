using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace PaymentGateway.Web.Service;

public class CustomAuthStateProvider(IHttpClientFactory httpClientFactory, ILocalStorageService localStorage) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await GetTokenFromLocalStorageAsync();

        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var principal = new ClaimsPrincipal(identity);

        return new AuthenticationState(principal);
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

    private IEnumerable<Claim>? ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        var claims = keyValuePairs?.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty));
        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }

    private const string TokenKey = "authToken";
    private async Task<string?> GetTokenFromLocalStorageAsync()
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