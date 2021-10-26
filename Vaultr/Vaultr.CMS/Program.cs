using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Vaultr.CMS;
using Vaultr.CMS.Authentication;
using Vaultr.Core.Handlers;
using Vaultr.Core.Providers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddAuthorizationCore();

builder.Services.AddSingleton<IAuthorizationHandler, AllowAllAuthorizationHandler>();
builder.Services.AddSingleton<AuthenticationStateProvider, ClientSpecifiedAuthenticationStateProvider>();

builder.Services.AddRapidCMSWebAssembly(config =>
{


    config.SetCustomLoginScreen(typeof(LoginScreen));
    // config.SetCustomLoginStatus(typeof(LoginScreen));
});

// TODO
//builder.Services.AddMsalAuthentication(options =>
//{
//    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
//});

await builder.Build().RunAsync();
