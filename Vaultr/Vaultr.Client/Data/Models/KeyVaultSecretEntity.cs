using System;
using System.Collections.Generic;
using RapidCMS.Core.Abstractions.Data;

namespace Vaultr.Client.Data.Models;

public class KeyVaultSecretEntity : IEntity
{
    public string Id { get; set; }

    public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, Uri> KeyVaultUris { get; set; } = new Dictionary<string, Uri>();
}
