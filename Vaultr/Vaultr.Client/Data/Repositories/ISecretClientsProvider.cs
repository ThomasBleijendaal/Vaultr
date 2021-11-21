using System.Collections.Generic;
using Azure.Security.KeyVault.Secrets;

namespace Vaultr.Client.Data.Repositories
{
    public interface ISecretClientsProvider
    {
        IReadOnlyDictionary<string, SecretClient> Clients { get; }

        void Build();
    }
}