namespace Vaultr.Client.Core.Models;

public class ConfigurationState
{
    public List<KeyVaultConfiguration> KeyVaults { get; set; } = new List<KeyVaultConfiguration>();

    public string KeyVaultsAsString
    {
        get => string.Join("\n", KeyVaults.Select(x => x.Name));
        set => KeyVaults = value
            .Split("\n")
            .Select(x => x.Replace("https://", "").Replace(".vault.azure.net", ""))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .Select(x => new KeyVaultConfiguration { Name = x })
            .ToList();
    }

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
