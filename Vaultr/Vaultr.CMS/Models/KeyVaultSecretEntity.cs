using RapidCMS.Core.Abstractions.Data;

namespace Vaultr.CMS.Models;

public class KeyVaultSecretEntity : IEntity
{
    public string? Id { get; set; }

    public List<string> Values { get; set; } = new List<string>();
}
