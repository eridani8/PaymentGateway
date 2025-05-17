using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace PaymentGateway.Shared.Services;

public abstract class BaseSignalRService(string hubUrl, ILogger logger) : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private Timer? _pingTimer;
    private const int pingInterval = 10000;
    private bool _isDisposed;

    protected virtual async Task InitializeConnectionAsync(string? token = null)
    {
        if (_isDisposed) return;

        try
        {
            if (_hubConnection is { State: HubConnectionState.Connected })
            {
                logger.LogDebug("SignalR соединение уже установлено");
                return;
            }

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
                .WithUrl(hubUrl, options =>
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        options.AccessTokenProvider = () => Task.FromResult(token);
                    }
                    
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
                    await HandleConnectionError(error);
                }

                await StartConnectionWithRetryAsync();
            };

            await StartConnectionWithRetryAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при инициализации SignalR соединения");
            await HandleInitializationError(ex);
        }
    }

    protected virtual async Task HandleConnectionError(Exception error)
    {
        await Task.CompletedTask;
    }

    protected virtual async Task HandleInitializationError(Exception error)
    {
        await Task.CompletedTask;
    }

    protected virtual async Task StartConnectionWithRetryAsync()
    {
        if (_isDisposed) return;

        try
        {
            if (_hubConnection == null)
            {
                throw new InvalidOperationException("Соединение с SignalR не инициализировано");
            }

            if (_hubConnection.State == HubConnectionState.Connected)
            {
                return;
            }

            await _hubConnection.StartAsync();

            if (_pingTimer != null)
            {
                await _pingTimer.DisposeAsync();
            }

            _pingTimer = new Timer(async void (_) =>
            {
                try
                {
                    await SendPingAsync();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Ошибка при отправке ping");
                }
            }, null, pingInterval, pingInterval);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при подключении SignalR");
            throw;
        }
    }

    protected virtual async Task SendPingAsync()
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

    protected void Subscribe<T>(string eventName, Action<T> handler)
    {
        try
        {
            if (_hubConnection == null || _hubConnection.State != HubConnectionState.Connected)
            {
                logger.LogDebug("Подписка на событие {EventName} не выполнена - соединение не активно", eventName);
                return;
            }

            _hubConnection.Remove(eventName);

            _hubConnection.On<T>(eventName, data =>
            {
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

    protected void Unsubscribe(string eventName)
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

    public virtual async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;

        try
        {
            _isDisposed = true;

            if (_pingTimer != null)
            {
                await _pingTimer.DisposeAsync();
                _pingTimer = null;
            }

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
    }
} 