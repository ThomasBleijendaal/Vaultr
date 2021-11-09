﻿using System.Collections.Generic;

namespace Vaultr.Client.Core.Models;

public class ConfigurationState
{
    public List<KeyVaultConfiguration> KeyVaults { get; set; } = new List<KeyVaultConfiguration> { /*new KeyVaultConfiguration()*/ };

    public string? TenantId { get; set; }

    public class KeyVaultConfiguration
    {
        public string Name { get; set; } = "";
    }

    public bool IsValid()
    {
        return KeyVaults.Count > 0 && !string.IsNullOrEmpty(TenantId);
    }
}
