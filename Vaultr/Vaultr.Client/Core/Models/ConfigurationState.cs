using System.Collections.Generic;
using System.Linq;

namespace Vaultr.Client.Core.Models;

public class ConfigurationState
{
    public List<KeyVaultConfiguration> KeyVaults { get; set; } = new List<KeyVaultConfiguration>();

    public string? Name { get; set; }

    public string? TenantId { get; set; }

    public class KeyVaultConfiguration
    {
        public string Name { get; set; } = "";
    }

    public bool IsValid()
    {
        return KeyVaults.Count > 0 
            && KeyVaults.All(x => !string.IsNullOrWhiteSpace(x.Name)) 
            && !string.IsNullOrEmpty(TenantId);
    }

    public void SanitizeKeyVaults()
    {
        KeyVaults = KeyVaults
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .GroupBy(x => x.Name)
            .Select(x => x.First())
            .ToList();
    }
}
