namespace Vaultr.Client.Core.Abstractions;

public interface IDangerModeProvider
{
    public bool IsEnabled { get; }

    public void Enable();
    public void Disable();
}
