namespace Vaultr.Client.Core;

public class NotificationService
{
    public event EventHandler<TitleEventArgs>? TitleChanged;

    public void UpdateTitle(string title)
    {
        TitleChanged?.Invoke(this, new TitleEventArgs(title));
    }
}
