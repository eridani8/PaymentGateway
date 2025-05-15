using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PaymentGateway.PhoneApp.Services;
using System.Text;
using CommunityToolkit.Maui.Storage;
using PaymentGateway.PhoneApp.Interfaces;
using PaymentGateway.PhoneApp.Types;

namespace PaymentGateway.PhoneApp.ViewModels;

public partial class LogsViewModel(
    InMemoryLogSink sink,
    ILogger<LogsViewModel> logger,
    LiteContext context,
    IAlertService alertService)
    : BaseViewModel
{
    [ObservableProperty] private InMemoryLogSink _sink = sink;
    private readonly ILogger _logger = logger;

    [RelayCommand]
    private async Task ClearLogs()
    {
        var confirmed = await alertService.ShowConfirmationAsync(
            "Очистить логи",
            "Вы уверены, что хотите очистить логи?",
            "Да", "Отмена");

        if (confirmed)
        {
            Sink.Logs.Clear();
            context.Logs.DeleteAll();
        }
    }

    [RelayCommand]
    private async Task ExportLogs()
    {
        try
        {
            if (Sink.Logs.Count == 0)
            {
                await Shell.Current.DisplayAlert("Экспорт логов", "Нет логов для экспорта", "OK");
                return;
            }

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var fileName = $"logs_{timestamp}.log";
            
            var sb = new StringBuilder();
            foreach (var log in Sink.Logs)
            {
                sb.AppendLine(log.AsString);
                sb.AppendLine();
            }

            var content = sb.ToString();

            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            {
                var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
                await File.WriteAllTextAsync(filePath, content);

                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Экспорт логов",
                    File = new ShareFile(filePath)
                });
            }
            else
            {
                var fileSaverResult = await FileSaver.SaveAsync(fileName, new MemoryStream(Encoding.UTF8.GetBytes(content)));
                
                if (fileSaverResult.IsSuccessful)
                {
                    await Shell.Current.DisplayAlert("Экспорт логов", "Логи успешно экспортированы", "OK");
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Ошибка при экспорте логов");
            await Shell.Current.DisplayAlert("Ошибка", "Не удалось экспортировать логи", "OK");
        }
    }

    [RelayCommand]
    private void TestLogs()
    {
        _logger.LogInformation("Test Information");
        _logger.LogError("Test Error");
        _logger.LogWarning("Test Warning");
        _logger.LogDebug("Test Debug");
        _logger.LogCritical("Test Critical");
    }
} 