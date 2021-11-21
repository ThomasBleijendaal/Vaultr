using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private ISecretClientsProvider SecretClientsProvider { get; set; } = null!;

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

                var client = SecretClientsProvider.Clients[kv.Name];

                var secret = await client.GetSecretAsync(_secret.Id);

                await client.SetSecretAsync(Clone.NewName, secret.Value.Value);

                Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Success, $"Cloned secret to {kv.Name}!"));
            }

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
