using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace PaymentGateway.Web.Services;

public class SignalRService(IOptions<ApiSettings> settings, ILogger<SignalRService> logger)
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl = $"{settings.Value.BaseAddress}/notificationHub";

    public async Task InitializeAsync()
    {
        try
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.Closed += async (error) =>
            {
                logger.LogWarning("Соединение с SignalR закрыто: {Error}", error?.Message);
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await _hubConnection.StartAsync();
            };

            await _hubConnection.StartAsync();
            logger.LogInformation("SignalR соединение установлено");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при инициализации SignalR соединения");
        }
    }

    public void SubscribeToUpdates(string methodName, Action handler)
    {
        if (_hubConnection == null)
        {
            logger.LogWarning("Попытка подписаться на обновления до инициализации соединения");
            return;
        }

        _hubConnection.On(methodName, handler);
    }

    public void UnsubscribeFromUpdates(string methodName)
    {
        _hubConnection?.Remove(methodName);
    }

    public async Task DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
} 