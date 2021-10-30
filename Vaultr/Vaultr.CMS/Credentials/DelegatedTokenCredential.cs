using Azure.Core;
using Blazored.SessionStorage;
using Newtonsoft.Json.Linq;

namespace Vaultr.CMS.Credentials;

public class DelegatedTokenCredential : TokenCredential
{
    private readonly ISyncSessionStorageService _syncSessionStorageService;

    public DelegatedTokenCredential(
        ISyncSessionStorageService syncSessionStorageService)
    {
        _syncSessionStorageService = syncSessionStorageService;
    }

    public string GetToken()
    {
        var length = _syncSessionStorageService.Length();

        for (var i = 0; i < length; i++)
        {
            var key = _syncSessionStorageService.Key(i);

            if (key.Contains("https://vault.azure.net/user_impersonation") && key.Contains("-accesstoken-"))
            {
                var data = _syncSessionStorageService.GetItemAsString(key);
                var jsonObject = JObject.Parse(data);

                return jsonObject.Value<string>("secret") ?? throw new UnauthorizedAccessException();
            }
        }

        throw new UnauthorizedAccessException();
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken) 
        => new AccessToken(GetToken(), DateTimeOffset.UtcNow.AddHours(1));

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken) 
        => new ValueTask<AccessToken>(Task.FromResult(GetToken(requestContext, cancellationToken)));
}
