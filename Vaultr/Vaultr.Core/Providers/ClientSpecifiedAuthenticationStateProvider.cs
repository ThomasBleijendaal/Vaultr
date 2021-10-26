using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Vaultr.Core.Providers
{
    public class ClientSpecifiedAuthenticationStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal()));


            var id = new ClaimsIdentity("anonymous");
            id.AddClaim(new Claim(ClaimTypes.Name, "Anonymous"));

            var principal = new ClaimsPrincipal(id);

            return Task.FromResult(new AuthenticationState(principal));
        }
    }
}
