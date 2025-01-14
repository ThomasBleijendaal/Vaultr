using Application = Microsoft.Maui.Controls.Application;

namespace Vaultr.Client;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage()) { Title = "Vaultr" };
    }
}
