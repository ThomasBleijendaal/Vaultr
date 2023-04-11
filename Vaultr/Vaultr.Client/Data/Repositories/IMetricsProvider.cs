using RapidCMS.Core.Abstractions.Mediators;
using Vaultr.Client.Core;
using Vaultr.Client.Data.Models;

namespace Vaultr.Client.Data.Repositories;

public interface IMetricsProvider
{
    KeyVaultSecretMetric? GetMetric(string keyVaultName, string secretName);

    internal void AcceptMediator(IMediator mediator);
    internal Task ReadMetricsAsync(object sender, StateChangedEventArgs args);
}
