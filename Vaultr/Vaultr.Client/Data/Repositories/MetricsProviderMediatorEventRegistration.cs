using RapidCMS.Core.Abstractions.Mediators;
using Vaultr.Client.Core;

namespace Vaultr.Client.Data.Repositories;

internal class MetricsProviderMediatorEventRegistration : IMediatorEventListener
{
    private readonly IMetricsProvider _metricsProvider;
    private IDisposable? _registration;

    public MetricsProviderMediatorEventRegistration(IMetricsProvider metricsProvider)
    {
        _metricsProvider = metricsProvider;
    }

    public void RegisterListener(IMediator mediator)
    {
        _registration = mediator.RegisterCallback<StateChangedEventArgs>(_metricsProvider.ReadMetricsAsync);
        _metricsProvider.AcceptMediator(mediator);
    }

    public void Dispose()
    {
        _registration?.Dispose();
        _registration = null;
    }
}
