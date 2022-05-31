using Vaultr.Client.Core.Models;

namespace Vaultr.Client.Core.Abstractions;

public interface IConfigurationStateProvider
{
    IReadOnlyList<ConfigurationState> GetConfigurations(); 

    ConfigurationState GetCurrentState();

    void SetCurrentState(ConfigurationState? state);

    void AddState(ConfigurationState state);

    void UpdateState(ConfigurationState state);

    void RemoveState(ConfigurationState state);
}
