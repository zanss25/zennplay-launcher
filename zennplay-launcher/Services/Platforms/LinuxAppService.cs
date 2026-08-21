using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace zennplay_launcher.Services.Platforms;

public class LinuxAppService : IAppPlatformService
{
    public IReadOnlyList<FilePickerFileType> GetExecutableFileFilters() =>
        new List<FilePickerFileType>
        {
            new("Linux Applications (*.AppImage, *.sh, *.desktop, Binaries)")
            {
                Patterns = new[] { "*.AppImage", "*.sh", "*.desktop", "*.bin" }
            },
            FilePickerFileTypes.All
        };

    public bool IsExecutable(string targetPath)
    {
        if (string.IsNullOrWhiteSpace(targetPath) || !File.Exists(targetPath))
            return false;

        string ext = Path.GetExtension(targetPath).ToLowerInvariant();
        if (ext is ".appimage" or ".sh" or ".desktop" or ".bin")
            return true;

        var mode = File.GetUnixFileMode(targetPath);
        return mode.HasFlag(UnixFileMode.UserExecute) ||
               mode.HasFlag(UnixFileMode.GroupExecute) ||
               mode.HasFlag(UnixFileMode.OtherExecute);
    }

    public (string Title, string Directory) ResolveAppMetadata(string targetPath)
    {
        string title = Path.GetFileNameWithoutExtension(targetPath);
        string directory = Path.GetDirectoryName(targetPath) ?? string.Empty;
        return (title, directory);
    }

    public Task LaunchAppAsync(string targetPath, string workingDir, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = targetPath,
            WorkingDirectory = string.IsNullOrWhiteSpace(workingDir) ? Path.GetDirectoryName(targetPath) : workingDir,
            Arguments = arguments,
            UseShellExecute = false
        };

        Process.Start(startInfo);
        return Task.CompletedTask;
    }
}