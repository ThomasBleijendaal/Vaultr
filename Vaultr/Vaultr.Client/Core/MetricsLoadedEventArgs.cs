using RapidCMS.Core.Abstractions.Mediators;

namespace Vaultr.Client.Core;

internal record MetricsLoadedEventArgs(string KeyVaultName) : IMediatorEventArgs;
