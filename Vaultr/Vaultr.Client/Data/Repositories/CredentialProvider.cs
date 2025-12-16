using Azure.Core;
using Azure.Identity;

namespace Vaultr.Client.Data.Repositories;

public class CredentialProvider : ICredentialProvider
{
    private readonly Dictionary<string, TokenCredential> _credentials = new();

    static CredentialProvider()
    {
#if MACCATALYST
        // Ensure Azure CLI is accessible on Mac
        EnsureAzureCliAccessible();
#endif
    }

    public TokenCredential GetTokenCredential(string tenantId)
    {
        if (_credentials.ContainsKey(tenantId))
        {
            return _credentials[tenantId];
        }

#if MACCATALYST
        // Use Azure CLI on Mac (requires 'az login')
        var credential = new AzureCliCredential(new AzureCliCredentialOptions
        {
            TenantId = tenantId
        });
#else
        // Use browser on Windows
        var credential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
        {
            TenantId = tenantId,
            TokenCachePersistenceOptions = new TokenCachePersistenceOptions
            {
                Name = "VaultR",
                UnsafeAllowUnencryptedStorage = false
            }
        });
#endif

        return _credentials[tenantId] = credential;
    }

#if MACCATALYST
    /// <summary>
    /// Ensures Azure CLI is accessible by adding its directory to PATH if needed.
    /// </summary>
    private static void EnsureAzureCliAccessible()
    {
        var currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        
        // Check common Azure CLI locations - only add if they exist and contain 'az'
        var candidatePaths = new[]
        {
            "/opt/homebrew/bin",     // Homebrew on Apple Silicon
            "/usr/local/bin",        // Homebrew on Intel
            "/opt/local/bin",        // MacPorts
        };

        var pathsToAdd = candidatePaths
            .Where(p => !currentPath.Contains(p))           // Not already in PATH
            .Where(p => Directory.Exists(p))                // Directory exists
            .Where(p => File.Exists(Path.Combine(p, "az"))) // Contains 'az' executable
            .ToList();
        
        if (pathsToAdd.Any())
        {
            Environment.SetEnvironmentVariable("PATH", $"{string.Join(":", pathsToAdd)}:{currentPath}");
        }
    }
#endif
}

