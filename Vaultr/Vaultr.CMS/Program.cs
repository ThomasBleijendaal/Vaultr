using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RapidCMS.Core.Abstractions.Plugins;
using Vaultr.CMS;
using Vaultr.CMS.Authentication;
using Vaultr.CMS.Plugins;
using Vaultr.CMS.Repositories;
using Vaultr.Core.Abstractions;
using Vaultr.Core.Handlers;
using Vaultr.Core.Providers;

// scoped to singleton
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddBlazoredLocalStorage();
var serviceProvider = builder.Build().Services;
var localStorage = serviceProvider.GetRequiredService<ISyncLocalStorageService>();



builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddAuthorizationCore();
builder.Services.AddSingleton(localStorage);

builder.Services.AddSingleton<IAuthorizationHandler, AllowAllAuthorizationHandler>();
builder.Services.AddSingleton<AuthenticationStateProvider, ClientSpecifiedAuthenticationStateProvider>();
builder.Services.AddSingleton<IConfigurationStateProvider, ConfigurationStateProvider>();

builder.Services.AddTransient<IPlugin, KeyVaultCollectionPlugin>();
builder.Services.AddTransient<KeyVaultRespository>();

builder.Services.AddRapidCMSWebAssembly(config =>
{
    config.SetCustomLoginScreen(typeof(LoginScreen));
    // config.SetCustomLoginStatus(typeof(LoginScreen));

    config.AddPlugin<KeyVaultCollectionPlugin>();
});

// TODO
//builder.Services.AddMsalAuthentication(options =>
//{
//    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
//});

await builder.Build().RunAsync();
