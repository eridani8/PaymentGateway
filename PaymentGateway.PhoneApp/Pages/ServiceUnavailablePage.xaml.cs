using PaymentGateway.PhoneApp.ViewModels;

namespace PaymentGateway.PhoneApp.Pages;

public partial class ServiceUnavailablePage : ContentPage
{
    public ServiceUnavailablePage(ServiceUnavailableViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
    protected override bool OnBackButtonPressed()
    {
        return true;
    }
} 