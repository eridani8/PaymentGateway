using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class PaymentService(
    IHttpClientFactory factory,
    ILogger<RequisiteService> logger,
    AuthenticationStateProvider authStateProvider,
    JsonSerializerOptions jsonOptions) : ServiceBase(factory, logger, jsonOptions), IPaymentService
{
    private const string apiEndpoint = "api/v1/payments";
    
    public async Task<Guid?> CreatePayment(PaymentCreateDto dto)
    {
        var response = await PostRequest(apiEndpoint, dto);
            
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content) && Guid.TryParse(response.Content, out var guid))
        {
            return guid;
        }
            
        return Guid.Empty;
    }
    
    public async Task<Response> ManualConfirmPayment(Guid id)
    {
        return await PutRequest($"{apiEndpoint}/confirm", new PaymentManualConfirmDto()
        {
            PaymentId = id
        });
    }

    public async Task<Response> CancelPayment(Guid id)
    {
        return await PutRequest($"{apiEndpoint}/cancel", new PaymentCancelDto()
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
        
        var response = await GetRequest(apiEndpoint);
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<List<PaymentDto>>(response.Content, JsonOptions) ?? [];
        }
        logger.LogWarning("Failed to get payments. Status code: {StatusCode}", response.Code);
        return [];
    }

    public async Task<List<PaymentDto>> GetUserPayments()
    {
        var response = await GetRequest($"{apiEndpoint}/user");
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
        var isAdmin = authState.User.IsInRole("Admin") || authState.User.IsInRole("Support");
        
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
    
    public async Task<PaymentDto?> GetPaymentById(Guid id)
    {
        var response = await GetRequest($"{apiEndpoint}/{id}");
        
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            try
            {
                return JsonSerializer.Deserialize<PaymentDto>(response.Content, JsonOptions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при десериализации платежа с ID {Id}: {Message}", id, ex.Message);
                return null;
            }
        }
        
        if (response.Code == HttpStatusCode.NotFound)
        {
            logger.LogInformation("Payment with ID {Id} not found", id);
            return null;
        }
        
        if (response.Code == HttpStatusCode.Forbidden)
        {
            logger.LogInformation("Access denied to payment with ID {Id}", id);
            return null;
        }
        
        logger.LogWarning("Failed to get payment by ID {Id}. Status code: {StatusCode}", id, response.Code);
        return null;
    }
}