using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;

namespace PaymentGateway.PhoneApp.Services;

public class AvailabilityChecker(IHttpClientFactory clientFactory, ILogger<AvailabilityChecker> logger) : IAvailabilityChecker
{
    public bool State { get; private set; }

    public async Task CheckAvailable(CancellationToken token = default)
    {
        try
        {
            using var client = clientFactory.CreateClient("API");
            var response = await client.GetAsync($"{client.BaseAddress}/Health/CheckAvailable", token);
            var isSuccess = response.IsSuccessStatusCode;
            State = isSuccess;
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
        }
    }
}