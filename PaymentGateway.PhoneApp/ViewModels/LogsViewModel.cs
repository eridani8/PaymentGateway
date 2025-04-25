using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PaymentGateway.PhoneApp.Services.Logs;

namespace PaymentGateway.PhoneApp.ViewModels;

public partial class LogsViewModel : BaseViewModel
{
    [ObservableProperty] private InMemoryLogSink _sink;

    public LogsViewModel(InMemoryLogSink sink)
    {
        Title = "Логи";
        _sink = sink;
    }

    [RelayCommand]
    private void ClearLogs()
    {
        Sink.Logs.Clear();
    }
} 