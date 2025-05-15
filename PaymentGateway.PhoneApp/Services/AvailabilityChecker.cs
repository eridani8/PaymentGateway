using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.Shared.Types;

namespace PaymentGateway.PhoneApp.Services;

public class AvailabilityChecker(
    IHttpClientFactory clientFactory,
    ILogger<AvailabilityChecker> logger,
    JsonSerializerOptions jsonOptions) : ServiceBase(clientFactory, logger, jsonOptions), IAvailabilityChecker
{
    private const string apiEndpoint = "api/v1/health";
    public bool State { get; private set; }

    public async Task CheckAvailable()
    {
        try
        {
            var response = await GetRequest($"{apiEndpoint}/check-available");
            State = response.Code == HttpStatusCode.OK;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка проверки доступности");
        }
    }

    public async Task BackgroundCheckAsync()
    {
        await CheckAvailable();
        logger.LogDebug("Проверка доступности: {State}", State);
        await ShowOrHideUnavailableModal();
    }

    public Task ShowOrHideUnavailableModal()
    {
        if (Shell.Current == null) return Task.CompletedTask;

        if (Shell.Current.Navigation is { } navigation)
        {
            // var serviceUnavailablePageShown = navigation.ModalStack.Any(p => p is ServiceUnavailablePage);
            // if (!State)
            // {
            //     if (!serviceUnavailablePageShown)
            //     {
            //         var viewModel = serviceProvider.GetRequiredService<ServiceUnavailableViewModel>();
            //         MainThread.BeginInvokeOnMainThread(async void () =>
            //         {
            //             await navigation.PushModalAsync(new ServiceUnavailablePage(viewModel), true);
            //         });
            //     }
            // }
            // else
            // {
            //     if (serviceUnavailablePageShown)
            //     {
            //         MainThread.BeginInvokeOnMainThread(async void () =>
            //         {
            //             await navigation.PopModalAsync(true);
            //         });
            //     }
            // } // TODO
        }
        return Task.CompletedTask;
    }
}