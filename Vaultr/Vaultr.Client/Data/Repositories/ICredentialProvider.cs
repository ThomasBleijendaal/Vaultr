using Azure.Core;

namespace Vaultr.Client.Data.Repositories;

public interface ICredentialProvider
{
    TokenCredential GetTokenCredential(string tenantId);
}

