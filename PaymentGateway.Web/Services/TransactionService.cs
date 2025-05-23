using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using PaymentGateway.Shared.DTOs.Transaction;
using PaymentGateway.Shared.Enums;
using PaymentGateway.Shared.Types;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class TransactionService(
    IHttpClientFactory factory,
    ILogger<TransactionService> logger,
    JsonSerializerOptions jsonOptions) : ServiceBase(factory, logger, jsonOptions), ITransactionService
{
    private const string apiEndpoint = "api/transactions";
    
    public async Task<List<TransactionDto>> GetTransactions()
    {
        var response = await GetRequest(apiEndpoint);
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<List<TransactionDto>>(response.Content, JsonOptions) ?? [];
        }
        return [];
    }

    public async Task<List<TransactionDto>> GetUserTransactions()
    {
        var response = await GetRequest($"{apiEndpoint}/user");
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<List<TransactionDto>>(response.Content, JsonOptions) ?? [];
        }
        return [];
    }
    
    public async Task<TransactionDto?> SimulatePayment(string paymentData, decimal amount, TransactionSource source)
    {
        try
        {
            var dto = new TransactionCreateDto
            {
                // PaymentData = paymentData,
                ExtractedAmount = amount,
                Source = source,
                RawMessage = $"Поступление {amount} на счет {paymentData} через {source}"
            };
            
            var response = await PostRequest<TransactionDto>(apiEndpoint, dto);
            
            if (response is not null)
            {
                logger.LogInformation("Успешно создана транзакция {id} на сумму {amount} через {source}", 
                    response.Id, amount, source);
            }
            else
            {
                logger.LogWarning("Не удалось создать транзакцию для платежа {paymentData} на сумму {amount}", 
                    paymentData, amount);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при имитации платежа на сумму {amount} для {paymentData}", amount, paymentData);
            throw;
        }
    }

    public async Task<List<TransactionDto>> GetTransactionsByUserId(Guid userId)
    {
        var response = await GetRequest($"{apiEndpoint}/user/{userId}");
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<List<TransactionDto>>(response.Content, JsonOptions) ?? [];
        }
        return [];
    }
} 