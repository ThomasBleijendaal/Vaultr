using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Vaultr.Core.Abstractions;

namespace Vaultr.Core.Providers
{
    public class ClientSpecifiedAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IConfigurationStateProvider _configurationStateProvider;

        public ClientSpecifiedAuthenticationStateProvider(
            IConfigurationStateProvider configurationStateProvider)
        {
            _configurationStateProvider = configurationStateProvider;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var state = _configurationStateProvider.GetCurrentState();
            if (state == null)
            {
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal()));
            }
            else
            {
                var id = new ClaimsIdentity("anonymous");
                id.AddClaim(new Claim(ClaimTypes.Name, "Anonymous"));

                var principal = new ClaimsPrincipal(id);

                return Task.FromResult(new AuthenticationState(principal));
            }
        }
    }
}
