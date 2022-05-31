namespace Vaultr.Client.Core
{
    public class TitleEventArgs : EventArgs
    {
        public TitleEventArgs(string title)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
        }

        public string Title { get; }
    }
}
