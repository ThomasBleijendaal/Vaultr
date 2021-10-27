using Vaultr.Core.Models;

namespace Vaultr.Core.Abstractions
{
    public interface IConfigurationStateProvider
    {
        ConfigurationState? GetCurrentState();

        void SetState(ConfigurationState state);
    }
}
