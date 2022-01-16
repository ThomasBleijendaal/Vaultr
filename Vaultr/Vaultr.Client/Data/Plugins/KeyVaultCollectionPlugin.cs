using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RapidCMS.Core.Abstractions.Plugins;
using RapidCMS.Core.Abstractions.Resolvers;
using RapidCMS.Core.Abstractions.Setup;
using RapidCMS.Core.Enums;
using RapidCMS.Core.Handlers;
using RapidCMS.Core.Models.Setup;
using Vaultr.Client.Components.Buttons;
using Vaultr.Client.Components.Editors;
using Vaultr.Client.Core.Abstractions;
using Vaultr.Client.Data.Metadata;
using Vaultr.Client.Data.Models;
using Vaultr.Client.Data.Repositories;

namespace Vaultr.Client.Data.Plugins;

public class KeyVaultCollectionPlugin : IPlugin
{
    private readonly IConfigurationStateProvider _configurationStateProvider;
    private readonly IEntityVariantSetup _secretVariant = new EntityVariantSetup("KeyVaultSecret", "AzureKeyVault", typeof(KeyVaultSecretEntity), "vaultr::secretentity");

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

        var collection = new CollectionSetup("AzureKeyVault", "Gray40", "Secrets", "vaultr::secrets", "vaultr::secrets")
        {
            TreeView = new TreeViewSetup(
                EntityVisibilty.Hidden,
                CollectionRootVisibility.Visible,
                false,
                false,
                new ExpressionMetadata<KeyVaultSecretEntity>("Id", x => x.Id ?? string.Empty)),
            EntityVariant = _secretVariant,
            Collections = new List<ITreeElementSetup>(),
            UsageType = UsageType.List,
            ListEditor = new ListSetup(
                20,
                true,
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
                            new CustomPropertyFieldSetup(typeof(SecretIdLabel))
                            {
                                Name = "Id",
                                Property = new PropertyMetadata<KeyVaultSecretEntity>(
                                    "Id",
                                    typeof(string),
                                    (e) => e.Id,
                                    (e, v) =>
                                    {
                                        e.Id = ((string?)v) ?? "";
                                        e.KeyVaultUris = e.KeyVaultUris.ToDictionary(
                                            x => x.Key,
                                            x => new Uri($"http://localhost/{v}"));
                                    },
                                    Guid.NewGuid().ToString()),
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
                                    (e, v) =>  { },
                                    Guid.NewGuid().ToString())
                            })
                        ).ToList(),
                        new List<ISubCollectionListSetup>(),
                        new List<IRelatedCollectionListSetup>())
                },
                new List<IButtonSetup>
                {
                    new ButtonSetup()
                    {
                        ButtonHandlerType = typeof(DefaultButtonActionHandler),
                        ButtonId = "newsecret",
                        Buttons = new List<IButtonSetup>(),
                        DefaultButtonType = DefaultButtonType.New,
                        Icon = "New",
                        IsPrimary = true,
                        Label = "Create new secret",
                        EntityVariant = _secretVariant
                    },
                    new ButtonSetup()
                    {
                        ButtonHandlerType = typeof(DefaultButtonActionHandler),
                        ButtonId = "cancelnewsecret",
                        Buttons = new List<IButtonSetup>(),
                        DefaultButtonType = DefaultButtonType.Return,
                        Icon = "Return",
                        IsPrimary = false,
                        Label = "Cancel",
                        EntityVariant = _secretVariant
                    },
                    new ButtonSetup()
                    {
                        ButtonHandlerType = typeof(DefaultButtonActionHandler),
                        ButtonId = "dangermode",
                        Buttons = new List<IButtonSetup>(),
                        DefaultButtonType = DefaultButtonType.New,
                        Icon = "Section",
                        IsPrimary = true,
                        Label = "Enable danger mode",
                        EntityVariant = _secretVariant,
                        CustomType = typeof(DangerModeButton)
                    },
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


