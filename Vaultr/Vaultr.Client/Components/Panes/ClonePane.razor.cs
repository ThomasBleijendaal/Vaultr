using Microsoft.AspNetCore.Components;
using RapidCMS.Core.Abstractions.Mediators;
using RapidCMS.Core.Enums;
using RapidCMS.Core.Models.EventArgs.Mediators;
using Vaultr.Client.Data.Models;
using Vaultr.Client.Data.Repositories;

namespace Vaultr.Client.Components.Panes
{
    public partial class ClonePane
    {
        [Inject]
        private ISecretsProvider SecretsProvider { get; set; } = null!;

        [Inject]
        private IMediator Mediator { get; set; } = null!;

        public CloneModel Clone = new CloneModel();

        private KeyVaultSecretEntity? _secret;

        protected override void OnInitialized()
        {
            if (EditContext.Entity is KeyVaultSecretEntity secret)
            {
                _secret = secret;
                Clone.NewName = secret.Id ?? "";

                foreach (var kv in secret.KeyVaultUris)
                {
                    Clone.KeyVaults.Add(new CloneModel.KeyVaultClone { Name = kv.Key, ShouldClone = true });
                }
            }
        }

        private async Task CloneAsync()
        {
            if (_secret == null)
            {
                ButtonClicked(CrudType.None);
                return;
            }

            foreach (var kv in Clone.KeyVaults.Where(x => x.ShouldClone))
            {
                Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Information, $"Cloning secret to {kv.Name}.."));

                await SecretsProvider.SaveSecretValueAsync(
                    kv.Name,
                    Clone.NewName,
                    await SecretsProvider.GetSecretValueAsync(kv.Name, _secret.Id!));

                Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Success, $"Cloned secret to {kv.Name}!"));
            }

            SecretsProvider.ClearCache();

            ButtonClicked(CrudType.Refresh);
        }
    }

    public class CloneModel
    {
        public string NewName { get; set; } = "";

        public List<KeyVaultClone> KeyVaults { get; set; } = new();

        public class KeyVaultClone
        {
            public string Name { get; set; } = null!;
            public bool ShouldClone { get; set; }
        }
    }
}
