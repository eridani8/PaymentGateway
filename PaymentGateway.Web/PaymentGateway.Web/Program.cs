using System.Net.Http.Headers;
using Blazored.LocalStorage;
using PaymentGateway.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using PaymentGateway.Web.Service;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
try
{
    builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(nameof(ApiSettings)));
    
    builder.RootComponents.Add<App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");

    builder.Services.AddBlazoredLocalStorage();

    builder.Services.AddAuthorizationCore();
    builder.Services.AddScoped<CustomAuthStateProvider>();
    builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
    builder.Services.AddScoped<AuthMessageHandler>();

    builder.Services.AddHttpClient("API", (serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
            
            client.BaseAddress = new Uri(settings.BaseAddress);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddHttpMessageHandler<AuthMessageHandler>();

    builder.Services.AddBlazorise(o => o.Immediate = true);
    builder.Services
        .AddBootstrap5Providers()
        .AddFontAwesomeIcons();


    var app = builder.Build();

    await app.RunAsync();
}
catch (Exception e)
{
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
    logger.LogError(e, "SPA не может запуститься");
}