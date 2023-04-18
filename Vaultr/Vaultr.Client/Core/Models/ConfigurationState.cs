namespace Vaultr.Client.Core.Models;

public class ConfigurationState
{
    public List<KeyVaultConfiguration> KeyVaults { get; set; } = new List<KeyVaultConfiguration>();

    public string? Name { get; set; }

    public string? TenantId { get; set; }

    public string? WorkspaceId { get; set; }

    public class KeyVaultConfiguration
    {
        public string Name { get; set; } = "";

        public string WorkspaceId { get; set; } = "";
    }

    public bool IsValid()
        => KeyVaults.Count > 0
            && KeyVaults.Any(x => !string.IsNullOrWhiteSpace(x.Name))
            && !string.IsNullOrEmpty(TenantId);

    public void SanitizeKeyVaults()
    {
        KeyVaults = KeyVaults
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .GroupBy(x => x.Name)
            .Select(x => x.First())
            .ToList();
    }
}
