using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RapidCMS.Core.Abstractions.Data;
using RapidCMS.Core.Abstractions.Forms;
using RapidCMS.Core.Abstractions.Repositories;

namespace Vaultr.Client.Data.Repositories;

public class KeyVaultRespository : IRepository
{
    private readonly ISecretsProvider _secretsProvider;

    public KeyVaultRespository(ISecretsProvider secretsProvider)
    {
        _secretsProvider = secretsProvider;
    }

    public Task AddAsync(IRelatedViewContext viewContext, string id) => throw new NotImplementedException();

    public Task DeleteAsync(string id, IViewContext viewContext) => throw new NotImplementedException();

    public async Task<IEnumerable<IEntity>> GetAllAsync(IViewContext viewContext, IView view) 
        => (await _secretsProvider.GetAllSecretsAsync())
            .Where(x => view.SearchTerm == null || (x.Id != null && x.Id.Contains(view.SearchTerm)));

    public Task<IEnumerable<IEntity>> GetAllNonRelatedAsync(IRelatedViewContext viewContext, IView view) => throw new NotImplementedException();

    public Task<IEnumerable<IEntity>> GetAllRelatedAsync(IRelatedViewContext viewContext, IView view) => throw new NotImplementedException();

    public Task<IEntity?> GetByIdAsync(string id, IViewContext viewContext) => throw new NotImplementedException();

    public Task<IEntity?> InsertAsync(IEditContext editContext) => throw new NotImplementedException();

    public Task<IEntity> NewAsync(IViewContext viewContext, Type? variantType) => Task.FromResult<IEntity>(_secretsProvider.GetEmptySecret());

    public Task RemoveAsync(IRelatedViewContext viewContext, string id) => throw new NotImplementedException();

    public Task ReorderAsync(string? beforeId, string id, IViewContext viewContext) => throw new NotImplementedException();

    public Task UpdateAsync(IEditContext editContext) => throw new NotImplementedException();
}
