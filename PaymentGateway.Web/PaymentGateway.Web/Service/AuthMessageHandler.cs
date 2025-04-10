﻿using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Authorization;

namespace PaymentGateway.Web.Service;

public class AuthMessageHandler(CustomAuthStateProvider customAuthStateProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await customAuthStateProvider.GetTokenFromLocalStorageAsync();

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}