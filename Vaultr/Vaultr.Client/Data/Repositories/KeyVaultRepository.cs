﻿using RapidCMS.Core.Abstractions.Data;
using RapidCMS.Core.Abstractions.Forms;
using RapidCMS.Core.Abstractions.Repositories;
using Vaultr.Client.Data.Models;
using IView = RapidCMS.Core.Abstractions.Data.IView;

namespace Vaultr.Client.Data.Repositories;

public class KeyVaultRepository : IRepository
{
    private readonly ISecretsProvider _secretsProvider;

    public KeyVaultRepository(ISecretsProvider secretsProvider)
    {
        _secretsProvider = secretsProvider;
    }

    public Task AddAsync(IRelatedViewContext viewContext, string id) => throw new NotImplementedException();

    public Task DeleteAsync(string id, IViewContext viewContext) => throw new NotImplementedException();

    public async Task<IEnumerable<IEntity>> GetAllAsync(IViewContext viewContext, IView view)
        => (await _secretsProvider.GetAllSecretsAsync())
            .Where(CompileQueryExpression(view))
            .Where(x => view.SearchTerm == null || (x.Id != null && x.Id.Contains(view.SearchTerm, StringComparison.InvariantCultureIgnoreCase)));

    private Func<KeyVaultSecretEntity, bool> CompileQueryExpression(IView view)
    {
        var expression = (view.ActiveDataView?.QueryExpression.Compile()) as Func<KeyVaultSecretEntity, bool>;

        return expression ?? NoFilter;
    }

    public Task<IEnumerable<IEntity>> GetAllNonRelatedAsync(IRelatedViewContext viewContext, IView view) => throw new NotImplementedException();

    public Task<IEnumerable<IEntity>> GetAllRelatedAsync(IRelatedViewContext viewContext, IView view) => throw new NotImplementedException();

    public Task<IEntity?> GetByIdAsync(string id, IViewContext viewContext) => throw new NotImplementedException();

    public Task<IEntity?> InsertAsync(IEditContext editContext) => throw new NotImplementedException();

    public Task<IEntity> NewAsync(IViewContext viewContext, Type? variantType) => Task.FromResult<IEntity>(_secretsProvider.GetEmptySecret());

    public Task RemoveAsync(IRelatedViewContext viewContext, string id) => throw new NotImplementedException();

    public Task ReorderAsync(string? beforeId, string id, IViewContext viewContext) => throw new NotImplementedException();

    public Task UpdateAsync(IEditContext editContext) => throw new NotImplementedException();

    private bool NoFilter(KeyVaultSecretEntity e) => true;
}