using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.Shared.Types;
using Polly;
using Polly.Retry;

namespace PaymentGateway.Shared.Services;

public class BaseSignalRService(
    IOptions<ApiSettings> settings,
    ILogger<BaseSignalRService> logger) : IAsyncDisposable
{
    public bool IsConnected => HubConnection is { State: HubConnectionState.Connected };
    
    protected HubConnection? HubConnection;
    private readonly string _hubUrl = $"{settings.Value.BaseAddress}/{settings.Value.HubName}";
    protected bool IsDisposed;

    private Timer? _pingTimer;
    private const int pingIntervalSeconds = 5;

    private static TimeSpan CloseTimeout => TimeSpan.FromMinutes(1);
    private static TimeSpan KeepAliveInterval => TimeSpan.FromSeconds(10);
    private static TimeSpan ServerTimeout => TimeSpan.FromSeconds(30);
    private static HttpTransportType TransportTypes => HttpTransportType.WebSockets | HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling;
    private static bool SkipNegotiation => false;
    protected virtual Func<Task<string?>>? AccessTokenProvider => null;
    private static Dictionary<string, string>? AdditionalHeaders => new() { { "Connection", "keep-alive" } };

    private static TimeSpan[] ReconnectionDelays =>
    [
        TimeSpan.Zero,
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(15),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(3),
        TimeSpan.FromMinutes(4),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(6),
        TimeSpan.FromMinutes(7),
        TimeSpan.FromMinutes(8),
        TimeSpan.FromMinutes(9),
        TimeSpan.FromMinutes(10),
    ];

    private readonly AsyncRetryPolicy _reconnectionPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
            99,
            retryAttempt => 
            {
                return retryAttempt switch
                {
                    <= 3 => TimeSpan.FromSeconds(1),
                    <= 6 => TimeSpan.FromSeconds(2),
                    <= 9 => TimeSpan.FromSeconds(5),
                    _ => TimeSpan.FromSeconds(10)
                };
            },
            (exception, timeSpan, retryCount, _) =>
            {
                logger.LogDebug(exception,
                    "Попытка {RetryCount} подключения к SignalR не удалась. Следующая попытка через {RetryTimeSpan} секунд",
                    retryCount, timeSpan.TotalSeconds);
            }
        );

    protected virtual async Task ConfigureHubConnectionAsync()
    {
        if (HubConnection != null)
        {
            await HubConnection.DisposeAsync();
        }
            
        HubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options =>
            {
                options.CloseTimeout = CloseTimeout;
                options.SkipNegotiation = SkipNegotiation;
                options.Transports = TransportTypes;
                
                if (AdditionalHeaders != null)
                {
                    foreach (var header in AdditionalHeaders)
                    {
                        options.Headers[header.Key] = header.Value;
                    }
                }

                if (AccessTokenProvider != null)
                {
                    options.AccessTokenProvider = AccessTokenProvider;
                }
            })
            .WithAutomaticReconnect(ReconnectionDelays)
            .WithKeepAliveInterval(KeepAliveInterval)
            .WithServerTimeout(ServerTimeout)
            .WithStatefulReconnect()
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
                options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
            })
            .Build();

        ConfigureHubEvents();
    }

    private void ConfigureHubEvents()
    {
        HubConnection!.Reconnecting += error =>
        {
            logger.LogDebug("Попытка переподключения SignalR: {Error}", error?.Message);
            return Task.CompletedTask;
        };

        HubConnection.Reconnected += connectionId =>
        {
            logger.LogDebug("SignalR успешно переподключен: {ConnectionId}", connectionId);
            StartPingTimer();
            return Task.CompletedTask;
        };

        HubConnection.Closed += async (error) =>
        {
            StopPingTimer();
            if (IsDisposed) return;

            if (error != null)
            {
                logger.LogWarning("Соединение с SignalR закрыто с ошибкой: {Error}", error.Message);

                if (error is IOException || 
                    error.Message.Contains("runtime.lastError") ||
                    error.Message.Contains("message channel closed") ||
                    error.Message.Contains("A listener indicated an asynchronous response"))
                {
                    logger.LogDebug("Обнаружена сетевая ошибка, инициируем полное переподключение");
                    
                    if (HubConnection != null)
                    {
                        await HubConnection.DisposeAsync();
                        HubConnection = null;
                    }
                    
                    await Task.Delay(1000);
                    
                    await InitializeAsync();
                    return;
                }
            }

            await StartConnectionWithRetryAsync();
        };
    }

    public virtual async Task InitializeAsync()
    {
        if (IsDisposed) return;
        
        try
        {
            if (HubConnection is { State: HubConnectionState.Connected })
            {
                logger.LogDebug("SignalR соединение уже установлено");
                return;
            }

            await ConfigureHubConnectionAsync();
            await StartConnectionWithRetryAsync();
            StartPingTimer();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при инициализации SignalR соединения");
        }
    }

    private async Task StartConnectionWithRetryAsync()
    {
        if (IsDisposed) return;
        
        logger.LogDebug("Начало попытки подключения SignalR с политикой переподключения");
        
        await _reconnectionPolicy.ExecuteAsync(async () =>
        {
            if (HubConnection == null)
            {
                logger.LogError("Соединение с SignalR не инициализировано");
                throw new InvalidOperationException("Соединение с SignalR не инициализировано");
            }

            if (HubConnection.State == HubConnectionState.Connected)
            {
                logger.LogDebug("SignalR уже подключен");
                return;
            }

            try
            {
                logger.LogDebug("Попытка подключения к SignalR хабу: {HubUrl}", _hubUrl);
                await HubConnection.StartAsync();
                logger.LogDebug("SignalR соединение установлено успешно");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при подключении SignalR");
                throw;
            }
        });
    }
    
    protected void Subscribe<T>(string eventName, Action<T> handler)
    {
        try
        {
            logger.LogDebug("Подписка на событие: {EventName}", eventName);
            
            if (HubConnection == null)
            {
                logger.LogDebug("Подписка на событие {EventName} не выполнена - соединение SignalR не инициализировано", eventName);
                return;
            }
            
            if (HubConnection.State != HubConnectionState.Connected)
            {
                logger.LogDebug("Подписка на событие {EventName} не выполнена - соединение SignalR не подключено (Текущее состояние: {State})", 
                    eventName, HubConnection.State);
                return;
            }
            
            HubConnection.Remove(eventName);

            HubConnection.On<T>(eventName, (data) =>
            {
                try
                {
                    logger.LogDebug("Получено событие: {EventName}", eventName);
                    handler(data);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка при обработке события SignalR {EventName}", eventName);
                }

                return Task.CompletedTask;
            });
            
            logger.LogDebug("Успешная подписка на событие: {EventName}", eventName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Не удалось подписаться на событие {EventName}", eventName);
        }
    }
    
    protected void Unsubscribe(string eventName)
    {
        try
        {
            HubConnection?.Remove(eventName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Не удалось отписаться от события {EventName}", eventName);
        }
    }
    
    protected void StartPingTimer()
    {
        StopPingTimer();
        _pingTimer = new Timer(async void (_) =>
        {
            try
            {
                if (HubConnection?.State != HubConnectionState.Connected) return;
                await HubConnection.InvokeAsync("Ping");
                logger.LogDebug("Ping отправлен");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при отправке ping");
            }
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(pingIntervalSeconds));
    }

    protected void StopPingTimer()
    {
        if (_pingTimer == null) return;
        _pingTimer.Dispose();
        _pingTimer = null;
    }

    public virtual async ValueTask DisposeAsync()
    {
        StopPingTimer();
        if (IsDisposed) return;
        
        try
        {
            if (HubConnection != null)
            {
                await HubConnection.DisposeAsync();
                HubConnection = null!;
                
                logger.LogDebug("Глобальное состояние инициализации SignalR сброшено");
            }
            
            IsDisposed = true;
            
            logger.LogDebug("Соединение SignalR успешно закрыто");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при утилизации SignalR соединения");
        }
    }
}