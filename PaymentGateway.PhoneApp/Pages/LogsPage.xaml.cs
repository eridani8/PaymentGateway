using System.Collections.Specialized;
using PaymentGateway.PhoneApp.Services.Logs;
using PaymentGateway.PhoneApp.ViewModels;

namespace PaymentGateway.PhoneApp.Pages;

public partial class LogsPage : ContentPage
{
    private readonly LogsViewModel _viewModel;
    
    public LogsPage(LogsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        
        viewModel.Sink.Logs.CollectionChanged += Logs_CollectionChanged;
        viewModel.Sink.LogAdded += Sink_LogAdded;
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        ScrollToBottom();
    }
    
    private void Logs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            ScrollToBottom();
        }
    }
    
    private void Sink_LogAdded(object? sender, LogEntry e)
    {
        ScrollToBottom();
    }
    
    private void ScrollToBottom()
    {
        Dispatcher.Dispatch(async void () =>
        {
            try
            {
                await Task.Delay(50);
                
                var count = _viewModel.Sink.Logs.Count;
                if (count > 0)
                {
                    LogsCollectionView.ScrollTo(count - 1, animate: false);
                }
            }
            catch
            {
                // ignore
            }
        });
    }
    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        _viewModel.Sink.Logs.CollectionChanged -= Logs_CollectionChanged;
        _viewModel.Sink.LogAdded -= Sink_LogAdded;
    }
} 