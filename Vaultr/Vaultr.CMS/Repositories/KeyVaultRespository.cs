using RapidCMS.Core.Abstractions.Data;
using RapidCMS.Core.Abstractions.Forms;
using RapidCMS.Core.Abstractions.Repositories;
using RapidCMS.Core.Extensions;
using Vaultr.CMS.Models;
using Vaultr.Core.Abstractions;

namespace Vaultr.CMS.Repositories;

public class KeyVaultRespository : IRepository
{
    private readonly IConfigurationStateProvider _configurationStateProvider;
    private readonly SecretClients _secretClients;

    public KeyVaultRespository(
        SecretClients secretClients,
        IConfigurationStateProvider configurationStateProvider)
    {
        _secretClients = secretClients;
        _configurationStateProvider = configurationStateProvider;
    }

    public Task AddAsync(IRelatedViewContext viewContext, string id)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(string id, IViewContext viewContext)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<IEntity>> GetAllAsync(IViewContext viewContext, IView view)
    {
        var secrets = new List<KeyVaultSecretEntity>();

        foreach (var kv in _secretClients)
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

        return secrets;
    }

    public Task<IEnumerable<IEntity>> GetAllNonRelatedAsync(IRelatedViewContext viewContext, IView view)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IEntity>> GetAllRelatedAsync(IRelatedViewContext viewContext, IView view)
    {
        throw new NotImplementedException();
    }

    public async Task<IEntity?> GetByIdAsync(string id, IViewContext viewContext)
    {
        throw new NotImplementedException();
        //return new KeyVaultSecretEntity
        //{
        //    Id = $"id-{id}",
        //    Values = Enumerable
        //        .Range(1, _configurationStateProvider.GetCurrentState()?.NumberOfKeyVaults ?? 1)
        //        .Select(x => x.ToString())
        //        .ToList()
        //};
    }

    public Task<IEntity?> InsertAsync(IEditContext editContext)
    {
        throw new NotImplementedException();
    }

    public Task<IEntity> NewAsync(IViewContext viewContext, Type? variantType)
        => Task.FromResult<IEntity>(new KeyVaultSecretEntity());

    public Task RemoveAsync(IRelatedViewContext viewContext, string id)
    {
        throw new NotImplementedException();
    }

    public Task ReorderAsync(string? beforeId, string id, IViewContext viewContext)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(IEditContext editContext)
    {
        throw new NotImplementedException();
    }
}
