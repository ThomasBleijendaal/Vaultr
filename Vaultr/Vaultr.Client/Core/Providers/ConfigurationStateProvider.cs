using Blazored.LocalStorage;
using Vaultr.Client.Core.Abstractions;
using Vaultr.Client.Core.Models;

namespace Vaultr.Client.Core.Providers
{
    public class ConfigurationStateProvider : IConfigurationStateProvider
    {
        private readonly ISyncLocalStorageService _localStorage;
        private ConfigurationState _state;

        public ConfigurationStateProvider(
            ISyncLocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public ConfigurationState GetCurrentState()
            => _state ??= _localStorage.GetItem<ConfigurationState>("vaultr-config");

        public void SetState(ConfigurationState state)
        {
            state.KeyVaults = state.KeyVaults
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .GroupBy(x => x.Name)
                .Select(x => x.First())
                .ToList();

            _localStorage.SetItem("vaultr-config", state);
            _state = state;
        }
    }
}
