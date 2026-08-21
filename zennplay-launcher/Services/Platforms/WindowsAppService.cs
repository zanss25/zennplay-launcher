using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace zennplay_launcher.Services.Platforms;

public class WindowsAppService : IAppPlatformService
{
    private static readonly HashSet<string> ExeExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".bat", ".cmd", ".lnk", ".url"
    };

    public IReadOnlyList<FilePickerFileType> GetExecutableFileFilters() =>
        new List<FilePickerFileType>
        {
            new("Windows Executables & Shortcuts (*.exe, *.lnk, *.bat)")
            {
                Patterns = new[] { "*.exe", "*.bat", "*.cmd", "*.lnk", "*.url" }
            },
            FilePickerFileTypes.All
        };

    public bool IsExecutable(string targetPath)
    {
        if (string.IsNullOrWhiteSpace(targetPath) || !File.Exists(targetPath))
            return false;

        return ExeExtensions.Contains(Path.GetExtension(targetPath));
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
            UseShellExecute = true
        };

        Process.Start(startInfo);
        return Task.CompletedTask;
    }
}