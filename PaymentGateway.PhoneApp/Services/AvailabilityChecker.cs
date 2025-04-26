using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;

namespace PaymentGateway.PhoneApp.Services;

public class AvailabilityChecker(IHttpClientFactory clientFactory, ILogger<AvailabilityChecker> logger) : IAvailabilityChecker
{
    public bool State { get; private set; }

    public async Task<bool> CheckAvailable()
    {
        try
        {
            using var client = clientFactory.CreateClient("API");
            var response = await client.GetAsync($"{client.BaseAddress}/Health/CheckAvailable");
            var isSuccess = response.IsSuccessStatusCode;
            State = isSuccess;
            return isSuccess;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return false;
        }
    }
}