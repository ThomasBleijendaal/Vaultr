using Azure.Core;
using Azure.Identity;

namespace Vaultr.Client.Data.Repositories;

public class CredentialProvider : ICredentialProvider
{
    private readonly Dictionary<string, TokenCredential> _credentials = new();

    public TokenCredential GetTokenCredential(string tenantId)
    {
        {
            if (_credentials.TryGetValue(tenantId, out var value))
            {
                return value;
            }
        }

        lock (_credentials)
        {
            if (_credentials.TryGetValue(tenantId, out var value))
            {
                return value;
            }

            //#if MACCATALYST
            //            var credential = new AzureCliCredential(new AzureCliCredentialOptions
            //            {
            //                TenantId = tenantId
            //            });
            //#else

            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                TenantId = tenantId,
                ExcludeInteractiveBrowserCredential = false,
                ExcludeEnvironmentCredential = true,
                ExcludeManagedIdentityCredential = true,
                ExcludeWorkloadIdentityCredential = true,
                ExcludeBrokerCredential = true
            });

            //credential.GetTokenAsync()''.Authenticate();
            //#endif

            return _credentials[tenantId] = credential;
        }
    }
}

