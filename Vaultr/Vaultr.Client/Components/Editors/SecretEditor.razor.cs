using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RapidCMS.Core.Abstractions.Mediators;
using RapidCMS.Core.Enums;
using RapidCMS.Core.Forms;
using RapidCMS.Core.Models.EventArgs.Mediators;
using Vaultr.Client.Components.Panes;
using Vaultr.Client.Core.Abstractions;
using Vaultr.Client.Data.Repositories;

namespace Vaultr.Client.Components.Editors
{
    public partial class SecretEditor
    {
        private string? Value { get; set; }
        private bool IsDecrypted { get; set; }
        private bool IsModified { get; set; }
        private bool IsEmpty => string.IsNullOrEmpty(GetValueAsString());
        private bool IsEncrypted => EditContext.EntityState == EntityState.IsExisting && !IsEmpty && !IsDecrypted;

        private bool? CanPromote => SecretsProvider.CanPromote(KeyVaultName) is bool can ? can && !IsEmpty : null;
        private bool? CanDemote => SecretsProvider.CanDemote(KeyVaultName) is bool can ? can && !IsEmpty : null;

        private string KeyVaultName => Configuration as string ?? throw new InvalidOperationException("Missing keyvault name in Configuration");

        private string? NextKeyVaultName => CanPromote == true ? SecretsProvider.NextKeyVaultName(KeyVaultName) : null;
        private string? PreviousKeyVaultName => CanDemote == true ? SecretsProvider.PreviousKeyVaultName(KeyVaultName) : null;

        [Inject] private ISecretsProvider SecretsProvider { get; set; } = null!;

        [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

        [Inject] private IMediator Mediator { get; set; } = null!;

        [Inject] private IDangerModeProvider DangerModeProvider { get; set; } = null!;

        private string KeyName => EditContext.Entity.Id ?? "";

        private string GetValue()
        {
            return IsDecrypted
                ? (Value ?? "")
                : !IsEmpty && EditContext.EntityState == EntityState.IsExisting
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
                IsDecrypted = true;
                Value = await SecretsProvider.GetSecretValueAsync(KeyVaultName, KeyName);

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
                Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Information, "Saving secret.."));

                await SecretsProvider.SaveSecretValueAsync(KeyVaultName, KeyName, Value ?? "");

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
                Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Information, "Copying secret.."));

                await SecretsProvider.CopySecretValueAsync(KeyVaultName, KeyName, targetKeyVault);

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
                Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Information, "Getting secret.."));

                var secretValue = await SecretsProvider.GetSecretValueAsync(KeyVaultName, KeyName);
                await JsRuntime.InvokeVoidAsync("clipboardCopy.copyText", secretValue);

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

                    await SecretsProvider.DeleteSecretAsync(KeyVaultName, KeyName);

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
    }
}
