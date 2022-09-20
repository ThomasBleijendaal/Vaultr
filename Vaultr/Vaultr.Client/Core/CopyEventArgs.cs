using RapidCMS.Core.Abstractions.Mediators;

namespace Vaultr.Client.Core;

internal record CopyEventArgs(string KeyVaultName, string KeyName) : IMediatorEventArgs;
