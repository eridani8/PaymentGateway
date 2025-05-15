using PaymentGateway.PhoneApp.ViewModels;

namespace PaymentGateway.PhoneApp.Pages;

public partial class DeviceIdPage : ContentPage
{
    public DeviceIdPage(DeviceIdViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}