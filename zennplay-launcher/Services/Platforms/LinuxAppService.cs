using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace zennplay_launcher.Services.Platforms;

public class LinuxAppService : IAppPlatformService
{
    private static readonly HashSet<string> SubfolderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin", "binaries", "x86_64", "x86", "release", "shipping"
    };

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

    public (string Title, string ResolvedTargetPath, string Directory) ResolveAppMetadata(string targetPath)
    {
        string finalPath = targetPath;
        string workingDir = string.Empty;
        string title = Path.GetFileNameWithoutExtension(targetPath);

        if (Path.GetExtension(targetPath).Equals(".desktop", StringComparison.OrdinalIgnoreCase))
        {
            var desktopInfo = ParseDesktopFile(targetPath);
            if (!string.IsNullOrWhiteSpace(desktopInfo.ExecPath))
                finalPath = desktopInfo.ExecPath;
            if (!string.IsNullOrWhiteSpace(desktopInfo.WorkingDirectory))
                workingDir = desktopInfo.WorkingDirectory;
            if (!string.IsNullOrWhiteSpace(desktopInfo.Name))
                title = desktopInfo.Name;
        }

        if (string.IsNullOrWhiteSpace(workingDir))
        {
            workingDir = Path.GetDirectoryName(finalPath) ?? string.Empty;
        }

        return (title, finalPath, workingDir);
    }

    public string DetectRootGameDirectory(string resolvedTargetPath, string defaultDir)
    {
        if (string.IsNullOrWhiteSpace(resolvedTargetPath))
            return defaultDir;

        var currentDirInfo = new DirectoryInfo(Path.GetDirectoryName(resolvedTargetPath) ?? defaultDir);

        while (currentDirInfo != null && currentDirInfo.Parent != null)
        {
            if (SubfolderNames.Contains(currentDirInfo.Name))
            {
                currentDirInfo = currentDirInfo.Parent;
            }
            else
            {
                break;
            }
        }

        return currentDirInfo?.FullName ?? defaultDir;
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

    private static (string Name, string ExecPath, string WorkingDirectory) ParseDesktopFile(string path)
    {
        string name = string.Empty;
        string exec = string.Empty;
        string workingDir = string.Empty;

        try
        {
            foreach (var line in File.ReadLines(path))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("Name=", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(name))
                {
                    name = trimmed[5..];
                }
                else if (trimmed.StartsWith("Exec=", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(exec))
                {
                    string rawExec = trimmed[5..];
                    // Bersihkan argument spesifikasi freedesktop (seperti %u, %f, %F)
                    exec = rawExec.Split('%')[0].Trim().Trim('"');
                }
                else if (trimmed.StartsWith("Path=", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(workingDir))
                {
                    workingDir = trimmed[5..].Trim().Trim('"');
                }
            }
        }
        catch { }

        return (name, exec, workingDir);
    }
}