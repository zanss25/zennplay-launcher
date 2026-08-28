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

    private static readonly HashSet<string> SubfolderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin", "binaries", "win64", "win32", "x64", "x86", "release", "retail", "shipping"
    };

    public IReadOnlyList<FilePickerFileType> GetExecutableFileFilters() =>
        new List<FilePickerFileType>
        {
            new("Windows Executables & Shortcuts (*.exe, *.lnk, *.bat, *.url)")
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

    public (string Title, string ResolvedTargetPath, string Directory) ResolveAppMetadata(string targetPath)
    {
        string finalPath = targetPath;
        string workingDir = string.Empty;
        string title = Path.GetFileNameWithoutExtension(targetPath);

        string ext = Path.GetExtension(targetPath).ToLowerInvariant();

        // 1. Resolusi Windows .lnk Shortcut
        if (ext == ".lnk")
        {
            var (resolvedPath, lnkWorkingDir) = ResolveLnkShortcut(targetPath);
            if (!string.IsNullOrWhiteSpace(resolvedPath) && File.Exists(resolvedPath))
            {
                finalPath = resolvedPath;
                workingDir = lnkWorkingDir;
            }
        }
        // 2. Resolusi Internet Shortcut .url
        else if (ext == ".url")
        {
            string? urlTarget = ResolveUrlShortcut(targetPath);
            if (!string.IsNullOrWhiteSpace(urlTarget) && File.Exists(urlTarget))
            {
                finalPath = urlTarget;
            }
        }

        // 3. Fallback Directory jika shortcut tidak mendefinisikan working directory
        if (string.IsNullOrWhiteSpace(workingDir))
        {
            workingDir = Path.GetDirectoryName(finalPath) ?? string.Empty;
        }

        // 4. Ekstrak nama asli game dari FileVersionInfo jika format .exe
        if (Path.GetExtension(finalPath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(finalPath);
                if (!string.IsNullOrWhiteSpace(versionInfo.FileDescription))
                    title = versionInfo.FileDescription;
                else if (!string.IsNullOrWhiteSpace(versionInfo.ProductName))
                    title = versionInfo.ProductName;
            }
            catch
            {
                // Fallback tetap menggunakan nama file
            }
        }

        return (title, finalPath, workingDir);
    }

    public string DetectRootAppDirectory(string resolvedTargetPath, string defaultDir)
    {
        if (string.IsNullOrWhiteSpace(resolvedTargetPath))
            return defaultDir;

        var currentDirInfo = new DirectoryInfo(Path.GetDirectoryName(resolvedTargetPath) ?? defaultDir);

        // Telusuri parent folder jika executable berada di dalam folder build/binaries (misal: Binaries/Win64)
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
            UseShellExecute = true
        };

        Process.Start(startInfo);
        return Task.CompletedTask;
    }

    private static (string TargetPath, string WorkingDir) ResolveLnkShortcut(string lnkPath)
    {
        try
        {
            Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null)
                return (string.Empty, string.Empty);

            dynamic? shell = Activator.CreateInstance(shellType);
            if (shell == null)
                return (string.Empty, string.Empty);

            dynamic shortcut = shell.CreateShortcut(lnkPath);
            string target = (string)shortcut.TargetPath;
            string workingDir = (string)shortcut.WorkingDirectory;

            return (target, workingDir);
        }
        catch
        {
            return (string.Empty, string.Empty);
        }
    }

    private static string? ResolveUrlShortcut(string urlPath)
    {
        try
        {
            foreach (var line in File.ReadLines(urlPath))
            {
                if (line.StartsWith("URL=", StringComparison.OrdinalIgnoreCase))
                {
                    string target = line[4..].Trim();
                    if (target.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
                    {
                        return Uri.UnescapeDataString(target[8..].Replace('/', '\\'));
                    }
                    return target;
                }
            }
        }
        catch { }
        return null;
    }
}