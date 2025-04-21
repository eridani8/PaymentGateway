using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;

namespace PaymentGateway.Web.Services;

public class AuthMessageHandler(
    CustomAuthStateProvider customAuthStateProvider,
    NavigationManager navigationManager) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await customAuthStateProvider.GetTokenFromLocalStorageAsync();

        if (!string.IsNullOrEmpty(token))
        {
            if (CustomAuthStateProvider.IsTokenExpired(token))
            {
                await customAuthStateProvider.MarkUserAsLoggedOut();
                
                _ = Task.Run(() => 
                {
                    navigationManager.NavigateTo("/login",true);
                }, cancellationToken);
                
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // if (response.StatusCode == HttpStatusCode.Unauthorized)
        // {
        //     await customAuthStateProvider.MarkUserAsLoggedOut();
        //     
        //     _ = Task.Run(() => 
        //     {
        //         navigationManager.NavigateTo("/login", true);
        //     }, cancellationToken);
        // }

        return response;
    }
}