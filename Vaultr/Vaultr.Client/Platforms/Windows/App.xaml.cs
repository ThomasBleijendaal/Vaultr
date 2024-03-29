﻿using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Vaultr.Client.Core;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vaultr.Client.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
    private NotificationService? _notificationService = null;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    protected override MauiApp CreateMauiApp()
    {
        var app = MauiProgram.CreateMauiApp();

        _notificationService = app.Services.GetRequiredService<NotificationService>();

        return app;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        var currentWindow = Application.Windows[0].Handler?.PlatformView;

        var _windowHandle = WindowNative.GetWindowHandle(currentWindow);
        var windowId = Win32Interop.GetWindowIdFromWindow(_windowHandle);

        if (_notificationService == null)
        {
            return;
        }

        _notificationService.TitleChanged += (s, e) =>
        {
            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Title = e.Title;
        };

        _notificationService.UpdateTitle("Vaultr");
    }
}
