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
using PaymentGateway.Shared.Services;
using PaymentGateway.Shared.Types;

namespace PaymentGateway.Web.Services;

public class NotificationService(
    IOptions<WebSocketSettings> settings,
    IServiceProvider serviceProvider,
    ILogger<NotificationService> logger) : BaseSignalRService(settings, logger)
{
    public override async Task InitializeAsync()
    {
        if (IsDisposed) return;
        
        using var scope = serviceProvider.CreateScope();
        var authStateProvider = scope.ServiceProvider.GetRequiredService<CustomAuthStateProvider>();
        
        try
        {
            if (HubConnection is { State: HubConnectionState.Connected })
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

            if (HubConnection != null)
            {
                await HubConnection.DisposeAsync();
            }

            HubConnection = new HubConnectionBuilder()
                .WithUrl(HubUrl, options =>
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

    public override ValueTask DisposeAsync()
    {
        if (IsDisposed) return ValueTask.CompletedTask;
        
        Unsubscribe(SignalREvents.Web.UserUpdated);
        Unsubscribe(SignalREvents.Web.UserDeleted);
        Unsubscribe(SignalREvents.Web.RequisiteUpdated);
        Unsubscribe(SignalREvents.Web.RequisiteDeleted);
        Unsubscribe(SignalREvents.Web.PaymentUpdated);
        Unsubscribe(SignalREvents.Web.PaymentDeleted);
        Unsubscribe(SignalREvents.Web.ChatMessageReceived);
        Unsubscribe(SignalREvents.Web.UserConnected);
        Unsubscribe(SignalREvents.Web.UserDisconnected);
        Unsubscribe(SignalREvents.Web.ChangeRequisiteAssignmentAlgorithm);
        
        return base.DisposeAsync();
    }

    #region User

    public void SubscribeToUserUpdates(Action<UserDto> handler)
    {
        logger.LogInformation("Подписка на обновления пользователей");
        Subscribe(SignalREvents.Web.UserUpdated, handler);
    }
    
    public void UnsubscribeFromUserUpdates()
    {
        logger.LogInformation("Отписка от обновлений пользователей");
        Unsubscribe(SignalREvents.Web.UserUpdated);
    }

    public void SubscribeToUserDeletions(Action<Guid> handler)
    {
        logger.LogInformation("Подписка на удаление пользователей");
        Subscribe(SignalREvents.Web.UserDeleted, handler);
    }
    
    public void UnsubscribeFromUserDeletions()
    {
        logger.LogInformation("Отписка от удаления пользователей");
        Unsubscribe(SignalREvents.Web.UserDeleted);
    }

    #endregion
    
    #region Transaction

    public void SubscribeToTransactionUpdates(Action<TransactionDto> handler)
    {
        Subscribe(SignalREvents.Web.TransactionUpdated, handler);
    }

    public void UnsubscribeFromTransactionUpdates()
    {
        Unsubscribe(SignalREvents.Web.TransactionUpdated);
    }

    #endregion
    
    #region Payment

    public void SubscribeToPaymentUpdates(Action<PaymentDto> handler)
    {
        Subscribe(SignalREvents.Web.PaymentUpdated, handler);
    }

    public void UnsubscribeFromPaymentUpdates()
    {
        Unsubscribe(SignalREvents.Web.PaymentUpdated);
    }

    public void SubscribeToPaymentDeletions(Action<Guid> handler)
    {
        Subscribe(SignalREvents.Web.PaymentDeleted, handler);
    }

    public void UnsubscribeFromPaymentDeletions()
    {
        Unsubscribe(SignalREvents.Web.PaymentDeleted);
    }

    #endregion
    
    #region Requisite

    public void SubscribeToRequisiteUpdates(Action<RequisiteDto> handler)
    {
        Subscribe(SignalREvents.Web.RequisiteUpdated, handler);
    }
    
    public void UnsubscribeFromRequisiteUpdates()
    {
        Unsubscribe(SignalREvents.Web.RequisiteUpdated);
    }

    public void SubscribeToRequisiteDeletions(Action<Guid> handler)
    {
        Subscribe(SignalREvents.Web.RequisiteDeleted, handler);
    }
    
    public void UnsubscribeFromRequisiteDeletions()
    {
        Unsubscribe(SignalREvents.Web.RequisiteDeleted);
    }

    public void SubscribeToChangeRequisiteAssignmentAlgorithm(Action<int> handler)
    {
        Subscribe(SignalREvents.Web.ChangeRequisiteAssignmentAlgorithm, handler);
    }

    public void UnsubscribeFromChangeRequisiteAssignmentAlgorithm()
    {
        Unsubscribe(SignalREvents.Web.ChangeRequisiteAssignmentAlgorithm);
    }

    #endregion

    #region Chat
    
    public async Task<List<UserState>> GetAllUsers()
    {
        try
        {
            return await HubConnection!.InvokeAsync<List<UserState>>(SignalREvents.Web.GetAllUsers);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка получения списка пользователей");
            return [];
        }
    }

    public void SubscribeToUserConnected(Action<UserState> handler)
    {
        Subscribe(SignalREvents.Web.UserConnected, handler);
    }

    public void UnsubscribeFromUserConnected()
    {
        Unsubscribe(SignalREvents.Web.UserConnected);
    }

    public void SubscribeToUserDisconnected(Action<UserState> handler)
    {
        Subscribe(SignalREvents.Web.UserDisconnected, handler);
    }

    public void UnsubscribeFromUserDisconnected()
    {
        Unsubscribe(SignalREvents.Web.UserDisconnected);
    }
    
    public void SubscribeToChatMessages(Action<ChatMessageDto> handler)
    {
        Subscribe(SignalREvents.Web.ChatMessageReceived, handler);
    }

    public void UnsubscribeFromChatMessages()
    {
        Unsubscribe(SignalREvents.Web.ChatMessageReceived);
    }
    
    public async Task SendChatMessage(string message)
    {
        try
        {
            await HubConnection!.InvokeAsync(SignalREvents.Web.SendChatMessage, message);
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
            return await HubConnection!.InvokeAsync<List<ChatMessageDto>>(SignalREvents.Web.GetChatMessages);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка загрузки истории чата");
            return [];
        }
    }

    #endregion
    
}