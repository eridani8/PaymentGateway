using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class RequisiteService(
    IHttpClientFactory factory,
    ILogger<RequisiteService> logger,
    AuthenticationStateProvider authStateProvider) : ServiceBase(factory, logger), IRequisiteService
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
        if (response.Code == HttpStatusCode.OK)
        {
            return JsonSerializer.Deserialize<List<RequisiteDto>>(response.Content ?? string.Empty, JsonOptions) ?? [];
        }
        return [];
    }
    
    public async Task<List<RequisiteDto>> GetUserRequisites()
    {
        var response = await GetRequest($"{ApiEndpoint}/GetUserRequisites");
        if (response.Code == HttpStatusCode.OK)
        {
            return JsonSerializer.Deserialize<List<RequisiteDto>>(response.Content ?? string.Empty, JsonOptions) ?? [];
        }
        return [];
    }
    
    public async Task<RequisiteDto?> CreateRequisite(RequisiteCreateDto dto)
    {
        return await PostRequest<RequisiteDto>($"{ApiEndpoint}/Create", dto);
    }
    
    public async Task<Response> UpdateRequisite(Guid id, RequisiteUpdateDto dto)
    {
        return await PutRequest($"{ApiEndpoint}/Update/{id}", dto);
    }
    
    public async Task<bool> DeleteRequisite(Guid id)
    {
        return await DeleteRequest($"{ApiEndpoint}/Delete/{id}");
    }
} 