using PaymentGateway.PhoneApp.ViewModels;

namespace PaymentGateway.PhoneApp.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}