using Azure.Core;
using Azure.Identity;
using RapidCMS.Core.Abstractions.Mediators;
using RapidCMS.Core.Enums;
using RapidCMS.Core.Models.EventArgs.Mediators;

namespace Vaultr.Client.Data.Repositories;

public class CredentialProvider : ICredentialProvider
{
    private readonly Dictionary<string, TokenCredential> _credentials = new();
    private readonly IMediator _mediator;

    public CredentialProvider(
        IMediator mediator)
    {
        _mediator = mediator;
    }

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

            var currentDevice = DeviceInfo.Current;

            var isWindows = currentDevice.Platform == DevicePlatform.WinUI;

            if (!isWindows)
            {
                EnsureAzureCliAccessible();

                var credential = new AzureCliCredential(new AzureCliCredentialOptions
                {
                    TenantId = tenantId
                });

                return _credentials[tenantId] = credential;
            }
            else
            {
                try
                {
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

                    return _credentials[tenantId] = credential;
                }
                catch (Exception ex)
                {
                    _mediator.NotifyEvent(this, new MessageEventArgs(MessageType.Error, $"Failed to authenticate: '{ex.Message}' -- falling back to AZ CLI"));

                    return _credentials[tenantId] = new AzureCliCredential(new AzureCliCredentialOptions
                    {
                        TenantId = tenantId
                    });
                }
            }
        }
    }

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
}

