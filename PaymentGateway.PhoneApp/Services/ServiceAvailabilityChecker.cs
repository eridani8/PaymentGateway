using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;

namespace PaymentGateway.PhoneApp.Services;

public class ServiceAvailabilityChecker(IHttpClientFactory clientFactory, ILogger<ServiceAvailabilityChecker> logger)
    : IServiceAvailabilityChecker
{
    public async Task<bool> CheckAvailable()
    {
        try
        {
            using var client = clientFactory.CreateClient("API");
            var response = await client.GetAsync($"{client.BaseAddress}/Health/CheckAvailable");
            return response.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return false;
        }
    }
}