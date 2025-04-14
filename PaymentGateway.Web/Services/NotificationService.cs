using Microsoft.AspNetCore.SignalR.Client;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Interfaces;

namespace PaymentGateway.Web.Services;

public class NotificationService : INotificationService, IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<NotificationService> _logger;

    public event Action<UserDto>? OnUserUpdated;
    public event Action<PaymentDto>? OnPaymentUpdated;
    public event Action<RequisiteDto>? OnRequisiteUpdated;
    public event Action<PaymentDto>? OnPaymentStatusChanged;

    public NotificationService(IConfiguration configuration, ILogger<NotificationService> logger)
    {
        _logger = logger;
        var hubUrl = configuration["ApiUrl"] + "/notificationHub";
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<UserDto>("UserUpdated", (user) =>
        {
            OnUserUpdated?.Invoke(user);
        });

        _hubConnection.On<PaymentDto>("PaymentUpdated", (payment) =>
        {
            OnPaymentUpdated?.Invoke(payment);
        });

        _hubConnection.On<RequisiteDto>("RequisiteUpdated", (requisite) =>
        {
            OnRequisiteUpdated?.Invoke(requisite);
        });

        _hubConnection.On<PaymentDto>("PaymentStatusChanged", (payment) =>
        {
            OnPaymentStatusChanged?.Invoke(payment);
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

    public Task NotifyUserUpdated(UserDto user)
    {
        return Task.CompletedTask;
    }

    public Task NotifyPaymentUpdated(PaymentDto payment)
    {
        return Task.CompletedTask;
    }

    public Task NotifyRequisiteUpdated(RequisiteDto requisite)
    {
        return Task.CompletedTask;
    }

    public Task NotifyRequisiteDeleted(Guid requisiteId, Guid userId)
    {
        return Task.CompletedTask;
    }

    public Task NotifyPaymentStatusChanged(PaymentDto payment)
    {
        return Task.CompletedTask;
    }
} 