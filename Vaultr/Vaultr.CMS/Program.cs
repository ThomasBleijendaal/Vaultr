using System.Net.Http.Headers;
using Azure.Core.Pipeline;
using Azure.Security.KeyVault.Secrets;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RapidCMS.Core.Abstractions.Plugins;
using Vaultr.CMS;
using Vaultr.CMS.Authentication;
using Vaultr.CMS.Credentials;
using Vaultr.CMS.Plugins;
using Vaultr.CMS.Repositories;
using Vaultr.Core.Abstractions;
using Vaultr.Core.Handlers;
using Vaultr.Core.Providers;

// scoped to singleton
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredSessionStorage();
var serviceProvider = builder.Build().Services;
var localStorage = serviceProvider.GetRequiredService<ISyncLocalStorageService>();
var sessionStorage = serviceProvider.GetRequiredService<ISyncSessionStorageService>();



builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddAuthorizationCore();
builder.Services.AddSingleton(localStorage);
builder.Services.AddSingleton(sessionStorage);

builder.Services.AddSingleton<IAuthorizationHandler, AllowAllAuthorizationHandler>();
//builder.Services.AddSingleton<AuthenticationStateProvider, ClientSpecifiedAuthenticationStateProvider>();
builder.Services.AddSingleton<IConfigurationStateProvider, ConfigurationStateProvider>();

builder.Services.AddSingleton<IPlugin, KeyVaultCollectionPlugin>();
builder.Services.AddSingleton<KeyVaultRespository>();
builder.Services.AddSingleton((sp) =>
{
    var clients = new SecretClients();

    var stateProvider = sp.GetRequiredService<IConfigurationStateProvider>();

    var currentState = stateProvider.GetCurrentState();
    if (currentState == null)
    {
        return clients;
    }

    var tokenCredential = sp.GetRequiredService<DelegatedTokenCredential>();
    
    foreach (var keyVault in currentState.KeyVaults)
    {
        var httpClient = new HttpClient(new TransformUrlHttpMessageHandler(keyVault.Name));
        
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenCredential.GetToken());
        httpClient.BaseAddress = new Uri($"https://vaultr.azurewebsites.net/{keyVault.Name}/");

        var options = new SecretClientOptions
        {
            Transport = new HttpClientTransport(httpClient)
        };

        clients.Add(keyVault.Name, new SecretClient(
            new Uri("https://vaultr.azurewebsites.net"),
            tokenCredential,
            options));
    }

    return clients;
});
builder.Services.AddSingleton<DelegatedTokenCredential>();



builder.Services.AddRapidCMSWebAssembly(config =>
{
    config.SetCustomLoginScreen(typeof(LoginScreen));
    // config.SetCustomLoginStatus(typeof(LoginScreen));

    config.AddPlugin<KeyVaultCollectionPlugin>();
});

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions);
});

await builder.Build().RunAsync();
