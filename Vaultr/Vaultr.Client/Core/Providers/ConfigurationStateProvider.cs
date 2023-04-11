using Newtonsoft.Json;
using RapidCMS.Core.Abstractions.Mediators;
using Vaultr.Client.Core.Abstractions;
using Vaultr.Client.Core.Models;

namespace Vaultr.Client.Core.Providers;

public class ConfigurationStateProvider : IConfigurationStateProvider
{
    private static ConfigurationState? _currentState { get; set; }
    private readonly List<ConfigurationState> _config;
    private readonly string _storageFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "settings.json");
    private readonly IMediator _mediator;

    public ConfigurationStateProvider(
        IMediator mediator)
    {
        if (File.Exists(_storageFile))
        {
            _config = JsonConvert.DeserializeObject<List<ConfigurationState>>(File.ReadAllText(_storageFile)) ?? new List<ConfigurationState>();
        }

        _config ??= new List<ConfigurationState>();
        _mediator = mediator;
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

        Notify();
    }

    public IReadOnlyList<ConfigurationState> GetConfigurations() => _config;

    public void AddState(ConfigurationState state)
    {
        state.SanitizeKeyVaults();
        _config.Add(state);
        SaveConfig();
    }

    public void UpdateState(ConfigurationState state)
    {
        state.SanitizeKeyVaults();
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

    private void Notify()
    {
        if (_currentState == null)
        {
            return;
        }

        _mediator.NotifyEvent(
            this,
            new StateChangedEventArgs(_currentState));
    }
}
