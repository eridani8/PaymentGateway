using PaymentGateway.PhoneApp.ViewModels;
using ZXing.Net.Maui;

namespace PaymentGateway.PhoneApp.Pages;

public partial class AuthorizationPage : ContentPage
{
    public AuthorizationPage(AuthorizationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
} 