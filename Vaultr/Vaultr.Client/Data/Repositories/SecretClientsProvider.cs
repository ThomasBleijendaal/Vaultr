using Azure.Security.KeyVault.Secrets;
using Vaultr.Client.Core.Abstractions;

namespace Vaultr.Client.Data.Repositories;

public class SecretClientsProvider : ISecretClientsProvider
{
    private readonly ICredentialProvider _credentialProvider;
    private readonly IConfigurationStateProvider _configurationStateProvider;
    private readonly Dictionary<string, SecretClient> _clients = new();

    public SecretClientsProvider(
        ICredentialProvider credentialProvider,
        IConfigurationStateProvider configurationStateProvider)
    {
        _credentialProvider = credentialProvider;
        _configurationStateProvider = configurationStateProvider;
        Build();
    }

    public void Build()
    {
        _clients.Clear();

        var state = _configurationStateProvider.GetCurrentState();

        if (state.IsValid())
        {
            foreach (var kv in state.KeyVaults)
            {
                _clients.Add(kv.Name, new SecretClient(
                    new Uri($"https://{kv.Name}.vault.azure.net"),
                    _credentialProvider.GetTokenCredential(state.TenantId!), 
                    new SecretClientOptions
                    {
                        Retry =
                        {
                            MaxRetries = 1
                        }
                    }));
            }
        }
    }

    public IReadOnlyDictionary<string, SecretClient> Clients => _clients;
}

