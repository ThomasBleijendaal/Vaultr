using RapidCMS.Core.Abstractions.Mediators;
using RapidCMS.Core.Enums;
using RapidCMS.Core.Models.EventArgs.Mediators;
using Vaultr.Client.Core.Abstractions;

namespace Vaultr.Client.Core.Providers;

internal class DangerModeProvider : IDangerModeProvider
{
    private readonly IMediator _mediator;

    public DangerModeProvider(IMediator mediator)
    {
        _mediator = mediator;
    }

    public bool IsEnabled { get; private set; }

    public void Disable()
    {
        IsEnabled = false;

        Notify();
    }

    public void Enable()
    {
        IsEnabled = true;

        Notify();
    }

    private void Notify()
    {
        _mediator.NotifyEvent(
            this,
            new CollectionRepositoryEventArgs(
                "vaultr::secrets",
                "vaultr::secrets",
                default,
                default(string),
                CrudType.Update));
    }
}
