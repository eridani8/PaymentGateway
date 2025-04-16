using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;
using System.Security.Claims;
using PaymentGateway.Shared;
using PaymentGateway.Shared.DTOs.Payment;
using Polly;
using Polly.Retry;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.DependencyInjection;

namespace PaymentGateway.Web.Services;

public class SignalRService(
    IOptions<ApiSettings> settings,
    ILogger<SignalRService> logger,
    CustomAuthStateProvider authStateProvider)
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl = $"{settings.Value.BaseAddress}/notificationHub";
    private bool _isDisposing;
    private bool _isDisposed;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private readonly AsyncRetryPolicy _reconnectionPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
            10,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.5, retryAttempt)),
            (exception, timeSpan, retryCount, _) =>
            {
                logger.LogWarning(exception,
                    "Попытка {RetryCount} подключения к SignalR не удалась. Следующая попытка через {RetryTimeSpan} секунд",
                    retryCount, timeSpan.TotalSeconds);
            }
        );

    private void EnsureConnectionInitialized()
    {
        if (_isDisposing || _isDisposed)
        {
            throw new ObjectDisposedException(nameof(SignalRService), "SignalR сервис находится в процессе утилизации");
        }

        if (_hubConnection == null)
        {
            throw new InvalidOperationException("SignalR соединение не инициализировано");
        }
    }

    private void Subscribe<T>(string eventName, Action<T> handler)
    {
        try
        {
            EnsureConnectionInitialized();
            
            _hubConnection!.Remove(eventName);
            
            _hubConnection!.On<T>(eventName, (data) => {
                try 
                {
                    handler(data);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка при обработке события SignalR {EventName}", eventName);
                }
                return Task.CompletedTask;
            });
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
            if (_isDisposing || _isDisposed || _hubConnection == null)
            {
                return;
            }

            _hubConnection.Remove(eventName);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Не удалось отписаться от события {EventName}", eventName);
        }
    }

    private async Task StartConnectionWithRetryAsync()
    {
        await _reconnectionPolicy.ExecuteAsync(async () =>
        {
            if (_isDisposing || _isDisposed) return;

            if (_hubConnection == null)
            {
                throw new InvalidOperationException("Соединение с SignalR не инициализировано");
            }

            if (_hubConnection.State == HubConnectionState.Connected)
            {
                logger.LogInformation("SignalR уже подключен");
                return;
            }

            await _hubConnection.StartAsync();
            logger.LogInformation("SignalR соединение установлено успешно");
        });
    }

    public async Task InitializeAsync()
    {
        if (_isDisposing || _isDisposed)
        {
            logger.LogWarning("Попытка инициализации SignalR во время утилизации");
            return;
        }

        try
        {
            await _connectionLock.WaitAsync();
        }
        catch (ObjectDisposedException)
        {
            logger.LogWarning("Не удалось получить блокировку - семафор утилизирован");
            return;
        }

        try
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                logger.LogInformation("SignalR соединение уже установлено");
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

            var userId = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roles = authState.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            logger.LogInformation("Инициализация SignalR для пользователя: {UserId}, роли: {Roles}",
                userId, string.Join(", ", roles));

            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl, options => { 
                    options.AccessTokenProvider = () => Task.FromResult(token)!;
                    options.CloseTimeout = TimeSpan.FromMinutes(60);
                    options.SkipNegotiation = false;
                    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets | 
                                         Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents;
                })
                .WithAutomaticReconnect([
                    TimeSpan.FromSeconds(0), 
                    TimeSpan.FromSeconds(2), 
                    TimeSpan.FromSeconds(10), 
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(2), 
                    TimeSpan.FromMinutes(5),
                    TimeSpan.FromMinutes(10),
                    TimeSpan.FromMinutes(15),
                    TimeSpan.FromMinutes(30)
                ])
                .WithKeepAliveInterval(TimeSpan.FromMinutes(1))
                .WithStatefulReconnect()
                .Build();

            _hubConnection.Closed += async (error) =>
            {
                if (_isDisposing || _isDisposed) return;

                if (error != null)
                {
                    logger.LogWarning("Соединение с SignalR закрыто с ошибкой: {Error}", error.Message);
                    if (error.Message.Contains("Unauthorized") || error.Message.Contains("401"))
                    {
                        await authStateProvider.MarkUserAsLoggedOut();
                        return;
                    }
                    
                    if (error.Message.Contains("runtime.lastError") || 
                        error.Message.Contains("message channel closed"))
                    {
                        logger.LogWarning("Обнаружена ошибка channel closed/runtime.lastError, пересоздание соединения");
                        
                        await Task.Delay(1000);
                    }
                }

                logger.LogInformation("Попытка переподключения к SignalR через Polly...");
                try
                {
                    await StartConnectionWithRetryAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Не удалось подключиться к SignalR после всех попыток");
                }
            };

            await StartConnectionWithRetryAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при инициализации SignalR соединения");
            if (ex.Message.Contains("Unauthorized") || ex.Message.Contains("401"))
            {
                await authStateProvider.MarkUserAsLoggedOut();
            }
        }
        finally
        {
            try
            {
                if (!_isDisposed)
                {
                    _connectionLock.Release();
                }
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
        }
    }

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

    #endregion

    #region User

    public void SubscribeToUserUpdates(Action<UserDto> handler)
    {
        Subscribe(SignalREvents.UserUpdated, handler);
    }
    
    public void UnsubscribeFromUserUpdates()
    {
        Unsubscribe(SignalREvents.UserUpdated);
    }

    public void SubscribeToUserDeletions(Action<Guid> handler)
    {
        Subscribe(SignalREvents.UserDeleted, handler);
    }

    public void UnsubscribeFromUserDeletions()
    {
        Unsubscribe(SignalREvents.UserDeleted);
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

    public async Task DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposing = true;

        try
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при утилизации SignalR соединения");
        }
        finally
        {
            try
            {
                _connectionLock.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при утилизации семафора");
            }

            _isDisposed = true;
        }
    }
}