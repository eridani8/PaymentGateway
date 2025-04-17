using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class PaymentService(
    IHttpClientFactory factory,
    ILogger<RequisiteService> logger,
    AuthenticationStateProvider authStateProvider) : ServiceBase(factory, logger), IPaymentService
{
    private const string ApiEndpoint = "Payment";
    
    public async Task<Response> ManualConfirmPayment(Guid id)
    {
        return await PostRequest($"{ApiEndpoint}/ManualConfirmPayment", new PaymentManualConfirmDto()
        {
            PaymentId = id
        });
    }

    public async Task<List<PaymentDto>> GetPayments()
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var isAdmin = authState.User.IsInRole("Admin");
        
        if (!isAdmin)
        {
            return await GetUserPayments();
        }
        
        var response = await GetRequest($"{ApiEndpoint}/GetAll");
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<List<PaymentDto>>(response.Content, JsonOptions) ?? [];
        }
        logger.LogWarning("Failed to get payments. Status code: {StatusCode}", response.Code);
        return [];
    }

    public async Task<List<PaymentDto>> GetUserPayments()
    {
        var response = await GetRequest($"{ApiEndpoint}/GetUserPayments");
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<List<PaymentDto>>(response.Content, JsonOptions) ?? [];
        }
        logger.LogWarning("Failed to get user payments. Status code: {StatusCode}", response.Code);
        return [];
    }
    
    public async Task<List<PaymentDto>> GetPaymentsByUserId(Guid userId)
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var isAdmin = authState.User.IsInRole("Admin");
        
        if (!isAdmin)
        {
            logger.LogWarning("Non-admin user attempted to access payment data for user ID: {UserId}", userId);
            return [];
        }
        
        var allPayments = await GetPayments();
        return allPayments
        .Where(p => p.Requisite != null && p.Requisite.UserId == userId)
        .ToList();
    }
}