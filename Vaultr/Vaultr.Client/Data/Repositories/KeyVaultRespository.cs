using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using RapidCMS.Core.Abstractions.Data;
using RapidCMS.Core.Abstractions.Forms;
using RapidCMS.Core.Abstractions.Repositories;
using RapidCMS.Core.Extensions;
using Vaultr.Client.Data.Models;

namespace Vaultr.Client.Data.Repositories;

public class KeyVaultRespository : IRepository
{
    private readonly SecretClientsProvider _secretClientsProvider;
    private readonly IMemoryCache _memoryCache;

    public KeyVaultRespository(
        SecretClientsProvider secretClientsProvider,
        IMemoryCache memoryCache)
    {
        _secretClientsProvider = secretClientsProvider;
        _memoryCache = memoryCache;
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
        return await _memoryCache.GetOrCreateAsync("secrets", async (entry) =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);

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
        });
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
    {
        var entity = new KeyVaultSecretEntity
        {
            KeyVaultUris = _secretClientsProvider.Clients.Keys.ToDictionary(x => x, x => new Uri("http://localhost")),
            Values = _secretClientsProvider.Clients.Keys.ToDictionary(x => x, x => "")
        };

        return Task.FromResult<IEntity>(entity);
    }

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
