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

    public async Task BackgroundCheckAsync(CancellationToken token = default)
    {
        try
        {
            await CheckAvailable(token);
            logger.LogDebug("Проверка доступности: {state}", State);
            await ShowOrHideUnavailableModal(token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при проверке доступности");
        }
    }

    public async Task ShowOrHideUnavailableModal(CancellationToken token = default)
    {
        try
        {
            if (Shell.Current == null) return;

            if (Shell.Current.Navigation is { } navigation)
            {
                var serviceUnavailablePageShown = navigation.ModalStack.Any(p => p is ServiceUnavailablePage);
                if (!State)
                {
                    if (!serviceUnavailablePageShown)
                    {
                        var viewModel = serviceProvider.GetRequiredService<ServiceUnavailableViewModel>();
                        await navigation.PushModalAsync(new ServiceUnavailablePage(viewModel), true);
                    }
                }
                else
                {
                    if (serviceUnavailablePageShown)
                    {
                        await navigation.PopModalAsync(true);
                    }
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
        }
    }
}