using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
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
    public SecretClientsProvider SecretClientsProvider { get; set; } = null!;

    protected override void OnInitialized()
    {
        Configurations = ConfigurationStateProvider.GetConfigurations();

        NewConfig = ConfigurationStateProvider.GetCurrentState() ?? NewConfig;
    }

    private void HandleSubmit(EditContext context)
    {
        if (NewConfig.IsValid())
        {
            ConfigurationStateProvider.AddState(NewConfig);
        }

        NewConfig = new ConfigurationState();

        StateHasChanged();
    }

    private void Remove(ConfigurationState config)
    {
        ConfigurationStateProvider.RemoveState(config);
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
