using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.PhoneApp.Pages;
using PaymentGateway.PhoneApp.ViewModels;

namespace PaymentGateway.PhoneApp.Services;

public class AvailabilityChecker(
    IHttpClientFactory clientFactory,
    ILogger<AvailabilityChecker> logger,
    IServiceProvider serviceProvider) : IAvailabilityChecker
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
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
        }
    }

    public async Task ShowOrHideUnavailableModal(CancellationToken token = default)
    {
        await CheckAvailable(token);
        logger.LogDebug("Проверка доступности: {state}", State);
        var serviceUnavailablePageShown = Shell.Current.Navigation.ModalStack.Any(p => p is ServiceUnavailablePage);
        if (!State)
        {
            if (!serviceUnavailablePageShown)
            {
                var viewModel = serviceProvider.GetRequiredService<ServiceUnavailableViewModel>();
                await Shell.Current.Navigation.PushModalAsync(
                    new ServiceUnavailablePage(viewModel), true);
            }
        }
        else
        {
            if (serviceUnavailablePageShown)
            {
                await Shell.Current.Navigation.PopModalAsync(true);
            }
        }
    }
}