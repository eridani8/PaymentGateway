using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using PaymentGateway.Shared.DTOs.Transaction;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class TransactionService(
    IHttpClientFactory factory,
    ILogger<TransactionService> logger,
    AuthenticationStateProvider authStateProvider,
    JsonSerializerOptions jsonOptions) : ServiceBase(factory, logger, jsonOptions), ITransactionService
{
    private const string ApiEndpoint = "Transaction";
    
    public async Task<List<TransactionDto>> GetTransactions()
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var isAdmin = authState.User.IsInRole("Admin");
        
        if (!isAdmin)
        {
            return await GetUserTransactions();
        }
        
        var response = await GetRequest($"{ApiEndpoint}/GetAll");
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<List<TransactionDto>>(response.Content, JsonOptions) ?? [];
        }
        logger.LogWarning("Failed to get transactions. Status code: {StatusCode}", response.Code);
        return [];
    }

    public async Task<List<TransactionDto>> GetUserTransactions()
    {
        var response = await GetRequest($"{ApiEndpoint}/GetUserTransactions");
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<List<TransactionDto>>(response.Content, JsonOptions) ?? [];
        }
        logger.LogWarning("Failed to get user transactions. Status code: {StatusCode}", response.Code);
        return [];
    }
} 