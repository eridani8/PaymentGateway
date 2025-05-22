using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.Types;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class PaymentService(
    IHttpClientFactory factory,
    ILogger<RequisiteService> logger,
    JsonSerializerOptions jsonOptions) : ServiceBase(factory, logger, jsonOptions), IPaymentService
{
    private const string apiEndpoint = "api/payments";
    
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
        var response = await GetRequest(apiEndpoint);
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<List<PaymentDto>>(response.Content, JsonOptions) ?? [];
        }
        return [];
    }

    public async Task<List<PaymentDto>> GetUserPayments()
    {
        var response = await GetRequest($"{apiEndpoint}/user");
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<List<PaymentDto>>(response.Content, JsonOptions) ?? [];
        }
        return [];
    }
    
    public async Task<List<PaymentDto>> GetPaymentsByUserId(Guid userId)
    {
        var payments = await GetPayments();
        return payments
            .Where(p => p.Requisite is not null && p.Requisite.UserId == userId)
            .ToList(); // TODO
    }
    
    public async Task<PaymentDto?> GetPaymentById(Guid id)
    {
        var response = await GetRequest($"{apiEndpoint}/{id}");
        
        switch (response.Code)
        {
            case HttpStatusCode.OK when !string.IsNullOrEmpty(response.Content):
                try
                {
                    return JsonSerializer.Deserialize<PaymentDto>(response.Content, JsonOptions);
                }
                catch
                {
                    return null;
                }

            case HttpStatusCode.NotFound:
            case HttpStatusCode.Forbidden:
            default:
                return null;
        }
    }
}