using System.Text.Json.Serialization;
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
    protected readonly string HubUrl = $"{settings.Value.BaseAddress}/{settings.Value.HubName}";
    protected bool IsDisposed;

    private readonly AsyncRetryPolicy _reconnectionPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
            99,
            retryAttempt => TimeSpan.FromSeconds(Math.Min(30, Math.Pow(1.5, retryAttempt))),
            (exception, timeSpan, retryCount, _) =>
            {
                logger.LogDebug(exception,
                    "Попытка {RetryCount} подключения к SignalR не удалась. Следующая попытка через {RetryTimeSpan} секунд",
                    retryCount, timeSpan.TotalSeconds);
            }
        );
    
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

            if (HubConnection != null)
            {
                await HubConnection.DisposeAsync();
            }
            
            HubConnection = new HubConnectionBuilder()
                .WithUrl(HubUrl, options =>
                {
                    options.CloseTimeout = TimeSpan.FromMinutes(1);
                    options.SkipNegotiation = false;
                    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets | 
                                       Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents |
                                       Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
                })
                .WithAutomaticReconnect([
                    TimeSpan.FromSeconds(0),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(15),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromMinutes(1)
                ])
                .WithKeepAliveInterval(TimeSpan.FromSeconds(10))
                .WithServerTimeout(TimeSpan.FromMinutes(60))
                .WithStatefulReconnect()
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
                    options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
                })
                .Build();

            HubConnection.Reconnecting += error =>
            {
                logger.LogDebug("Попытка переподключения SignalR: {Error}", error?.Message);
                return Task.CompletedTask;
            };

            HubConnection.Reconnected += connectionId =>
            {
                logger.LogDebug("SignalR успешно переподключен: {ConnectionId}", connectionId);
                return Task.CompletedTask;
            };

            HubConnection.Closed += async (error) =>
            {
                if (IsDisposed) return;

                if (error != null)
                {
                    logger.LogWarning("Соединение с SignalR закрыто с ошибкой: {Error}", error.Message);

                    if (error.Message.Contains("runtime.lastError") ||
                        error.Message.Contains("message channel closed") ||
                        error.Message.Contains("A listener indicated an asynchronous response"))
                    {
                        logger.LogDebug("Обнаружена ошибка chrome runtime, инициируем полное переподключение");
                        
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

            await StartConnectionWithRetryAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при инициализации SignalR соединения");
        }
    }
    
    protected async Task StartConnectionWithRetryAsync()
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
                logger.LogDebug("Попытка подключения к SignalR хабу: {HubUrl}", HubUrl);
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
    
    public virtual async ValueTask DisposeAsync()
    {
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