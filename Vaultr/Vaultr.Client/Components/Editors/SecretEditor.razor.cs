using System;
using System.Linq;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Vaultr.Client.Components.Editors
{
    public partial class SecretEditor
    {
        private string _buttonId = "_" + Guid.NewGuid().ToString().Replace("-", "");
        private string _menuId = "_" + Guid.NewGuid().ToString().Replace("-", "");

        private string? Value { get; set; }
        private bool IsDecrypted { get; set; }
        private bool IsModified { get; set; }
        private bool IsEmpty => string.IsNullOrEmpty(GetValueAsString());
        private bool IsEncrypted => !IsEmpty && !IsDecrypted;

        private string KeyVaultName => Configuration as string ?? throw new InvalidOperationException("Missing keyvault name in Configuration");

        private SecretClient SecretClient => SecretClientsProvider.Clients[KeyVaultName];

        private string KeyName => new Uri(GetValueAsString()).AbsolutePath.Split('/').Last();

        [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JSRuntime.InvokeAsync<bool>("Interop.addPopper", _buttonId, _menuId);
            await base.OnAfterRenderAsync(firstRender);
        }

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

        private async Task UnlockAsync(MouseEventArgs e)
        {
            var secret = await SecretClient.GetSecretAsync(KeyName);

            IsDecrypted = true;
            Value = secret.Value.Value;

            StateHasChanged();
        }

        private void Lock(MouseEventArgs e)
        {
            Value = null;
            IsDecrypted = false;
            IsModified = false;

            StateHasChanged();
        }

        private async Task SaveAsync(MouseEventArgs e)
        {
            await SecretClient.SetSecretAsync(KeyName, Value);
            Lock(e);
        }

        private async Task CopyAsync(string targetKeyVault)
        {
            var secret = await SecretClient.GetSecretAsync(KeyName);

            var targetSecretClient = SecretClientsProvider.Clients[targetKeyVault];

            await targetSecretClient.SetSecretAsync(KeyName, secret.Value.Value);
        }
    }
}
