using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.JSInterop;
using RapidCMS.Core.Abstractions.Mediators;
using RapidCMS.Core.Enums;
using RapidCMS.Core.Forms;
using RapidCMS.Core.Models.EventArgs.Mediators;
using Vaultr.Client.Components.Panes;
using Vaultr.Client.Core;
using Vaultr.Client.Core.Abstractions;
using Vaultr.Client.Data.Repositories;

namespace Vaultr.Client.Components.Editors;

public partial class SecretEditor
{
    private enum Action
    {
        Nothing,
        Deleting,
        Saving, 
        Copying,
        Promoting,
        Demoting,
        Unlocking
    }

    private static Guid _latestEditorIdToCopy;
    private readonly Guid _editorId = Guid.NewGuid();

    private IDisposable? _highlightEvent;
    private IDisposable? _copyEvent;

    private string? Value { get; set; }
    private bool IsDecrypted { get; set; }
    private bool IsModified { get; set; }
    private bool IsEmpty => string.IsNullOrEmpty(GetValueAsString());
    private bool IsEncrypted => EditContext.EntityState == EntityState.IsExisting && !IsEmpty && !IsDecrypted;

    private bool? CanPromote => SecretsProvider.CanPromote(KeyVaultName) is bool can ? can && !IsEmpty : null;
    private bool? CanDemote => SecretsProvider.CanDemote(KeyVaultName) is bool can ? can && !IsEmpty : null;

    private Action IsDoing { get; set; }

    private string KeyVaultName => Configuration as string ?? throw new InvalidOperationException("Missing keyvault name in Configuration");

    private string? NextKeyVaultName => CanPromote == true ? SecretsProvider.NextKeyVaultName(KeyVaultName) : null;
    private string? PreviousKeyVaultName => CanDemote == true ? SecretsProvider.PreviousKeyVaultName(KeyVaultName) : null;

    [Inject] private ISecretsProvider SecretsProvider { get; set; } = null!;

    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

    [Inject] private IMediator Mediator { get; set; } = null!;

    [Inject] private IDangerModeProvider DangerModeProvider { get; set; } = null!;

    [Inject] private IMemoryCache MemoryCache { get; set; } = null!;

    private string KeyName => EditContext.Entity.Id ?? "";

    protected override void OnParametersSet()
    {
        // survive refresh when a secret for another keyvault is saved
        if (!string.IsNullOrWhiteSpace(EditContext.Entity.Id) && EditContext.EntityState == EntityState.IsExisting)
        {
            Value = ((string?)MemoryCache.Get(CacheKey())) ?? "";

            if (!string.IsNullOrEmpty(Value))
            {
                IsDecrypted = true;
                IsModified = true;

                // refresh the lease
                MemoryCache.Set(CacheKey(), Value, TimeSpan.FromSeconds(60));
            }
        }
    }

    protected override void AttachListener()
    {
        _highlightEvent = Mediator.RegisterCallback<HighlightEventArgs>(async (sender, args) =>
        {
            _latestEditorIdToCopy = args.EditorId;

            await InvokeAsync(() => StateHasChanged());
        });

        _copyEvent = Mediator.RegisterCallback<CopyEventArgs>(async (sender, args) =>
        {
            if (KeyVaultName == args.KeyVaultName && KeyName == args.KeyName)
            {
                await InvokeAsync(() => StateHasChanged());
            }
        });
    }

    protected override void DetachListener()
    {
        _highlightEvent?.Dispose();
        _highlightEvent = null;

        _copyEvent?.Dispose();
        _copyEvent = null;
    }

    private string GetValue()
        => IsDecrypted
            ? (Value ?? "")
            : !IsEmpty && EditContext.EntityState == EntityState.IsExisting
                ? "***"
                : "";

    private void SetValue(string value)
    {
        if (!IsEncrypted)
        {
            IsModified = true;
            Value = value;

            if (!string.IsNullOrWhiteSpace(EditContext.Entity.Id))
            {
                MemoryCache.Set(CacheKey(), value, TimeSpan.FromSeconds(60));
            }
        }
    }

    private async Task UnlockAsync()
    {
        try
        {
            IsDoing = Action.Unlocking;

            IsDecrypted = true;
            Value = await SecretsProvider.GetSecretValueAsync(KeyVaultName, KeyName);

            MemoryCache.Remove(CacheKey());

            StateHasChanged();
        }
        catch (Exception ex)
        {
            Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Error, $"Failed to get secret: {ex.Message}"));
        }
        finally
        {
            DoNothingIf(Action.Unlocking);
        }
    }

    private void Lock()
    {
        Value = null;
        IsDecrypted = false;
        IsModified = false;

        MemoryCache.Remove(CacheKey());

        StateHasChanged();
    }

    private async Task SaveAsync()
    {
        try
        {
            IsDoing = Action.Saving;

            MemoryCache.Remove(CacheKey());

            Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Information, "Saving secret.."));

            await SecretsProvider.SaveSecretValueAsync(KeyVaultName, KeyName, Value ?? "");

            Lock();

            Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Success, "Secret saved!"));

            Mediator.NotifyEvent(this, new CollectionRepositoryEventArgs(EditContext.CollectionAlias, EditContext.RepositoryAlias, EditContext.Parent?.GetParentPath(), EditContext.Entity.Id, CrudType.Add));

            StateHasChanged();
        }
        catch (Exception ex)
        {
            Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Error, $"Failed to save secret: {ex.Message}"));
        }
        finally
        {
            DoNothingIf(Action.Saving);
        }
    }

    private Task DemoteAsync() => PreviousKeyVaultName != null ? CopyAsync(PreviousKeyVaultName, Action.Demoting) : Task.CompletedTask;
    private Task PromoteAsync() => NextKeyVaultName != null ? CopyAsync(NextKeyVaultName, Action.Promoting) : Task.CompletedTask;

    private async Task CopyAsync(string targetKeyVault, Action action)
    {
        try
        {
            IsDoing = action;

            Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Information, "Copying secret.."));

            await SecretsProvider.CopySecretValueAsync(KeyVaultName, KeyName, targetKeyVault);

            Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Success, "Secret copied!"));
            Mediator.NotifyEvent(this, new CopyEventArgs(targetKeyVault, KeyName));

            Lock();
        }
        catch (Exception ex)
        {
            Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Error, $"Failed to copy secret: {ex.Message}"));
        }
        finally
        {
            DoNothingIf(action);
        }
    }

    private async Task CopyToClipboardAsync()
    {
        try
        {
            IsDoing = Action.Copying;

            _latestEditorIdToCopy = _editorId;
            Mediator.NotifyEvent(this, new HighlightEventArgs(_editorId));

            Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Information, "Getting secret.."));

            var secretValue = await SecretsProvider.GetSecretValueAsync(KeyVaultName, KeyName);
            await JsRuntime.InvokeVoidAsync("clipboardCopy.copyText", secretValue);

            Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Success, "Secret copied to clipboard!"));
        }
        catch (Exception ex)
        {
            Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Error, $"Failed to get secret: {ex.Message}"));
        }
        finally
        {
            DoNothingIf(Action.Copying);
        }
    }

    private async Task DeleteAsync()
    {
        try
        {
            IsDoing = Action.Deleting;

            MemoryCache.Remove(CacheKey());

            if (await Mediator.NotifyEventAsync(this, new PaneRequestEventArgs(typeof(ConfirmPane), EditContext, new ButtonContext(default, KeyVaultName))) == CrudType.Delete)
            {
                Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Information, "Deleting secret.."));

                SetValue(null!);

                StateHasChanged();

                await SecretsProvider.DeleteSecretAsync(KeyVaultName, KeyName);

                Lock();

                Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Success, "Secret deleted!"));

                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Error, $"Failed to delete secret: {ex.Message}"));
        }
        finally
        {
            DoNothingIf(Action.Deleting);
        }
    }

    private string CacheKey() => $"{EditContext.Entity.Id}{KeyVaultName}";

    private void DoNothingIf(Action action)
    {
        if (IsDoing == action)
        {
            IsDoing = Action.Nothing;
        }
    }
}
