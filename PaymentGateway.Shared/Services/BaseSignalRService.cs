﻿using System.ComponentModel;
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
    ILogger<BaseSignalRService> logger) : IAsyncDisposable, INotifyPropertyChanged
{
    public event EventHandler<bool>? ConnectionStateChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            _isConnected = value;
            ConnectionStateChanged?.Invoke(this, value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
        }
    }

    private string _accessToken = string.Empty;
    public string AccessToken
    {
        get => _accessToken;
        set
        {
            _accessToken = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccessToken)));
        }
    }
    
    private bool _isInitializing;
    public bool IsInitializing
    {
        get => _isInitializing;
        set
        {
            _isInitializing = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInitializing)));
        }
    }
    
    private bool _isServiceUnavailable;
    public bool IsServiceUnavailable
    {
        get => _isServiceUnavailable;
        set
        {
            _isServiceUnavailable = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsServiceUnavailable)));
        }
    }
    
    private bool _isLoggedIn;
    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set
        {
            _isLoggedIn = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoggedIn)));
        }
    }

    protected bool IsDisposed;
    private bool _isStopped;
    
    protected HubConnection? HubConnection;

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
                    "Попытка {RetryCount} подключения не удалась. Следующая попытка через {RetryTimeSpan} секунд",
                    retryCount, timeSpan.TotalSeconds);
            }
        );

    private static bool ShouldRetry(Exception e)
    {
        switch (e)
        {
            case HttpRequestException { StatusCode: HttpStatusCode.Unauthorized }:
                return false;
            case HttpRequestException
            {
                StatusCode: HttpStatusCode.ServiceUnavailable or
                HttpStatusCode.GatewayTimeout or
                HttpStatusCode.BadGateway or
                HttpStatusCode.InternalServerError
            }:
                return false;
            default:
                return true;
        }
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
    
    protected virtual async Task ConfigureHubConnectionAsync()
    {
        if (HubConnection is not null)
        {
            await HubConnection.DisposeAsync();
        }
            
        HubConnection = new HubConnectionBuilder()
            .WithUrl(GetHubUrl(), options =>
            {
                options.CloseTimeout = CloseTimeout;
                options.SkipNegotiation = SkipNegotiation;
                options.Transports = TransportTypes;

                if (AccessTokenProvider is not null)
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
            IsServiceUnavailable = true;
            logger.LogDebug("Попытка переподключения: {Error}", error?.Message);
            return Task.CompletedTask;
        };

        HubConnection.Reconnected += connectionId =>
        {
            if (_isStopped) return Task.CompletedTask;
            
            IsConnected = true;
            IsServiceUnavailable = false;
            logger.LogDebug("Успешное переподключение: {ConnectionId}", connectionId);
            return Task.CompletedTask;
        };

        HubConnection.Closed += async (error) =>
        {
            IsConnected = false;
            if (IsDisposed || _isStopped) return;
            
            if (error is not null)
            {
                IsServiceUnavailable = true;
                logger.LogWarning("Соединение закрыто с ошибкой: {Error}", error.Message);

                if (error is IOException || 
                    error.Message.Contains("runtime.lastError") ||
                    error.Message.Contains("message channel closed") ||
                    error.Message.Contains("A listener indicated an asynchronous response"))
                {
                    logger.LogDebug("Обнаружена сетевая ошибка, инициируем полное переподключение");
                    
                    if (HubConnection is not null)
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

    public virtual async Task<bool> InitializeAsync()
    {
        if (IsDisposed) return false;
        
        try
        {
            _isStopped = false;
            if (HubConnection is { State: HubConnectionState.Connected })
            {
                logger.LogDebug("Соединение уже установлено");
                IsLoggedIn = true;
                return true;
            }

            await ConfigureHubConnectionAsync();
            await StartConnectionWithRetryAsync();

            IsServiceUnavailable = false;
            IsLoggedIn = true;
            return true;
        }
        catch (Exception e)
        {
            switch (e)
            {
                case HttpRequestException { StatusCode: HttpStatusCode.Unauthorized }:
                    IsLoggedIn = false;
                    logger.LogError(e, "Токен недействителен");
                    break;
                case HttpRequestException
                {
                    StatusCode: HttpStatusCode.ServiceUnavailable or
                    HttpStatusCode.GatewayTimeout or
                    HttpStatusCode.BadGateway or
                    HttpStatusCode.InternalServerError
                }:
                    IsServiceUnavailable = true;
                    logger.LogError(e, "Сервис временно недоступен");
                    break;
                default:
                    logger.LogError(e, "Ошибка при инициализации соединения");
                    break;
            }
            return false;
        }
    }

    protected virtual async Task StopAsync()
    {
        if (HubConnection is null) return;
        
        try
        {
            _isStopped = true;
            IsConnected = false;
            await HubConnection.StopAsync();
            logger.LogDebug("Соединение остановлено");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при остановке соединения");
        }
    }

    private async Task StartConnectionWithRetryAsync()
    {
        if (IsDisposed) return;
        
        logger.LogDebug("Начало попытки подключения с политикой переподключения");
        
        await _reconnectionPolicy.ExecuteAsync(async () =>
        {
            if (HubConnection is null)
            {
                IsConnected = false;
                IsLoggedIn = false;
            }

            if (HubConnection!.State == HubConnectionState.Connected)
            {
                IsConnected = true;
                IsLoggedIn = true;
                logger.LogDebug("Соединение уже установлено");
                return;
            }

            try
            {
                logger.LogDebug("Попытка подключения к сервису");
                await HubConnection.StartAsync();
                IsConnected = true;
                IsLoggedIn = true;
                logger.LogDebug("Соединение установлено");
            }
            catch (Exception e)
            {
                IsConnected = false;
                logger.LogError(e, "Ошибка при подключении к сервису");
                throw;
            }
        });
    }
    
    protected void Subscribe<T>(string eventName, Action<T> handler)
    {
        try
        {
            logger.LogDebug("Подписка на событие: {EventName}", eventName);
            
            if (HubConnection is null)
            {
                logger.LogDebug("Подписка на событие {EventName} не выполнена - соединение не инициализировано", eventName);
                return;
            }
            
            if (HubConnection.State != HubConnectionState.Connected)
            {
                logger.LogDebug("Подписка на событие {EventName} не выполнена - соединение не подключено (Текущее состояние: {State})", 
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
                    logger.LogError(ex, "Ошибка при обработке события {EventName}", eventName);
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
            if (HubConnection is not null)
            {
                IsConnected = false;
                IsLoggedIn = false;
                await HubConnection.DisposeAsync();
                HubConnection = null!;
                
                logger.LogDebug("Глобальное состояние инициализации сброшено");
            }
            
            IsDisposed = true;
            
            logger.LogDebug("Соединение успешно закрыто");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при утилизации соединения");
        }
    }
}