using System.Linq;
using Vaultr.Client.Core.Abstractions;
using Vaultr.Client.Core.Models;

namespace Vaultr.Client.Core.Providers;

public class ConfigurationStateProvider : IConfigurationStateProvider
{
    private ConfigurationState? _state;

    public ConfigurationStateProvider()
    {

    }

    public ConfigurationState GetCurrentState()
        => _state ??= new ConfigurationState
        {
            KeyVaults =
            {
                new ConfigurationState.KeyVaultConfiguration { Name = "vaultr-test" },
                new ConfigurationState.KeyVaultConfiguration { Name = "vaultr-prod" }
            },
            TenantId = "324bfecd-c8d2-4233-887b-c1be7fa11256"
        };

    public void SetState(ConfigurationState state)
    {
        state.KeyVaults = state.KeyVaults
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .GroupBy(x => x.Name)
            .Select(x => x.First())
            .ToList();

        _state = state;
    }
}
