using ZXing.Net.Maui;

namespace PaymentGateway.PhoneApp.Pages;

public partial class QrScannerPage : ContentPage
{
    public event EventHandler<string>? OnQrCodeScanned;

    public QrScannerPage()
    {
        InitializeComponent();
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (e.Results.Length <= 0) return;
        var result = e.Results[0];
        OnQrCodeScanned?.Invoke(this, result.Value);
        Navigation.PopAsync();
    }
} 