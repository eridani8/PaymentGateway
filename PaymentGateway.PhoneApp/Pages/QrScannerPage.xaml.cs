using ZXing.Net.Maui;

namespace PaymentGateway.PhoneApp.Pages;

public partial class QrScannerPage : ContentPage
{
    public event EventHandler<string>? OnQrCodeScanned;
    private bool _isProcessing;

    public QrScannerPage()
    {
        InitializeComponent();
    }

    private async void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs? e)
    {
        try
        {
            if (_isProcessing) return;
            _isProcessing = true;

            if (e?.Results is not { Length: > 0 }) return;
            
            var result = e.Results[0];
            if (string.IsNullOrEmpty(result?.Value)) return;

            OnQrCodeScanned?.Invoke(this, result.Value);
        }
        catch
        {
            await DisplayAlert("Ошибка", "Произошла ошибка при сканировании QR-кода", "OK");
        }
        finally
        {
            _isProcessing = false;
        }
    }
} 