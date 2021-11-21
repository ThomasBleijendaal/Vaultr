using System.Collections.Generic;
using System.Threading.Tasks;
using Vaultr.Client.Data.Models;

namespace Vaultr.Client.Data.Repositories;

public interface ISecretsProvider
{
    KeyVaultSecretEntity GetEmptySecret();

    Task<IReadOnlyList<KeyVaultSecretEntity>> GetAllSecretsAsync();

    Task<string> GetSecretValueAsync(string keyVaultName, string keyName);

    Task SaveSecretValueAsync(string keyVaultName, string keyName, string keyValue);

    Task CopySecretValueAsync(string keyVaultName, string keyName, string targetKeyVaultName);

    Task DeleteSecretAsync(string keyVaultName, string keyName);

    bool CanPromote(string keyVaultName);
    bool CanDemote(string keyVaultName);

    string? NextKeyVaultName(string keyVaultName);
    string? PreviousKeyVaultName(string keyVaultName);
}
