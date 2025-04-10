using System.Net.Http.Headers;
using Blazored.LocalStorage;
using PaymentGateway.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using PaymentGateway.Web.Service;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
try
{
    builder.RootComponents.Add<App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");

    builder.Services.AddHttpClient("API", client =>
    {
        client.BaseAddress = new Uri("http://109.73.192.135");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    });
    builder.Services.AddAuthorizationCore();
    builder.Services.AddScoped<CustomAuthStateProvider>();

    builder.Services.AddBlazorise(o => o.Immediate = true);
    builder.Services
        .AddBootstrap5Providers()
        .AddFontAwesomeIcons();
    builder.Services.AddBlazoredLocalStorage();

    var app = builder.Build();

    await app.RunAsync();
}
catch (Exception e)
{
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
    logger.LogError(e, "SPA не может запуститься");
}