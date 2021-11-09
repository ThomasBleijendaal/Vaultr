using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RapidCMS.Core.Abstractions.Metadata;
using RapidCMS.Core.Abstractions.Plugins;
using RapidCMS.Core.Abstractions.Resolvers;
using RapidCMS.Core.Abstractions.Setup;
using RapidCMS.Core.Enums;
using RapidCMS.Core.Models.Setup;
using Vaultr.Client.Components.Editors;
using Vaultr.Client.Core.Abstractions;
using Vaultr.Client.Data.Metadata;
using Vaultr.Client.Data.Models;
using Vaultr.Client.Data.Repositories;

namespace Vaultr.Client.Data.Plugins;

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
            return Task.FromResult(default(IResolvedSetup<CollectionSetup>));
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
                        new List<IButtonSetup>
                        {

                        },
                        new List<IFieldSetup>
                        {
                            new ExpressionFieldSetup()
                            {
                                Name = "Id",
                                Expression = _keyVaultSecretIdExpression,
                                Index = 0
                            }
                        }.Concat(state.KeyVaults.Select(x =>
                            new CustomPropertyFieldSetup(typeof(SecretEditor))
                            {
                                Name = x.Name,
                                Configuration = x.Name,
                                Property = new PropertyMetadata<KeyVaultSecretEntity>(
                                    x.Name,
                                    typeof(string),
                                    (e) => e.KeyVaultUris.TryGetValue(x.Name, out var uri) ? uri.ToString() : null,
                                    (e, v) => e.Values[x.Name] = (string?)v ?? "",
                                    Guid.NewGuid().ToString())
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


