using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RapidCMS.Core.Abstractions.Data;
using RapidCMS.Core.Abstractions.Forms;
using RapidCMS.Core.Abstractions.Repositories;
using RapidCMS.Core.Extensions;
using Vaultr.Client.Data.Models;

namespace Vaultr.Client.Data.Repositories;

public class KeyVaultRespository : IRepository
{
    private readonly SecretClientsProvider _secretClientsProvider;

    public KeyVaultRespository(SecretClientsProvider secretClientsProvider)
    {
        _secretClientsProvider = secretClientsProvider;
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

        return secrets.OrderBy(x => x.Id);
    }

    public Task<IEnumerable<IEntity>> GetAllNonRelatedAsync(IRelatedViewContext viewContext, IView view)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IEntity>> GetAllRelatedAsync(IRelatedViewContext viewContext, IView view)
    {
        throw new NotImplementedException();
    }

    public Task<IEntity?> GetByIdAsync(string id, IViewContext viewContext)
    {
        throw new NotImplementedException();
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
