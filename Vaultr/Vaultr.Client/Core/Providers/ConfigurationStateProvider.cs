using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Vaultr.Client.Core.Abstractions;
using Vaultr.Client.Core.Models;

namespace Vaultr.Client.Core.Providers;

public class ConfigurationStateProvider : IConfigurationStateProvider
{
    private List<ConfigurationState> _config;
    private ConfigurationState? _currentState;
    private readonly string _storageFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "settings.json");

    public ConfigurationStateProvider()
    {
        if (File.Exists(_storageFile))
        {
            _config = JsonConvert.DeserializeObject<List<ConfigurationState>>(File.ReadAllText(_storageFile)) ?? new List<ConfigurationState>();
        }

        _config ??= new List<ConfigurationState>();
    }

    public ConfigurationState GetCurrentState()
        => _currentState ?? new ConfigurationState
        {
            KeyVaults = new List<ConfigurationState.KeyVaultConfiguration> { new ConfigurationState.KeyVaultConfiguration() },
            TenantId = ""
        };

    public void SetCurrentState(ConfigurationState? state)
    {
        _currentState = state;
    }

    public IReadOnlyList<ConfigurationState> GetConfigurations() => _config;

    public void AddState(ConfigurationState state)
    {
        state.KeyVaults = state.KeyVaults
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .GroupBy(x => x.Name)
            .Select(x => x.First())
            .ToList();

        _config.Add(state);
        SaveConfig();
    }

    public void RemoveState(ConfigurationState state)
    {
        _config.Remove(state);
        SaveConfig();
    }

    private void SaveConfig()
    {
        File.WriteAllText(_storageFile, JsonConvert.SerializeObject(_config));
    }
}
