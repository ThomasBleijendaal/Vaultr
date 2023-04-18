using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using RapidCMS.Core.Extensions;
using Vaultr.Client.Core.Abstractions;
using Vaultr.Client.Core.Models;
using Vaultr.Client.Data.Repositories;

namespace Vaultr.Client.Components.Authentication;

public partial class LoginScreen
{
    public IReadOnlyList<ConfigurationState> Configurations { get; set; } = null!;

    public ConfigurationState NewConfig { get; set; } = new ConfigurationState();

    [Inject]
    public NavigationManager Navigation { get; set; } = null!;

    [Inject]
    public IConfigurationStateProvider ConfigurationStateProvider { get; set; } = null!;

    [Inject]
    public ISecretClientsProvider SecretClientsProvider { get; set; } = null!;

    [Inject]
    public IJSRuntime JsRuntime { get; set; } = null!;

    protected override void OnInitialized()
    {
        Configurations = ConfigurationStateProvider.GetConfigurations();

        NewConfig = ConfigurationStateProvider.GetCurrentState() ?? NewConfig;
    }

    private void HandleSubmit(EditContext context)
    {
        if (NewConfig.IsValid())
        {
            if (!Configurations.Contains(NewConfig))
            {
                ConfigurationStateProvider.AddState(NewConfig);
            }
            else
            {
                ConfigurationStateProvider.UpdateState(NewConfig);
            }
        }

        NewConfig = new ConfigurationState();

        StateHasChanged();
    }

    private void Edit(ConfigurationState config)
    {
        NewConfig = config;

        StateHasChanged();
    }

    private void Duplicate(ConfigurationState config)
    {
        NewConfig = new ConfigurationState
        {
            Name = config.Name,
            TenantId = config.TenantId,
            KeyVaults = config.KeyVaults.ToList(x => new ConfigurationState.KeyVaultConfiguration
            {
                Name = x.Name
            })
        };

        StateHasChanged();
    }

    private async Task RemoveAsync(ConfigurationState config)
    {
        if (await JsRuntime.InvokeAsync<bool>("window.confirm", $"Remove {config.Name}?"))
        {
            ConfigurationStateProvider.RemoveState(config);

            StateHasChanged();
        }
    }

    private void Login(ConfigurationState config)
    {
        if (config.IsValid())
        {
            ConfigurationStateProvider.SetCurrentState(config);
            SecretClientsProvider.Build();

            Navigation.NavigateTo("/", true);
        }
        else
        {
            ConfigurationStateProvider.RemoveState(config);
        }
    }
}
