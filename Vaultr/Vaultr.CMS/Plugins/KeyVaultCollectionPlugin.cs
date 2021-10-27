using Microsoft.AspNetCore.Components;
using RapidCMS.Core.Abstractions.Metadata;
using RapidCMS.Core.Abstractions.Plugins;
using RapidCMS.Core.Abstractions.Resolvers;
using RapidCMS.Core.Abstractions.Setup;
using RapidCMS.Core.Enums;
using RapidCMS.Core.Models.Setup;
using Vaultr.CMS.Metadata;
using Vaultr.CMS.Models;
using Vaultr.CMS.Repositories;
using Vaultr.Core.Abstractions;

namespace Vaultr.CMS.Plugins;

public class KeyVaultCollectionPlugin : IPlugin
{
    private readonly IConfigurationStateProvider _configurationStateProvider;
    private readonly IExpressionMetadata _keyVaultSecretIdExpression = new ExpressionMetadata<KeyVaultSecretEntity>("Id", x => x.Id ?? string.Empty);

    public KeyVaultCollectionPlugin(
        IConfigurationStateProvider configurationStateProvider)
    {
        _configurationStateProvider = configurationStateProvider;
    }

    public string CollectionPrefix => "vaultr";

    public Task<IResolvedSetup<CollectionSetup>?> GetCollectionAsync(string collectionAlias)
    {
        var state = _configurationStateProvider.GetCurrentState();
        if (state == null)
        {
            return null;
        } 

        var collection = new CollectionSetup("AzureKeyVault", "RedOrange10", "Secrets", "vaultr::secrets", "vaultr::secrets")
        {
            TreeView = new TreeViewSetup(
                EntityVisibilty.Hidden,
                CollectionRootVisibility.Visible,
                false,
                false,
                _keyVaultSecretIdExpression),
            EntityVariant = new EntityVariantSetup("KeyVaultSecret", "AzureKeyVault", typeof(KeyVaultSecretEntity), "vaultr::secretentity"),
            Collections = new List<ITreeElementSetup>(),
            UsageType = UsageType.List,
            ListEditor = new ListSetup(
                20,
                false,
                false,
                ListType.Table,
                EmptyVariantColumnVisibility.Collapse,
                new List<IPaneSetup>
                {
                    new PaneSetup(
                        default, 
                        default, 
                        (m, s) => true, 
                        typeof(KeyVaultSecretEntity), 
                        new List<IButtonSetup>(), 
                        new List<IFieldSetup>
                        {
                            new ExpressionFieldSetup()
                            {
                                Name = "Id",
                                Expression = _keyVaultSecretIdExpression,
                                Index = 0
                            }
                        }.Concat(Enumerable.Range(0, state.NumberOfKeyVaults).Select(x =>
                            new PropertyFieldSetup
                            {
                                Name = $"Key{x + 1}",
                                Property = new PropertyMetadata<KeyVaultSecretEntity>($"Key{x}", typeof(string), (e) => e.Values[x], (e, v) => e.Values[x] = ((string?)v) ?? "", Guid.NewGuid().ToString()),
                                Index = x
                            })
                        ).ToList(),
                        new List<ISubCollectionListSetup>(),
                        new List<IRelatedCollectionListSetup>())
                },
                new List<IButtonSetup>
                {

                })
            {
                
            }
        };

        return Task.FromResult<IResolvedSetup<CollectionSetup>?>(new ResolvedSetup<CollectionSetup>(collection, false));
    }

    public Type? GetRepositoryType(string collectionAlias) => typeof(KeyVaultRespository);

    public Task<IEnumerable<ITreeElementSetup>> GetTreeElementsAsync()
        => Task.FromResult<IEnumerable<ITreeElementSetup>>(
            new ITreeElementSetup[]
            {
                new TreeElementSetup("vaultr::secrets", "KeyVault Secrets", PageType.Collection) 
            });
}


