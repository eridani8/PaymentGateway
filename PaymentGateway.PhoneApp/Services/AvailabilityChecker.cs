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
    private const string apiVersion = "v1";
    public bool State { get; private set; }

    public async Task CheckAvailable(CancellationToken token = default)
    {
        try
        {
            using var client = clientFactory.CreateClient("API");
            var response = await client.GetAsync($"{client.BaseAddress}/api/{apiVersion}/health/check-available", token);
            var isSuccess = response.IsSuccessStatusCode;
            State = isSuccess;
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
            // ignore
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

    public Task ShowOrHideUnavailableModal(CancellationToken token = default)
    {
        try
        {
            if (Shell.Current == null) return Task.CompletedTask;

            if (Shell.Current.Navigation is { } navigation)
            {
                var serviceUnavailablePageShown = navigation.ModalStack.Any(p => p is ServiceUnavailablePage);
                if (!State)
                {
                    if (!serviceUnavailablePageShown)
                    {
                        var viewModel = serviceProvider.GetRequiredService<ServiceUnavailableViewModel>();
                        MainThread.BeginInvokeOnMainThread(async void () =>
                        {
                            await navigation.PushModalAsync(new ServiceUnavailablePage(viewModel), true);
                        });
                    }
                }
                else
                {
                    if (serviceUnavailablePageShown)
                    {
                        MainThread.BeginInvokeOnMainThread(async void () =>
                        {
                            await navigation.PopModalAsync(true);
                        });
                    }
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
        }

        return Task.CompletedTask;
    }
}