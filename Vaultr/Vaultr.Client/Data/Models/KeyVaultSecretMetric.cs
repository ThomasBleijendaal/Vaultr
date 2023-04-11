namespace Vaultr.Client.Data.Models;

public class KeyVaultSecretMetric
{
    public string KeyVaultName { get; set; } = null!;

    public string SecretName { get; set; } = null!;

    public long Reads { get; set; }
}
