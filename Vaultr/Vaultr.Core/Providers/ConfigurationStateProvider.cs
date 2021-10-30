﻿using Blazored.LocalStorage;
using Vaultr.Core.Abstractions;
using Vaultr.Core.Models;

namespace Vaultr.Core.Providers
{
    public class ConfigurationStateProvider : IConfigurationStateProvider
    {
        private readonly ISyncLocalStorageService _localStorage;
        private ConfigurationState? _state;

        public ConfigurationStateProvider(
            ISyncLocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public ConfigurationState? GetCurrentState() 
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
