using Microsoft.AspNetCore.SignalR.Client;
using PaymentGateway.Shared.Interfaces;

namespace PaymentGateway.Web.Services;

public class NotificationService : INotificationService, IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<NotificationService> _logger;

    public event Action<string>? OnPaymentStatusChanged;
    public event Action? OnUserUpdated;
    public event Action? OnPaymentUpdated;
    public event Action? OnRequisiteUpdated;

    public NotificationService(IConfiguration configuration, ILogger<NotificationService> logger)
    {
        _logger = logger;
        var hubUrl = configuration["ApiUrl"] + "/notificationHub";
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<string>("PaymentStatusChanged", (paymentId) =>
        {
            OnPaymentStatusChanged?.Invoke(paymentId);
        });

        _hubConnection.On("UserUpdated", () =>
        {
            OnUserUpdated?.Invoke();
        });

        _hubConnection.On("PaymentUpdated", () =>
        {
            OnPaymentUpdated?.Invoke();
        });

        _hubConnection.On("RequisiteUpdated", () =>
        {
            OnRequisiteUpdated?.Invoke();
        });
    }

    public async Task StartAsync()
    {
        try
        {
            await _hubConnection.StartAsync();
            _logger.LogInformation("SignalR connection started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting SignalR connection");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }

    public Task NotifyUserUpdated()
    {
        return Task.CompletedTask;
    }

    public Task NotifyPaymentUpdated()
    {
        return Task.CompletedTask;
    }

    public Task NotifyRequisiteUpdated()
    {
        return Task.CompletedTask;
    }

    public Task NotifyPaymentStatusChanged(string paymentId)
    {
        return Task.CompletedTask;
    }
} 