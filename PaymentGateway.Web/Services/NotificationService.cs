using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR.Client;
using PaymentGateway.Shared;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.Transaction;
using PaymentGateway.Shared.DTOs.User;
using Polly;
using Polly.Retry;
using PaymentGateway.Shared.DTOs.Chat;
using PaymentGateway.Shared.Types;

namespace PaymentGateway.Web.Services;

public class NotificationService(
    IOptions<ApiSettings> settings,
    IServiceProvider serviceProvider,
    ILogger<NotificationService> logger) : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl = $"{settings.Value.BaseAddress}/notificationHub";
    private Timer? _pingTimer;
    private const int pingInterval = 10000;
    private bool _isDisposed;

    private readonly AsyncRetryPolicy _reconnectionPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
            20,
            retryAttempt => TimeSpan.FromSeconds(Math.Min(30, Math.Pow(1.5, retryAttempt))),
            (exception, timeSpan, retryCount, _) =>
            {
                logger.LogDebug(exception,
                    "Попытка {RetryCount} подключения к SignalR не удалась. Следующая попытка через {RetryTimeSpan} секунд",
                    retryCount, timeSpan.TotalSeconds);
            }
        );

    private void Subscribe<T>(string eventName, Action<T> handler)
    {
        try
        {
            logger.LogDebug("Подписка на событие: {EventName}", eventName);
            
            if (_hubConnection == null)
            {
                logger.LogDebug("Подписка на событие {EventName} не выполнена - соединение SignalR не инициализировано", eventName);
                return;
            }
            
            if (_hubConnection.State != HubConnectionState.Connected)
            {
                logger.LogDebug("Подписка на событие {EventName} не выполнена - соединение SignalR не подключено (Текущее состояние: {State})", 
                    eventName, _hubConnection.State);
                return;
            }
            
            _hubConnection.Remove(eventName);

            _hubConnection.On<T>(eventName, (data) =>
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
            if (_hubConnection is { State: HubConnectionState.Connected }) 
            {
                await _hubConnection.InvokeAsync("Ping", cts.Token);
                logger.LogDebug("Ping отправлен успешно");
            }
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
        
        logger.LogDebug("Начало попытки подключения SignalR с политикой переподключения");
        
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
                logger.LogDebug("Попытка подключения к SignalR хабу: {HubUrl}", _hubUrl);
                await _hubConnection.StartAsync();
                logger.LogDebug("SignalR соединение установлено успешно");

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
                }, null, pingInterval, pingInterval);
                
                logger.LogDebug("Таймер ping успешно настроен");
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
        
        using var scope = serviceProvider.CreateScope();
        var authStateProvider = scope.ServiceProvider.GetRequiredService<CustomAuthStateProvider>();
        
        try
        {
            logger.LogDebug("Начало инициализации NotificationService");
            
            if (_hubConnection is { State: HubConnectionState.Connected })
            {
                logger.LogDebug("SignalR соединение уже установлено");
                return;
            }

            var token = await authStateProvider.GetTokenFromLocalStorageAsync();

            if (string.IsNullOrEmpty(token))
            {
                logger.LogDebug("Токен не найден в localStorage");
                return;
            }

            var authState = await authStateProvider.GetAuthenticationStateAsync();
            if (!authState.User.Identity?.IsAuthenticated ?? true)
            {
                logger.LogWarning("Пользователь не аутентифицирован");
                return;
            }

            var username = authState.User.FindFirst(ClaimTypes.Name)?.Value;

            logger.LogDebug("Инициализация SignalR для пользователя: {Username}", username);

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
                    
                    options.CloseTimeout = TimeSpan.FromMinutes(1);
                    options.SkipNegotiation = false;
                    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
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
                .WithKeepAliveInterval(TimeSpan.FromSeconds(15))
                .WithServerTimeout(TimeSpan.FromMinutes(60))
                .WithStatefulReconnect()
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
                    options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
                })
                .Build();

            _hubConnection.On("KeepAlive", () => Task.CompletedTask);

            _hubConnection.Reconnecting += error =>
            {
                logger.LogDebug("Попытка переподключения SignalR: {Error}", error?.Message);
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += connectionId =>
            {
                logger.LogDebug("SignalR успешно переподключен: {ConnectionId}", connectionId);
                return Task.CompletedTask;
            };

            _hubConnection.Closed += async (error) =>
            {
                if (_isDisposed) return;
                
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
                        logger.LogDebug("Обнаружена ошибка chrome runtime, инициируем полное переподключение");
                        
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
            
            logger.LogDebug("Глобальное состояние инициализации SignalR установлено");
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
            logger.LogDebug("Начало утилизации NotificationService");
            
            Unsubscribe(SignalREvents.UserUpdated);
            Unsubscribe(SignalREvents.UserDeleted);
            Unsubscribe(SignalREvents.RequisiteUpdated);
            Unsubscribe(SignalREvents.RequisiteDeleted);
            Unsubscribe(SignalREvents.PaymentUpdated);
            Unsubscribe(SignalREvents.PaymentDeleted);
            Unsubscribe(SignalREvents.ChatMessageReceived);
            Unsubscribe(SignalREvents.UserConnected);
            Unsubscribe(SignalREvents.UserDisconnected);
            Unsubscribe(SignalREvents.ChangeRequisiteAssignmentAlgorithm);
            
            if (_pingTimer != null)
            {
                await _pingTimer.DisposeAsync();
                _pingTimer = null;
            }
            
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null!;
                
                logger.LogDebug("Глобальное состояние инициализации SignalR сброшено");
            }
            
            logger.LogDebug("Соединение SignalR успешно закрыто");
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
    
    #region Transaction

    public void SubscribeToTransactionUpdates(Action<TransactionDto> handler)
    {
        Subscribe(SignalREvents.TransactionUpdated, handler);
    }

    public void UnsubscribeFromTransactionUpdates()
    {
        Unsubscribe(SignalREvents.TransactionUpdated);
    }

    #endregion
    
    #region Payment

    public void SubscribeToPaymentUpdates(Action<PaymentDto> handler)
    {
        Subscribe(SignalREvents.PaymentUpdated, handler);
    }

    public void UnsubscribeFromPaymentUpdates()
    {
        Unsubscribe(SignalREvents.PaymentUpdated);
    }

    public void SubscribeToPaymentDeletions(Action<Guid> handler)
    {
        Subscribe(SignalREvents.PaymentDeleted, handler);
    }

    public void UnsubscribeFromPaymentDeletions()
    {
        Unsubscribe(SignalREvents.PaymentDeleted);
    }

    #endregion
    
    #region Requisite

    public void SubscribeToRequisiteUpdates(Action<RequisiteDto> handler)
    {
        Subscribe(SignalREvents.RequisiteUpdated, handler);
    }
    
    public void UnsubscribeFromRequisiteUpdates()
    {
        Unsubscribe(SignalREvents.RequisiteUpdated);
    }

    public void SubscribeToRequisiteDeletions(Action<Guid> handler)
    {
        Subscribe(SignalREvents.RequisiteDeleted, handler);
    }
    
    public void UnsubscribeFromRequisiteDeletions()
    {
        Unsubscribe(SignalREvents.RequisiteDeleted);
    }

    public void SubscribeToChangeRequisiteAssignmentAlgorithm(Action<int> handler)
    {
        Subscribe(SignalREvents.ChangeRequisiteAssignmentAlgorithm, handler);
    }

    public void UnsubscribeFromChangeRequisiteAssignmentAlgorithm()
    {
        Unsubscribe(SignalREvents.ChangeRequisiteAssignmentAlgorithm);
    }

    #endregion

    #region Chat
    
    public async Task<List<UserState>> GetAllUsers()
    {
        try
        {
            return await _hubConnection!.InvokeAsync<List<UserState>>(SignalREvents.GetAllUsers);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка получения списка пользователей");
            return [];
        }
    }

    public void SubscribeToUserConnected(Action<UserState> handler)
    {
        Subscribe(SignalREvents.UserConnected, handler);
    }

    public void UnsubscribeFromUserConnected()
    {
        Unsubscribe(SignalREvents.UserConnected);
    }

    public void SubscribeToUserDisconnected(Action<UserState> handler)
    {
        Subscribe(SignalREvents.UserDisconnected, handler);
    }

    public void UnsubscribeFromUserDisconnected()
    {
        Unsubscribe(SignalREvents.UserDisconnected);
    }
    
    public void SubscribeToChatMessages(Action<ChatMessageDto> handler)
    {
        Subscribe(SignalREvents.ChatMessageReceived, handler);
    }

    public void UnsubscribeFromChatMessages()
    {
        Unsubscribe(SignalREvents.ChatMessageReceived);
    }
    
    public async Task SendChatMessage(string message)
    {
        try
        {
            await _hubConnection!.InvokeAsync(SignalREvents.SendChatMessage, message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при отправке сообщения в чат");
        }
    }

    public async Task<List<ChatMessageDto>> GetChatHistory()
    {
        try
        {
            return await _hubConnection!.InvokeAsync<List<ChatMessageDto>>(SignalREvents.GetChatMessages);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка загрузки истории чата");
            return [];
        }
    }

    #endregion
    
}