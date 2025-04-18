using System.Net.Http.Headers;
using Blazored.LocalStorage;
using PaymentGateway.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FluentValidation;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using MudBlazor;
using MudBlazor.Services;
using PaymentGateway.Shared.DTOs.User;
using PaymentGateway.Web.Interfaces;
using PaymentGateway.Web.Services;
using Microsoft.Extensions.Localization;
using System.Globalization;
using PaymentGateway.Shared.DTOs.Requisite;
using PaymentGateway.Shared.Validations.Validators.Requisite;
using PaymentGateway.Shared.Validations.Validators.User;
using System.Text.Json;
using System.Text.Json.Serialization;
using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Shared.Validations.Validators.Payment;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(nameof(ApiSettings)));

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.ReferenceHandler = ReferenceHandler.Preserve;
    options.PropertyNameCaseInsensitive = true;
    options.MaxDepth = 128;
});

builder.Services.AddSingleton(new JsonSerializerOptions
{
    ReferenceHandler = ReferenceHandler.Preserve,
    PropertyNameCaseInsensitive = true,
    MaxDepth = 128
});

builder.Services.AddScoped<IValidator<LoginDto>, LoginModelValidator>();
builder.Services.AddScoped<IValidator<ChangePasswordDto>, ChangePasswordValidator>();

builder.Services.AddScoped<IValidator<RequisiteCreateDto>, RequisiteCreateDtoValidator>();
builder.Services.AddScoped<IValidator<PaymentCreateDto>, PaymentCreateDtoValidator>();
    
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<AuthMessageHandler>();
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IRequisiteService, RequisiteService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.AddHttpClient("API", (serviceProvider, client) =>
    {
        var settings = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;

        client.BaseAddress = new Uri(settings.BaseAddress);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    })
    .AddHttpMessageHandler<AuthMessageHandler>();

builder.Services.AddMudServices(c =>
{
    c.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
    c.SnackbarConfiguration.ShowCloseIcon = true;
    c.SnackbarConfiguration.VisibleStateDuration = 5000;
    c.SnackbarConfiguration.HideTransitionDuration = 500;
    c.SnackbarConfiguration.ShowTransitionDuration = 500;
    c.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddSingleton<IStringLocalizer<MudResources>, StringLocalizer<MudResources>>();

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru-RU");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("ru-RU");

builder.Services.AddTransient<MudLocalizer, ResXMudLocalizer>();
builder.Services.AddTransient<SignalRService>();

var app = builder.Build();

await app.RunAsync();