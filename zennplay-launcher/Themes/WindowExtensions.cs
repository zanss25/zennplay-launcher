using Avalonia.Controls;
using System;

namespace zennplay_launcher.Themes
{
    public static class WindowExtensions
    {
        public static void ApplyPlatformDecorations(this Window window)
        {
            if (OperatingSystem.IsWindows())
            {
                window.ExtendClientAreaToDecorationsHint = true;
                window.ExtendClientAreaTitleBarHeightHint = -1;
                window.WindowDecorations = WindowDecorations.Full;
            }
            else if (OperatingSystem.IsLinux())
            {
                window.ExtendClientAreaToDecorationsHint = false;
                window.WindowDecorations = WindowDecorations.None;
                window.Classes.Add("LinuxWindow");
            }
        }
    }
}