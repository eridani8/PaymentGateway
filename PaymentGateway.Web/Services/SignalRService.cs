using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using PaymentGateway.Shared.DTOs.Requisite;
using Blazored.LocalStorage;
using System.Security.Claims;

namespace PaymentGateway.Web.Services;

public class SignalRService(
    IOptions<ApiSettings> settings, 
    ILogger<SignalRService> logger,
    CustomAuthStateProvider authStateProvider)
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl = $"{settings.Value.BaseAddress}/notificationHub";

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

    public void SubscribeToUpdates(string methodName, Action handler)
    {
        if (_hubConnection == null)
        {
            logger.LogWarning("Попытка подписаться на обновления до инициализации соединения");
            return;
        }

        _hubConnection.On(methodName, handler);
    }

    public void SubscribeToRequisiteUpdates(Action<RequisiteDto> handler)
    {
        if (_hubConnection == null)
        {
            logger.LogWarning("Попытка подписаться на обновления реквизитов до инициализации соединения");
            return;
        }

        _hubConnection.On("RequisiteUpdated", handler);
    }

    public void UnsubscribeFromUpdates(string methodName)
    {
        _hubConnection?.Remove(methodName);
    }

    public async Task DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
} 