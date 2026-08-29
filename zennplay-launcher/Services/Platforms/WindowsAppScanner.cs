using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Platforms
{
    public class WindowsAppScanner
    {
        public List<ScannedAppModel> GetInstalledApps()
        {
            var apps = new List<ScannedAppModel>();
            var registryTargets = new[]
            {
        (RegistryHive.LocalMachine, RegistryView.Registry64),
        (RegistryHive.LocalMachine, RegistryView.Registry32),
        (RegistryHive.CurrentUser, RegistryView.Default)
    };

            const string uninstallKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            var processedTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var (hive, view) in registryTargets)
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                using var subKey = baseKey.OpenSubKey(uninstallKeyPath);
                if (subKey == null) continue;

                foreach (var subKeyName in subKey.GetSubKeyNames())
                {
                    using var appKey = subKey.OpenSubKey(subKeyName);
                    if (appKey == null) continue;

                    // Abaikan update/patch sistem atau komponen tersembunyi
                    if ((int?)appKey.GetValue("SystemComponent") == 1) continue;
                    if (appKey.GetValue("ParentKeyName") != null) continue;

                    string? displayName = appKey.GetValue("DisplayName") as string;
                    if (string.IsNullOrWhiteSpace(displayName)) continue;

                    string? installLocation = appKey.GetValue("InstallLocation") as string;
                    string? displayIcon = appKey.GetValue("DisplayIcon") as string;
                    string? uninstallString = appKey.GetValue("UninstallString") as string;

                    string? cleanIconPath = displayIcon?.Split(',')[0].Trim('\"');
                    string targetExe = FindMainExecutable(installLocation, cleanIconPath);

                    if (string.IsNullOrWhiteSpace(targetExe) || !File.Exists(targetExe))
                        continue;

                    if (!processedTargets.Add(targetExe))
                        continue;

                    string appDir = !string.IsNullOrWhiteSpace(installLocation) && Directory.Exists(installLocation)
                        ? installLocation
                        : (Path.GetDirectoryName(targetExe) ?? string.Empty);

                    string? explicitIconSource = !string.IsNullOrWhiteSpace(cleanIconPath) && File.Exists(cleanIconPath)
                        ? cleanIconPath
                        : targetExe;

                    apps.Add(new ScannedAppModel
                    {
                        Title = displayName,
                        AppDir = appDir,
                        Target = targetExe,
                        IconSourcePath = explicitIconSource, // Simpan path icon spesifik
                        Icon = IconExtractor.ExtractFromWindowsExecutable(explicitIconSource),
                        IsSelected = false
                    });
                }
            }

            return apps;
        }

        private string FindMainExecutable(string? installDir, string? iconPath)
        {
            if (!string.IsNullOrWhiteSpace(iconPath) && iconPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && File.Exists(iconPath))
                return iconPath;

            if (!string.IsNullOrWhiteSpace(installDir) && Directory.Exists(installDir))
            {
                // Ambil exe pertama di folder root instalasi
                var exe = Directory.GetFiles(installDir, "*.exe", SearchOption.TopDirectoryOnly);
                if (exe.Length > 0) return exe[0];
            }

            return string.Empty;
        }
    }
}
