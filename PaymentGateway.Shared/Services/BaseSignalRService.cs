using System.Net;
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
    public event EventHandler<bool>? ConnectionStateChanged;
    
    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            _isConnected = value;
            ConnectionStateChanged?.Invoke(this, value);
        }
    }
    
    protected HubConnection? HubConnection;
    
    public string? AccessToken { get; set; }

    protected bool IsDisposed;
    private bool _isStopped;

    private static TimeSpan CloseTimeout => TimeSpan.FromMinutes(1);
    private static TimeSpan KeepAliveInterval => TimeSpan.FromSeconds(10);
    private static TimeSpan ServerTimeout => TimeSpan.FromSeconds(30);
    private static HttpTransportType TransportTypes => HttpTransportType.WebSockets | HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling;
    private static bool SkipNegotiation => false;
    protected virtual Func<Task<string?>>? AccessTokenProvider => null;

    private static TimeSpan[] ReconnectionDelays =>
    [
        TimeSpan.Zero,
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(15),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(1)
    ];

    private readonly AsyncRetryPolicy _reconnectionPolicy = Policy
        .Handle<Exception>(ShouldRetry)
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

    private static bool ShouldRetry(Exception e)
    {
        if (e is HttpRequestException { StatusCode: HttpStatusCode.Unauthorized })
        {
            return false;
        }

        if (e.InnerException is HttpRequestException { StatusCode: HttpStatusCode.Unauthorized })
        {
            return false;
        }

        return true;
    }
    
    private string GetHubUrl()
    {
        var url = $"{settings.Value.BaseAddress}/{settings.Value.HubName}";
        if (!string.IsNullOrEmpty(AccessToken))
        {
            url += $"?access_token={AccessToken}";
        }
        return url;
    }

    public async Task<bool> WaitConnection(TimeSpan timeout)
    {
        using var ct = new CancellationTokenSource(timeout);
        while (!ct.IsCancellationRequested)
        {
            if (IsConnected) return true;

            try
            {
                await Task.Delay(1000, ct.Token);
            }
            catch (OperationCanceledException) { }
        }
        return false;
    }
    
    protected virtual async Task ConfigureHubConnectionAsync()
    {
        if (HubConnection != null)
        {
            await HubConnection.DisposeAsync();
        }
            
        HubConnection = new HubConnectionBuilder()
            .WithUrl(GetHubUrl(), options =>
            {
                options.CloseTimeout = CloseTimeout;
                options.SkipNegotiation = SkipNegotiation;
                options.Transports = TransportTypes;

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
            if (_isStopped) return Task.CompletedTask;
            
            IsConnected = false;
            logger.LogDebug("Попытка переподключения SignalR: {Error}", error?.Message);
            return Task.CompletedTask;
        };

        HubConnection.Reconnected += connectionId =>
        {
            if (_isStopped) return Task.CompletedTask;
            
            IsConnected = true;
            logger.LogDebug("SignalR успешно переподключен: {ConnectionId}", connectionId);
            return Task.CompletedTask;
        };

        HubConnection.Closed += async (error) =>
        {
            IsConnected = false;
            if (IsDisposed || _isStopped) return;

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
            _isStopped = false;
            if (HubConnection is { State: HubConnectionState.Connected })
            {
                logger.LogDebug("SignalR соединение уже установлено");
                return;
            }

            await ConfigureHubConnectionAsync();
            await StartConnectionWithRetryAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при инициализации SignalR соединения");
        }
    }

    protected virtual async Task StopAsync()
    {
        if (HubConnection == null) return;
        
        try
        {
            _isStopped = true;
            IsConnected = false;
            await HubConnection.StopAsync();
            logger.LogDebug("SignalR соединение остановлено");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при остановке SignalR соединения");
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
                IsConnected = false;
                logger.LogError("Соединение с SignalR не инициализировано");
                throw new InvalidOperationException("Соединение с SignalR не инициализировано");
            }

            if (HubConnection.State == HubConnectionState.Connected)
            {
                IsConnected = true;
                logger.LogDebug("SignalR уже подключен");
                return;
            }

            try
            {
                logger.LogDebug("Попытка подключения к SignalR хабу: {HubUrl}", GetHubUrl());
                await HubConnection.StartAsync();
                IsConnected = true;
                logger.LogDebug("SignalR соединение установлено успешно");
            }
            catch (Exception ex)
            {
                IsConnected = false;
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
    
    public virtual async ValueTask DisposeAsync()
    {
        if (IsDisposed) return;
        
        try
        {
            if (HubConnection != null)
            {
                IsConnected = false;
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