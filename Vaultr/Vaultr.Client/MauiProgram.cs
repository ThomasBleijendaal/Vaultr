using System.Collections.Generic;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Vaultr.Client.Components.Authentication;

namespace Vaultr.Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .RegisterBlazorMauiWebView()
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddBlazorWebView();

            builder.Services.AddAuthorizationCore();

            builder.Services.AddRapidCMSWebAssembly(config =>
            {
                config.SetCustomLoginScreen(typeof(LoginScreen));


            });


            builder.Services.AddMsalAuthentication(options =>
            {
                // TODO: ask these credentials
                options.ProviderOptions.Authentication.Authority = "https://login.microsoftonline.com/324bfecd-c8d2-4233-887b-c1be7fa11256";
                options.ProviderOptions.Authentication.ClientId = "c7257161-fb36-4f26-9dc6-fb846b3c4958";
                options.ProviderOptions.Authentication.ValidateAuthority = true;
                options.ProviderOptions.DefaultAccessTokenScopes = new List<string> 
                {
                    "https://vault.azure.net/user_impersonation"
                };



                // builder.Configuration.Bind("AzureAd", options.ProviderOptions);
            });

            return builder.Build();
        }
    }
}
