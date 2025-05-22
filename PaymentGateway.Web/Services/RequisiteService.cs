using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.Types;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class RequisiteService(
    IHttpClientFactory factory,
    ILogger<RequisiteService> logger,
    JsonSerializerOptions jsonOptions) : ServiceBase(factory, logger, jsonOptions), IRequisiteService
{
    private const string apiEndpoint = "api/requisites";
    
    public async Task<List<RequisiteDto>> GetRequisites()
    {
        var response = await GetRequest(apiEndpoint);
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<List<RequisiteDto>>(response.Content, JsonOptions) ?? [];
        }
        return [];
    }
    
    public async Task<List<RequisiteDto>> GetUserRequisites()
    {
        var response = await GetRequest($"{apiEndpoint}/user");
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<List<RequisiteDto>>(response.Content, JsonOptions) ?? [];
        }
        return [];
    }
    
    public async Task<Response> CreateRequisite(RequisiteCreateDto dto)
    {
        return await PostRequest(apiEndpoint, dto);
    }
    
    public async Task<Response> UpdateRequisite(Guid id, RequisiteUpdateDto dto)
    {
        return await PutRequest($"{apiEndpoint}/{id}", dto);
    }
    
    public async Task<Response> DeleteRequisite(Guid id)
    {
        return await DeleteRequest($"{apiEndpoint}/{id}");
    }

    public async Task<List<RequisiteDto>> GetRequisitesByUserId(Guid userId)
    {
        var response = await GetRequest($"{apiEndpoint}/user/{userId}");
        if (response.Code == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
        {
            return JsonSerializer.Deserialize<List<RequisiteDto>>(response.Content, JsonOptions) ?? [];
        }
        return [];
    }
} 