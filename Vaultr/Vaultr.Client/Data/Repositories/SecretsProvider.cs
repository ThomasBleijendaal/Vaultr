using System.Collections.Concurrent;
using Azure.Security.KeyVault.Secrets;
using RapidCMS.Core.Abstractions.Mediators;
using RapidCMS.Core.Enums;
using RapidCMS.Core.Extensions;
using RapidCMS.Core.Models.EventArgs.Mediators;
using Vaultr.Client.Core.Extensions;
using Vaultr.Client.Data.Models;

namespace Vaultr.Client.Data.Repositories;

public class SecretsProvider : ISecretsProvider
{
    private readonly ISecretClientsProvider _secretClientsProvider;
    private readonly IMediator _mediator;

    private List<KeyVaultSecretEntity> _secrets = new();
    private Task _initTask;
    private Task _firstPageTask;

    public SecretsProvider(
        ISecretClientsProvider secretClientsProvider,
        IMediator mediator)
    {
        _secretClientsProvider = secretClientsProvider;
        _mediator = mediator;
        _firstPageTask = new TaskCompletionSource().Task;
        _initTask = RefreshSecretCacheAsync();
    }

    public void ClearCache()
    {
        _secrets.Clear();
        _initTask = RefreshSecretCacheAsync();
    }

    public bool? CanPromote(string keyVaultName)
        => GetKeyVaultIndex(keyVaultName) < _secretClientsProvider.Clients.Count - 1 ? true : null;

    public bool? CanDemote(string keyVaultName)
        => GetKeyVaultIndex(keyVaultName) > 0 ? true : null;

    public async Task CopySecretValueAsync(string keyVaultName, string keyName, string targetKeyVaultName)
    {
        await SaveSecretAsync(targetKeyVaultName, keyName, await GetSecretValueAsync(keyVaultName, keyName));
    }

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

    public async Task<IReadOnlyList<KeyVaultSecretEntity>> GetAllSecretsAsync(bool firstPage)
    {
        await (firstPage ? _firstPageTask : _initTask);
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

    public bool HighKeyvaultCount => _secretClientsProvider.Clients.Count > 5;

    private SecretClient Client(string keyVaultName)
        => _secretClientsProvider.Clients[keyVaultName];

    private async Task RefreshSecretCacheAsync()
    {
        var firstPageTcs = new TaskCompletionSource();

        _firstPageTask = firstPageTcs.Task;

        var concurrentDictionary = new ConcurrentDictionary<string, KeyVaultSecretEntity>();

        await foreach (var keyVaultSecret in _secretClientsProvider.Clients.ZipAsync(x => x.GetPropertiesOfSecretsAsync()))
        {
            var keyVault = keyVaultSecret.Key;
            var secret = keyVaultSecret.Value;

            if (secret.Managed)
            {
                continue;
            }

            concurrentDictionary.AddOrUpdate(secret.Name,
                name => new KeyVaultSecretEntity
                {
                    Id = name,
                    KeyVaultUris =
                    {
                        { keyVault, secret.Id }
                    }
                },
                (name, existingSecret) =>
                {
                    var newSecret = new KeyVaultSecretEntity
                    {
                        Id = name,
                        KeyVaultUris = existingSecret.KeyVaultUris.ToDictionary(x => x.Key, x => x.Value)
                    };

                    newSecret.KeyVaultUris[keyVault] = secret.Id;

                    return newSecret;
                });

            if (!_firstPageTask.IsCompleted && concurrentDictionary.Count > 50)
            {
                // provide data for the first page
                _secrets = concurrentDictionary.Values.OrderBy(x => x.Id).ToList();
                firstPageTcs.SetResult();
            }
        }

        _secrets = concurrentDictionary.Values.OrderBy(x => x.Id).ToList();

        _mediator.NotifyEvent(this, new CollectionRepositoryEventArgs(
            "vaultr::secrets",
            "vaultr::secrets",
            default,
            default(string),
            CrudType.Update));
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
            // there is no problem
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
