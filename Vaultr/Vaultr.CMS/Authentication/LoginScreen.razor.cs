using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Vaultr.Core.Abstractions;
using Vaultr.Core.Models;

namespace Vaultr.CMS.Authentication;

public partial class LoginScreen
{
    public ConfigurationState Config { get; set; } = new ConfigurationState();

    [Inject]
    public NavigationManager Navigation { get; set; } = null!;

    [Inject]
    public IConfigurationStateProvider ConfigurationStateProvider { get; set; } = null!;

    private void HandleSubmit(EditContext context)
    {
        ConfigurationStateProvider.SetState(Config);

        Navigation.NavigateTo("/", true);
    }
}
