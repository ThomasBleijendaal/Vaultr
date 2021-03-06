using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Vaultr.Client.Core.Abstractions;

namespace Vaultr.Client.Data.Repositories;

public class SecretClientsProvider : ISecretClientsProvider
{
    private readonly IConfigurationStateProvider _configurationStateProvider;
    private readonly Dictionary<string, SecretClient> _clients = new();

    public SecretClientsProvider(IConfigurationStateProvider configurationStateProvider)
    {
        _configurationStateProvider = configurationStateProvider;
        Build();
    }

    public void Build()
    {
        _clients.Clear();

        var state = _configurationStateProvider.GetCurrentState();

        if (state.IsValid())
        {
            var credential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
            {
                TenantId = state.TenantId
            });

            foreach (var kv in state.KeyVaults)
            {
                _clients.Add(kv.Name, new SecretClient(
                    new Uri($"https://{kv.Name}.vault.azure.net"),
                    credential, 
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
