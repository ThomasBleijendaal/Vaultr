using System;
using System.Linq;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RapidCMS.Core.Abstractions.Mediators;
using RapidCMS.Core.Enums;
using RapidCMS.Core.Forms;
using RapidCMS.Core.Models.EventArgs.Mediators;
using Vaultr.Client.Components.Panes;

namespace Vaultr.Client.Components.Editors
{
    public partial class SecretEditor
    {
        private string? Value { get; set; }
        private bool IsDecrypted { get; set; }
        private bool IsModified { get; set; }
        private bool IsEmpty => string.IsNullOrEmpty(GetValueAsString());
        private bool IsEncrypted => !IsEmpty && !IsDecrypted;

        private int KeyVaultIndex => SecretClientsProvider.Clients.Keys.ToList().IndexOf(KeyVaultName);

        private bool CanPromote => !IsEmpty && KeyVaultIndex < SecretClientsProvider.Clients.Count - 1;
        private bool CanDemote => !IsEmpty && KeyVaultIndex > 0;

        private string KeyVaultName => Configuration as string ?? throw new InvalidOperationException("Missing keyvault name in Configuration");

        private string? NextKeyVaultName => CanPromote ? SecretClientsProvider.Clients.Keys.ElementAt(KeyVaultIndex + 1) : null;
        private string? PreviousKeyVaultName => CanDemote ? SecretClientsProvider.Clients.Keys.ElementAt(KeyVaultIndex - 1) : null;

        private SecretClient SecretClient => SecretClientsProvider.Clients[KeyVaultName];

        [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

        [Inject] private IMediator Mediator { get; set; } = null!;

        private string KeyName => new Uri(GetValueAsString()).AbsolutePath.Split('/').Last();

        private string GetValue()
        {
            return IsDecrypted
                ? (Value ?? "")
                : !IsEmpty
                    ? "***"
                    : "";
        }

        private void SetValue(string value)
        {
            if (!IsEncrypted)
            {
                IsModified = true;
                Value = value;
            }
        }

        private async Task UnlockAsync()
        {
            try
            {
                var secret = await SecretClient.GetSecretAsync(KeyName);

                IsDecrypted = true;
                Value = secret.Value.Value;

                StateHasChanged();
            }
            catch (Exception ex)
            {
                Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Error, $"Failed to get secret: {ex.Message}"));
            }
        }

        private void Lock()
        {
            Value = null;
            IsDecrypted = false;
            IsModified = false;

            StateHasChanged();
        }

        private async Task SaveAsync()
        {
            try
            {
                await RecoverSecretAsync(SecretClient);

                await SecretClient.SetSecretAsync(KeyName, Value);
                Lock();

                Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Success, "Secret saved!"));
                Mediator.NotifyEvent(this, new CollectionRepositoryEventArgs(EditContext.CollectionAlias, EditContext.RepositoryAlias, default, Entity.Id, CrudType.Update));
            }
            catch (Exception ex)
            {
                Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Error, $"Failed to save secret: {ex.Message}"));
            }
        }

        private Task DemoteAsync() => PreviousKeyVaultName != null ? CopyAsync(PreviousKeyVaultName) : Task.CompletedTask;
        private Task PromoteAsync() => NextKeyVaultName != null ? CopyAsync(NextKeyVaultName) : Task.CompletedTask;

        private async Task CopyAsync(string targetKeyVault)
        {
            try
            {
                var secret = await SecretClient.GetSecretAsync(KeyName);

                var targetSecretClient = SecretClientsProvider.Clients[targetKeyVault];

                await RecoverSecretAsync(targetSecretClient);

                await targetSecretClient.SetSecretAsync(KeyName, secret.Value.Value);

                Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Success, "Secret copied!"));
                Mediator.NotifyEvent(this, new CollectionRepositoryEventArgs(EditContext.CollectionAlias, EditContext.RepositoryAlias, default, Entity.Id, CrudType.Update));

                Lock();
            }
            catch (Exception ex)
            {
                Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Error, $"Failed to copy secret: {ex.Message}"));
            }
        }

        private async Task CopyToClipboardAsync()
        {
            try
            {
                var secret = await SecretClient.GetSecretAsync(KeyName, Value);
                await JsRuntime.InvokeVoidAsync("clipboardCopy.copyText", secret.Value.Value);

                Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Success, "Secret copied to clipboard!"));
            }
            catch (Exception ex)
            {
                Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Error, $"Failed to get secret: {ex.Message}"));
            }
        }

        private async Task DeleteAsync()
        {
            try
            {
                if (await Mediator.NotifyEventAsync(this, new PaneRequestEventArgs(typeof(ConfirmPane), EditContext, new ButtonContext(default, KeyVaultName))) == CrudType.Delete)
                {
                    Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Information, "Deleting secret.."));

                    SetValue(null!);

                    var deletion = await SecretClient.StartDeleteSecretAsync(KeyName);
                    await deletion.WaitForCompletionAsync();

                    Lock();

                    Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Success, "Secret deleted!"));
                    Mediator.NotifyEvent(this, new CollectionRepositoryEventArgs(EditContext.CollectionAlias, EditContext.RepositoryAlias, default, Entity.Id, CrudType.Update));
                }
            }
            catch (Exception ex)
            {
                Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Error, $"Failed to delete secret: {ex.Message}"));
            }
        }

        private async Task RecoverSecretAsync(SecretClient targetSecretClient)
        {
            var hasDeletedSecret = false;
            try
            {
                var deletedSecret = await targetSecretClient.GetDeletedSecretAsync(KeyName);
                hasDeletedSecret = deletedSecret.GetRawResponse().Status == 200;
            }
            catch
            {

            }

            if (hasDeletedSecret)
            {
                var recovery = await targetSecretClient.StartRecoverDeletedSecretAsync(KeyName);

                Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Information, $"Recovering deleted secret.."));

                await recovery.WaitForCompletionAsync();

                Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Information, $"Deleted secret recovered."));
            }
        }
    }
}
