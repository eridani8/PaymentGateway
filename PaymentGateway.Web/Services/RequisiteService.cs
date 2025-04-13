using System.Net;
using System.Text.Json;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class RequisiteService(
    IHttpClientFactory factory,
    ILogger<RequisiteService> logger) : ServiceBase(factory, logger), IRequisiteService
{
    private const string ApiEndpoint = "Requisite";
    
    public async Task<List<RequisiteDto>> GetRequisites()
    {
        var response = await GetRequest($"{ApiEndpoint}/GetAll");
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
    
    public async Task<bool> DeleteRequisite(Guid id)
    {
        return await DeleteRequest($"{ApiEndpoint}/Delete/{id}");
    }
} 