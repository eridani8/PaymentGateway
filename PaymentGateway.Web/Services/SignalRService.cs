using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Shared.Constants;
using System.Security.Claims;

namespace PaymentGateway.Web.Services;

public class SignalRService(
    IOptions<ApiSettings> settings, 
    ILogger<SignalRService> logger,
    CustomAuthStateProvider authStateProvider)
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl = $"{settings.Value.BaseAddress}/notificationHub";

    private void EnsureConnectionInitialized()
    {
        if (_hubConnection == null)
        {
            throw new InvalidOperationException("SignalR соединение не инициализировано");
        }
    }

    private void Subscribe<T>(string eventName, Action<T> handler)
    {
        EnsureConnectionInitialized();
        _hubConnection!.On(eventName, handler);
    }

    private void Unsubscribe(string eventName)
    {
        EnsureConnectionInitialized();
        _hubConnection!.Remove(eventName);
    }

    public async Task InitializeAsync()
    {
        try
        {
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

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(token)!;
                })
                .WithAutomaticReconnect([TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)])
                .Build();

            _hubConnection.Closed += async (error) =>
            {
                if (error != null)
                {
                    logger.LogWarning("Соединение с SignalR закрыто с ошибкой: {Error}", error.Message);
                    if (error.Message.Contains("Unauthorized") || error.Message.Contains("401"))
                    {
                        await authStateProvider.MarkUserAsLoggedOut();
                        return;
                    }
                }
                
                logger.LogInformation("Попытка переподключения к SignalR...");
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await _hubConnection.StartAsync();
            };

            await _hubConnection.StartAsync();
            logger.LogInformation("SignalR соединение установлено");
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

    public void SubscribeToRequisiteUpdates(Action<RequisiteDto> handler)
    {
        Subscribe(SignalREvents.RequisiteUpdated, handler);
    }

    public void SubscribeToRequisiteDeletions(Action<Guid> handler)
    {
        Subscribe(SignalREvents.RequisiteDeleted, handler);
    }

    public void SubscribeToUserUpdates(Action<UserDto> handler)
    {
        Subscribe(SignalREvents.UserUpdated, handler);
    }

    public void SubscribeToUserDeletions(Action<Guid> handler)
    {
        Subscribe(SignalREvents.UserDeleted, handler);
    }

    public void UnsubscribeFromUserUpdates()
    {
        Unsubscribe(SignalREvents.UserUpdated);
    }

    public void UnsubscribeFromUserDeletions()
    {
        Unsubscribe(SignalREvents.UserDeleted);
    }

    public void UnsubscribeFromRequisiteUpdates()
    {
        Unsubscribe(SignalREvents.RequisiteUpdated);
    }

    public void UnsubscribeFromRequisiteDeletions()
    {
        Unsubscribe(SignalREvents.RequisiteDeleted);
    }

    public async Task DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
} 