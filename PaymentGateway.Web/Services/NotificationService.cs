using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR.Client;
using PaymentGateway.Shared;
using PaymentGateway.Shared.DTOs.User;
using Polly;
using Polly.Retry;

namespace PaymentGateway.Web.Services;

public class NotificationService(
    IOptions<ApiSettings> settings,
    CustomAuthStateProvider authStateProvider,
    ILogger<NotificationService> logger) : IAsyncDisposable
{
    private static bool _globalInitialized;
    private static readonly Lock Lock = new();
    
    private readonly string _instanceId = Guid.NewGuid().ToString("N")[..8];
    private HubConnection? _hubConnection;
    private readonly string _hubUrl = $"{settings.Value.BaseAddress}/notificationHub";
    private Timer? _pingTimer;
    private const int PingInterval = 10000;
    private bool _isDisposed;

    public event EventHandler<bool>? ConnectionChanged;
    
    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    private readonly AsyncRetryPolicy _reconnectionPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
            20,
            retryAttempt => TimeSpan.FromSeconds(Math.Min(30, Math.Pow(1.5, retryAttempt))),
            (exception, timeSpan, retryCount, _) =>
            {
                logger.LogWarning(exception,
                    "Попытка {RetryCount} подключения к SignalR не удалась. Следующая попытка через {RetryTimeSpan} секунд",
                    retryCount, timeSpan.TotalSeconds);
            }
        );

    private void Subscribe<T>(string eventName, Action<T> handler)
    {
        try
        {
            logger.LogInformation("Подписка на событие: {EventName}", eventName);
            
            if (_hubConnection == null)
            {
                logger.LogWarning("Подписка на событие {EventName} не выполнена - соединение SignalR не инициализировано", eventName);
                return;
            }
            
            if (_hubConnection.State != HubConnectionState.Connected)
            {
                logger.LogWarning("Подписка на событие {EventName} не выполнена - соединение SignalR не подключено (Текущее состояние: {State})", 
                    eventName, _hubConnection.State);
                return;
            }
            
            _hubConnection.Remove(eventName);

            _hubConnection.On<T>(eventName, (data) =>
            {
                try
                {
                    logger.LogInformation("Получено событие: {EventName}", eventName);
                    handler(data);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка при обработке события SignalR {EventName}", eventName);
                }

                return Task.CompletedTask;
            });
            
            logger.LogInformation("Успешная подписка на событие: {EventName}", eventName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Не удалось подписаться на событие {EventName}", eventName);
        }
    }

    private void Unsubscribe(string eventName)
    {
        try
        {
            _hubConnection?.Remove(eventName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Не удалось отписаться от события {EventName}", eventName);
        }
    }

    private async Task SendPingAsync(object? state)
    {
        if (_isDisposed) return;
        
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            if (_hubConnection != null) await _hubConnection.InvokeAsync("Ping", cts.Token);
            logger.LogDebug("Ping отправлен успешно");
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Ping был отменен из-за таймаута");
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Connection was stopped before invocation result was received") ||
                ex.Message.Contains("Connection not active") ||
                ex.Message.Contains("The connection was stopped during invocation"))
            {
                logger.LogDebug("Соединение было закрыто во время ping");
                return;
            }

            logger.LogDebug(ex, "Ошибка отправки ping");
        }
    }

    private async Task StartConnectionWithRetryAsync()
    {
        if (_isDisposed) return;
        
        logger.LogInformation("Начало попытки подключения SignalR с политикой переподключения");
        
        await _reconnectionPolicy.ExecuteAsync(async () =>
        {
            if (_hubConnection == null)
            {
                logger.LogError("Соединение с SignalR не инициализировано");
                throw new InvalidOperationException("Соединение с SignalR не инициализировано");
            }

            if (_hubConnection.State == HubConnectionState.Connected)
            {
                logger.LogDebug("SignalR уже подключен");
                return;
            }

            try
            {
                logger.LogInformation("Попытка подключения к SignalR хабу: {HubUrl}", _hubUrl);
                await _hubConnection.StartAsync();
                logger.LogInformation("SignalR соединение установлено успешно");
                
                ConnectionChanged?.Invoke(this, true);

                if (_pingTimer != null)
                {
                    await _pingTimer.DisposeAsync();
                }

                _pingTimer = new Timer(async void (_) =>
                {
                    try
                    {
                        await SendPingAsync(null);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Ошибка при отправке ping");
                    }
                }, null, PingInterval, PingInterval);
                
                logger.LogInformation("Таймер ping успешно настроен");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при подключении SignalR");
                throw;
            }
        });
    }

    public async Task InitializeAsync()
    {
        if (_isDisposed) return;
        
        try
        {
            logger.LogInformation("[{InstanceId}] Начало инициализации NotificationService", _instanceId);
            
            lock (Lock)
            {
                if (_globalInitialized && _hubConnection?.State == HubConnectionState.Connected)
                {
                    logger.LogDebug("[{InstanceId}] SignalR соединение уже инициализировано глобально", _instanceId);
                    return;
                }
            }
            
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                logger.LogDebug("[{InstanceId}] SignalR соединение уже установлено", _instanceId);
                return;
            }

            var token = await authStateProvider.GetTokenFromLocalStorageAsync();

            if (string.IsNullOrEmpty(token))
            {
                logger.LogWarning("Токен не найден в localStorage");
                return;
            }

            var authState = await authStateProvider.GetAuthenticationStateAsync();
            if (!authState.User.Identity?.IsAuthenticated ?? true)
            {
                logger.LogWarning("Пользователь не аутентифицирован");
                return;
            }

            var username = authState.User.FindFirst(ClaimTypes.Name)?.Value;

            logger.LogInformation("Инициализация SignalR для пользователя: {Username}", username);

            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
            
            if (_pingTimer != null)
            {
                await _pingTimer.DisposeAsync();
            }

            _pingTimer = null;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl, options =>
                {
                    options.AccessTokenProvider = async () => 
                    {
                        var currentToken = await authStateProvider.GetTokenFromLocalStorageAsync();
                        
                        if (string.IsNullOrEmpty(currentToken))
                        {
                            logger.LogWarning("Токен отсутствует при запросе AccessTokenProvider");
                            await authStateProvider.MarkUserAsLoggedOut();
                            throw new InvalidOperationException("Токен не найден");
                        }
                        
                        return currentToken;
                    };
                    
                    options.CloseTimeout = TimeSpan.FromMinutes(60);
                    options.SkipNegotiation = false;
                    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                                         Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents;
                })
                .WithAutomaticReconnect([
                    TimeSpan.FromSeconds(0),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(20),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(2),
                    TimeSpan.FromMinutes(5),
                    TimeSpan.FromMinutes(10),
                    TimeSpan.FromMinutes(15)
                ])
                .WithKeepAliveInterval(TimeSpan.FromSeconds(30))
                .WithServerTimeout(TimeSpan.FromMinutes(60))
                .WithStatefulReconnect()
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.ReferenceHandler =
                        System.Text.Json.Serialization.ReferenceHandler.Preserve;
                    options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.PayloadSerializerOptions.MaxDepth = 128;
                })
                .Build();

            _hubConnection.On("KeepAlive", () => Task.CompletedTask);

            _hubConnection.Reconnecting += error =>
            {
                logger.LogWarning("Попытка переподключения SignalR: {Error}", error?.Message);
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += connectionId =>
            {
                logger.LogInformation("SignalR успешно переподключен: {ConnectionId}", connectionId);
                return Task.CompletedTask;
            };

            _hubConnection.Closed += async (error) =>
            {
                if (_isDisposed) return;
                
                ConnectionChanged?.Invoke(this, false);
                
                if (_pingTimer != null)
                {
                    await _pingTimer.DisposeAsync();
                }

                _pingTimer = null;

                if (error != null)
                {
                    logger.LogWarning("Соединение с SignalR закрыто с ошибкой: {Error}", error.Message);

                    if (error.Message.Contains("Unauthorized") || error.Message.Contains("401"))
                    {
                        await authStateProvider.MarkUserAsLoggedOut();
                        return;
                    }

                    if (error.Message.Contains("runtime.lastError") ||
                        error.Message.Contains("message channel closed") ||
                        error.Message.Contains("A listener indicated an asynchronous response"))
                    {
                        logger.LogWarning("Обнаружена ошибка chrome runtime, инициируем полное переподключение");
                        
                        if (_hubConnection != null)
                        {
                            await _hubConnection.DisposeAsync();
                            _hubConnection = null;
                        }
                        
                        await Task.Delay(1000);
                        
                        await InitializeAsync();
                        return;
                    }
                }

                await StartConnectionWithRetryAsync();
            };

            await StartConnectionWithRetryAsync();
            
            lock (Lock)
            {
                _globalInitialized = true;
            }
            
            logger.LogInformation("[{InstanceId}] Глобальное состояние инициализации SignalR установлено", _instanceId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при инициализации SignalR соединения");
            if (ex.Message.Contains("Unauthorized") || ex.Message.Contains("401"))
            {
                await authStateProvider.MarkUserAsLoggedOut();
            }
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        
        try
        {
            _isDisposed = true;
            logger.LogInformation("[{InstanceId}] Начало утилизации NotificationService", _instanceId);
            
            try
            {
                ConnectionChanged?.Invoke(this, false);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка при уведомлении о закрытии соединения");
            }
            
            Unsubscribe(SignalREvents.UserUpdated);
            Unsubscribe(SignalREvents.UserDeleted);
            Unsubscribe(SignalREvents.RequisiteUpdated);
            Unsubscribe(SignalREvents.RequisiteDeleted);
            Unsubscribe(SignalREvents.PaymentUpdated);
            Unsubscribe(SignalREvents.PaymentDeleted);
            Unsubscribe(SignalREvents.ChatMessageReceived);
            Unsubscribe(SignalREvents.UserConnected);
            Unsubscribe(SignalREvents.UserDisconnected);
            
            if (_pingTimer != null)
            {
                await _pingTimer.DisposeAsync();
                _pingTimer = null;
            }
            
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null!;
                
                lock (Lock)
                {
                    _globalInitialized = false;
                }
                logger.LogInformation("[{InstanceId}] Глобальное состояние инициализации SignalR сброшено", _instanceId);
            }
            
            logger.LogInformation("[{InstanceId}] Соединение SignalR успешно закрыто", _instanceId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при утилизации SignalR соединения");
        }
    }
    
    #region User

    public void SubscribeToUserUpdates(Action<UserDto> handler)
    {
        logger.LogInformation("Подписка на обновления пользователей");
        Subscribe(SignalREvents.UserUpdated, handler);
    }
    
    public void UnsubscribeFromUserUpdates()
    {
        logger.LogInformation("Отписка от обновлений пользователей");
        Unsubscribe(SignalREvents.UserUpdated);
    }

    public void SubscribeToUserDeletions(Action<Guid> handler)
    {
        logger.LogInformation("Подписка на удаление пользователей");
        Subscribe(SignalREvents.UserDeleted, handler);
    }
    
    public void UnsubscribeFromUserDeletions()
    {
        logger.LogInformation("Отписка от удаления пользователей");
        Unsubscribe(SignalREvents.UserDeleted);
    }

    #endregion
}