using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class RequisiteService(
    IHttpClientFactory factory,
    ILogger<RequisiteService> logger,
    AuthenticationStateProvider authStateProvider,
    JsonSerializerOptions jsonOptions) : ServiceBase(factory, logger, jsonOptions), IRequisiteService
{
    private const string ApiEndpoint = "Requisite";
    
    public async Task<List<RequisiteDto>> GetRequisites()
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var isAdmin = authState.User.IsInRole("Admin");
        
        if (!isAdmin)
        {
            return await GetUserRequisites();
        }

        var response = await GetRequest($"{ApiEndpoint}/GetAll");
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<List<RequisiteDto>>(response.Content, JsonOptions) ?? [];
        }
        logger.LogWarning("Failed to get requisites. Status code: {StatusCode}", response.Code);
        return [];
    }
    
    public async Task<List<RequisiteDto>> GetUserRequisites()
    {
        var response = await GetRequest($"{ApiEndpoint}/GetUserRequisites");
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<List<RequisiteDto>>(response.Content, JsonOptions) ?? [];
        }
        logger.LogWarning("Failed to get user requisites. Status code: {StatusCode}", response.Code);
        return [];
    }
    
    public async Task<RequisiteDto?> CreateRequisite(RequisiteCreateDto dto)
    {
        try
        {
            var response = await PostRequest($"{ApiEndpoint}/Create", dto);
            
            if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
            {
                return JsonSerializer.Deserialize<RequisiteDto>(response.Content, JsonOptions);
            }
            
            if (response.Code == HttpStatusCode.BadRequest && !string.IsNullOrEmpty(response.Content))
            {
                var errorMessage = response.Content;
                throw new Exception(errorMessage);
            }
            
            logger.LogWarning("Failed to create requisite. Status code: {StatusCode}", response.Code);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating requisite");
            throw;
        }
    }
    
    public async Task<Response> UpdateRequisite(Guid id, RequisiteUpdateDto dto)
    {
        var response = await PutRequest($"{ApiEndpoint}/Update/{id}", dto);
        if (response.Code != HttpStatusCode.OK)
        {
            logger.LogWarning("Failed to update requisite {Id}. Status code: {StatusCode}", id, response.Code);
        }
        return response;
    }
    
    public async Task<RequisiteDto?> DeleteRequisite(Guid id)
    {
        var response = await DeleteRequest<RequisiteDto>($"{ApiEndpoint}/Delete/{id}");
        if (response is null)
        {
            logger.LogWarning("Failed to delete requisite {Id}", id);
        }
        return response;
    }

    public async Task<List<RequisiteDto>> GetRequisitesByUserId(Guid userId)
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var isAdmin = authState.User.IsInRole("Admin") || authState.User.IsInRole("Support");
        
        if (!isAdmin)
        {
            logger.LogWarning("Non-admin user attempted to access requisite data for user ID: {UserId}", userId);
            return [];
        }
        
        var allPayments = await GetRequisites();
        return allPayments
            .Where(r => r.UserId == userId)
            .ToList();
    }
} 