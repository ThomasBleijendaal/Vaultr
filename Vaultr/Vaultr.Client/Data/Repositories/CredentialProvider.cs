﻿using Azure.Core;
using Azure.Identity;

namespace Vaultr.Client.Data.Repositories;

public class CredentialProvider : ICredentialProvider
{
    private readonly Dictionary<string, TokenCredential> _credentials = new();

    public TokenCredential GetTokenCredential(string tenantId)
    {
        if (_credentials.ContainsKey(tenantId))
        {
            return _credentials[tenantId];
        }

#if MACCATALYST
        var credential = new AzureCliCredential(new AzureCliCredentialOptions
        {
            TenantId = tenantId
        });
#else
        var credential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
        {
            TenantId = tenantId,
            TokenCachePersistenceOptions = new TokenCachePersistenceOptions
            {
                Name = "VaultR",
                UnsafeAllowUnencryptedStorage = false
            }
        });

        credential.Authenticate();
#endif

        return _credentials[tenantId] = credential;
    }
}

