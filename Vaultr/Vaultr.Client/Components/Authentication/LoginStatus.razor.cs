using Microsoft.AspNetCore.Components;
using Vaultr.Client.Core.Abstractions;
using Vaultr.Client.Data.Repositories;

namespace Vaultr.Client.Components.Authentication;

public partial class LoginStatus
{
    [Inject]
    public NavigationManager Navigation { get; set; } = null!;

    [Inject]
    public IConfigurationStateProvider ConfigurationStateProvider { get; set; } = null!;

    [Inject]
    public SecretClientsProvider SecretClientsProvider { get; set; } = null!;

    private void Logout()
    {
        ConfigurationStateProvider.SetCurrentState(default);
        SecretClientsProvider.Build();
        Navigation.NavigateTo("/", true);
    }
}
