using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR.Client;
using PaymentGateway.Shared;
using PaymentGateway.Shared.Constants;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.Transaction;
using PaymentGateway.Shared.DTOs.User;
using Polly;
using Polly.Retry;
using PaymentGateway.Shared.DTOs.Chat;
using PaymentGateway.Shared.DTOs.Device;
using PaymentGateway.Shared.Services;
using PaymentGateway.Shared.Types;

namespace PaymentGateway.Web.Services;

public class NotificationService(
    IOptions<ApiSettings> settings,
    IServiceProvider serviceProvider,
    ILogger<NotificationService> logger) : BaseSignalRService(settings, logger)
{
    protected override Func<Task<string?>> AccessTokenProvider => async () => 
    {
        using var scope = serviceProvider.CreateScope();
        var authStateProvider = scope.ServiceProvider.GetRequiredService<CustomAuthStateProvider>();
        var currentToken = await authStateProvider.GetTokenFromLocalStorageAsync();
        
        if (string.IsNullOrEmpty(currentToken))
        {
            logger.LogWarning("Токен отсутствует при запросе AccessTokenProvider");
            await authStateProvider.MarkUserAsLoggedOut();
            throw new InvalidOperationException("Токен не найден");
        }
        
        return currentToken;
    };

    public override async Task<bool> InitializeAsync()
    {
        if (IsDisposed) return false;
        
        using var scope = serviceProvider.CreateScope();
        var authStateProvider = scope.ServiceProvider.GetRequiredService<CustomAuthStateProvider>();
        
        try
        {
            var token = await authStateProvider.GetTokenFromLocalStorageAsync();

            if (string.IsNullOrEmpty(token))
            {
                logger.LogDebug("Токен не найден в localStorage");
                return false;
            }

            var authState = await authStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity is not { IsAuthenticated: true })
            {
                logger.LogWarning("Пользователь не аутентифицирован");
                return false;
            }

            var username = authState.User.FindFirst(ClaimTypes.Name)?.Value;
            logger.LogDebug("Инициализация соединения для пользователя: {Username}", username);

            return await base.InitializeAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при инициализации соединения");
            if (ex.Message.Contains("Unauthorized") || ex.Message.Contains("401"))
            {
                await authStateProvider.MarkUserAsLoggedOut();
            }

            return false;
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
        Unsubscribe(SignalREvents.Web.OnDeviceConnected);
        Unsubscribe(SignalREvents.Web.OnDeviceDisconnected);
        
        return base.DisposeAsync();
    }

    #region Device

    public void SubscribeToDeviceConnected(Action<DeviceDto> handler)
    {
        Subscribe(SignalREvents.Web.OnDeviceConnected, handler);
    }

    public void UnsubscribeFromDeviceConnected()
    {
        Unsubscribe(SignalREvents.Web.OnDeviceConnected);
    }

    public void SubscribeToDeviceDisconnected(Action<DeviceDto> handler)
    {
        Subscribe(SignalREvents.Web.OnDeviceDisconnected, handler);
    }

    public void UnsubscribeFromDeviceDisconnected()
    {
        Unsubscribe(SignalREvents.Web.OnDeviceDisconnected);
    }

    #endregion

    #region User

    public void SubscribeToUserUpdates(Action<UserDto> handler)
    {
        Subscribe(SignalREvents.Web.UserUpdated, handler);
    }
    
    public void UnsubscribeFromUserUpdates()
    {
        Unsubscribe(SignalREvents.Web.UserUpdated);
    }

    public void SubscribeToUserDeletions(Action<Guid> handler)
    {
        Subscribe(SignalREvents.Web.UserDeleted, handler);
    }
    
    public void UnsubscribeFromUserDeletions()
    {
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