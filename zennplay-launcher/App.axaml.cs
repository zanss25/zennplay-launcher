using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using zennplay_launcher.Models;
using zennplay_launcher.Services;
using zennplay_launcher.Services.Repositories;
using zennplay_launcher.ViewModels;
using zennplay_launcher.Views;


namespace zennplay_launcher;

public partial class App : Application
{
    public static UserConfigModel ActiveConfig = ConfigServices.LoadConfig();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(),
            };
        }

        var dbInitializer = new DatabaseInitializer();
        dbInitializer.Initialize();

        ConfigServices.CreateDefaultConfig();

        base.OnFrameworkInitializationCompleted();
    }
}