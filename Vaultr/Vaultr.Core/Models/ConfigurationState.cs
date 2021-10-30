namespace Vaultr.Core.Models
{
    public class ConfigurationState
    {
        public List<KeyVaultConfiguration> KeyVaults { get; set; } = new List<KeyVaultConfiguration> { new KeyVaultConfiguration() };

        public class KeyVaultConfiguration
        {
            public string Name { get; set; } = "";
        }

        public bool IsValid()
        {
            return KeyVaults.Count > 0;
        }
    }
}
