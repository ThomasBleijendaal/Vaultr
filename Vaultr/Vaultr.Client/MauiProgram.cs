using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using RapidCMS.Core.Abstractions.Plugins;
using Vaultr.Client.Components.Authentication;
using Vaultr.Client.Core;
using Vaultr.Client.Core.Abstractions;
using Vaultr.Client.Core.Handlers;
using Vaultr.Client.Core.Providers;
using Vaultr.Client.Data.Plugins;
using Vaultr.Client.Data.Repositories;

namespace Vaultr.Client;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        Environment.CurrentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        builder.Services.AddMemoryCache();

        builder.Services.AddAuthorizationCore();

        builder.Services.AddSingleton<ICredentialProvider, CredentialProvider>();

        builder.Services.AddScoped<ISecretClientsProvider, SecretClientsProvider>();
        builder.Services.AddScoped<ISecretsProvider, SecretsProvider>();
        builder.Services.AddScoped<IDangerModeProvider, DangerModeProvider>();

        builder.Services.AddSingleton<IAuthorizationHandler, AllowAllAuthorizationHandler>();
        builder.Services.AddSingleton<AuthenticationStateProvider, ClientSpecifiedAuthenticationStateProvider>();
        builder.Services.AddSingleton<IConfigurationStateProvider, ConfigurationStateProvider>();

        builder.Services.AddSingleton<IPlugin, KeyVaultCollectionPlugin>();
        builder.Services.AddScoped<KeyVaultRespository>();
        builder.Services.AddSingleton<NotificationService>();

        builder.Services.AddRapidCMSWebAssembly(config =>
        {
            config.SetSiteName("Vaultr");

            config.SetCustomLoginScreen(typeof(LoginScreen));
            config.SetCustomLoginStatus(typeof(LoginStatus));

            config.Dashboard.AddSection("vaultr::secrets", edit: true);

            config.AddPlugin<KeyVaultCollectionPlugin>();
        });

        return builder.Build();
    }
}
