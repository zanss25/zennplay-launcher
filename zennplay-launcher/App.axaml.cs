using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using zennplay_launcher.Models;
using zennplay_launcher.Services;
using zennplay_launcher.Services.Platforms;
using zennplay_launcher.Services.Repositories;
using zennplay_launcher.ViewModels;
using zennplay_launcher.Views;


namespace zennplay_launcher;

public partial class App : Application
{
    public static UserConfigModel ActiveConfig = ConfigServices.LoadConfig();
    public static IAppPlatformService PlatformService { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        IdentifyOSPlatform();
    }

    private static void IdentifyOSPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            PlatformService = new WindowsAppService();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            PlatformService = new LinuxAppService();
        }
        else
        {
            throw new PlatformNotSupportedException("OS tidak didukung.");
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var dbInitializer = new DatabaseInitializer();
        dbInitializer.Initialize();

        ConfigServices.CreateDefaultConfig();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
