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
    private ConfigurationState? _state;
    private readonly string _storageFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "settings.json");

    public ConfigurationStateProvider()
    {
        if (File.Exists(_storageFile))
        {
            _state = JsonConvert.DeserializeObject<List<ConfigurationState>>(File.ReadAllText(_storageFile))?.FirstOrDefault();
        }
    }

    public ConfigurationState GetCurrentState()
        => _state ??= new ConfigurationState
        {
            KeyVaults = new List<ConfigurationState.KeyVaultConfiguration> {  new ConfigurationState.KeyVaultConfiguration()},
            TenantId = ""
            //KeyVaults =
            //{
            //    new ConfigurationState.KeyVaultConfiguration { Name = "vaultr-test" },
            //    new ConfigurationState.KeyVaultConfiguration { Name = "vaultr-prod" }
            //},
            //TenantId = "324bfecd-c8d2-4233-887b-c1be7fa11256"
        };

    public void SetState(ConfigurationState state)
    {
        state.KeyVaults = state.KeyVaults
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .GroupBy(x => x.Name)
            .Select(x => x.First())
            .ToList();

        _state = state;

        File.WriteAllText(_storageFile, JsonConvert.SerializeObject(new[] { _state }));
    }
}
