﻿using Application = Microsoft.Maui.Controls.Application;

namespace Vaultr.Client;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new MainPage();
    }
}
