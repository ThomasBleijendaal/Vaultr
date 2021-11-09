using Vaultr.Client.Core.Models;

namespace Vaultr.Client.Core.Abstractions;

public interface IConfigurationStateProvider
{
    ConfigurationState GetCurrentState();

    void SetState(ConfigurationState state);
}
