using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using RapidCMS.Core.Abstractions.Mediators;
using RapidCMS.Core.Enums;
using RapidCMS.Core.Extensions;
using RapidCMS.Core.Models.EventArgs.Mediators;
using Vaultr.Client.Data.Models;

namespace Vaultr.Client.Data.Repositories;

public class SecretsProvider : ISecretsProvider
{
    private readonly ISecretClientsProvider _secretClientsProvider;
    private readonly IMediator _mediator;

    private List<KeyVaultSecretEntity> _secrets = new();
    private readonly Task _initTask;

    public SecretsProvider(
        ISecretClientsProvider secretClientsProvider,
        IMediator mediator)
    {
        _secretClientsProvider = secretClientsProvider;
        _mediator = mediator;
        _initTask = RefreshSecretCacheAsync();
    }

    public bool CanPromote(string keyVaultName)
        => GetKeyVaultIndex(keyVaultName) < _secretClientsProvider.Clients.Count - 1;

    public bool CanDemote(string keyVaultName)
        => GetKeyVaultIndex(keyVaultName) > 0;

    public async Task CopySecretValueAsync(string keyVaultName, string keyName, string targetKeyVaultName) 
        => await SaveSecretAsync(targetKeyVaultName, keyName, await GetSecretValueAsync(keyVaultName, keyName));

    public async Task DeleteSecretAsync(string keyVaultName, string keyName)
    {
        var client = Client(keyVaultName);
        var deletion = await client.StartDeleteSecretAsync(keyName);
        await deletion.WaitForCompletionAsync();

        var secret = _secrets.First(x => x.Id == keyName);

        secret.KeyVaultUris.Remove(keyVaultName);

        if (!secret.KeyVaultUris.Any())
        {
            _secrets.Remove(secret);
        }
    }

    public async Task<IReadOnlyList<KeyVaultSecretEntity>> GetAllSecretsAsync()
    {
        await _initTask;
        return _secrets.ToList();
    }

    public KeyVaultSecretEntity GetEmptySecret()
    {
        var entity = new KeyVaultSecretEntity();

        return entity;
    }

    public async Task<string> GetSecretValueAsync(string keyVaultName, string keyName) 
        => (await Client(keyVaultName).GetSecretAsync(keyName)).Value.Value;

    public string? NextKeyVaultName(string keyVaultName)
        => _secretClientsProvider.Clients.Keys.ElementAt(GetKeyVaultIndex(keyVaultName) + 1);
    
    public string? PreviousKeyVaultName(string keyVaultName)
        => _secretClientsProvider.Clients.Keys.ElementAt(GetKeyVaultIndex(keyVaultName) - 1);

    public async Task SaveSecretValueAsync(string keyVaultName, string keyName, string keyValue) 
        => await SaveSecretAsync(keyVaultName, keyName, keyValue);

    private SecretClient Client(string keyVaultName) 
        => _secretClientsProvider.Clients[keyVaultName];

    private async Task RefreshSecretCacheAsync()
    {
        var secrets = new List<KeyVaultSecretEntity>();

        foreach (var kv in _secretClientsProvider.Clients)
        {
            var keyVaultSecrets = await kv.Value.GetPropertiesOfSecretsAsync().ToListAsync();

            foreach (var keyVaultSecret in keyVaultSecrets)
            {
                var secret = secrets.FirstOrDefault(x => x.Id == keyVaultSecret.Name);
                if (secret == null)
                {
                    secret = new KeyVaultSecretEntity
                    {
                        Id = keyVaultSecret.Name
                    };
                    secrets.Add(secret);
                }

                secret.KeyVaultUris.Add(kv.Key, keyVaultSecret.Id);
            }
        }

        _secrets = secrets.OrderBy(x => x.Id).ToList();
    }

    private async Task SaveSecretAsync(string keyVaultName, string keyName, string keyValue)
    {
        var client = Client(keyVaultName);
        await RecoverSecretAsync(client, keyName);
        var response = await client.SetSecretAsync(keyName, keyValue);

        var secret = _secrets.FirstOrDefault(x => x.Id == keyName);

        if (secret == null)
        {
            secret = GetEmptySecret();
            secret.Id = keyName;
            secret.KeyVaultUris[keyVaultName] = response.Value.Id;
            _secrets.Insert(0, secret);
        } 
        else
        {
            secret.KeyVaultUris[keyVaultName] = response.Value.Id;
        }
    }

    private async Task RecoverSecretAsync(SecretClient targetSecretClient, string keyName)
    {
        var hasDeletedSecret = false;
        try
        {
            var deletedSecret = await targetSecretClient.GetDeletedSecretAsync(keyName);
            hasDeletedSecret = deletedSecret.GetRawResponse().Status == 200;
        }
        catch
        {

        }

        if (hasDeletedSecret)
        {
            var recovery = await targetSecretClient.StartRecoverDeletedSecretAsync(keyName);

            _mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Information, $"Recovering deleted secret.."));

            await recovery.WaitForCompletionAsync();

            _mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Information, $"Deleted secret recovered."));
        }
    }

    private int GetKeyVaultIndex(string keyVaultName)
        => _secretClientsProvider.Clients.Keys.ToList().IndexOf(keyVaultName);
}
