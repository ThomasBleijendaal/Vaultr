using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Vaultr.Client.Core.Abstractions;

namespace Vaultr.Client.Core.Providers;

public class ClientSpecifiedAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly NotificationService _notificationService;
    private readonly IConfigurationStateProvider _configurationStateProvider;

    public ClientSpecifiedAuthenticationStateProvider(
        NotificationService notificationService,
        IConfigurationStateProvider configurationStateProvider)
    {
        _notificationService = notificationService;
        _configurationStateProvider = configurationStateProvider;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var state = _configurationStateProvider.GetCurrentState();
        if (state?.IsValid() != true)
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal()));
        }
        else
        {
            var id = new ClaimsIdentity("anonymous");
            id.AddClaim(new Claim(ClaimTypes.Name, "Anonymous"));

            var principal = new ClaimsPrincipal(id);

            _notificationService.UpdateTitle($"{state.Name ?? "Vaultr"} - {state.TenantId}");

            return Task.FromResult(new AuthenticationState(principal));
        }
    }
}
