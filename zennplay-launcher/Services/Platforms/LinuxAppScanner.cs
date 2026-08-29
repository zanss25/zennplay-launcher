using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Platforms
{
    public class LinuxAppScanner
    {
        public List<ScannedAppModel> GetInstalledApps()
        {
            var apps = new List<ScannedAppModel>();
            var searchPaths = new List<string>
    {
        "/usr/share/applications",
        "/usr/local/share/applications",
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/applications"),
        "/var/lib/flatpak/exports/share/applications",
        "/var/lib/snapd/desktop/applications"
    };

            var processedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var dir in searchPaths)
            {
                if (!Directory.Exists(dir)) continue;

                foreach (var filePath in Directory.GetFiles(dir, "*.desktop", SearchOption.TopDirectoryOnly))
                {
                    string fileName = Path.GetFileName(filePath);
                    if (processedFiles.Contains(fileName)) continue;

                    var app = ParseDesktopFile(filePath);

                    // 🔑 GUARD: Hanya tambahkan jika model valid dan target terisi
                    if (app != null && !string.IsNullOrWhiteSpace(app.Target))
                    {
                        apps.Add(app);
                        processedFiles.Add(fileName);
                    }
                }
            }

            return apps;
        }

        private ScannedAppModel? ParseDesktopFile(string path)
        {
            try
            {
                var lines = File.ReadAllLines(path);
                string? name = null;
                string? exec = null;
                string? icon = null;
                bool isNoDisplay = false;
                bool isApp = false;

                foreach (var rawLine in lines)
                {
                    var line = rawLine.Trim();
                    if (line.StartsWith("Type=Application", StringComparison.OrdinalIgnoreCase))
                        isApp = true;
                    else if (line.StartsWith("Name=", StringComparison.OrdinalIgnoreCase) && name == null)
                        name = line.Substring(5).Trim();
                    else if (line.StartsWith("Exec=", StringComparison.OrdinalIgnoreCase) && exec == null)
                    {
                        exec = line.Substring(5).Trim();
                        // Hapus parameter flag FreeDesktop seperti %u, %F, %f, dll.
                        exec = System.Text.RegularExpressions.Regex.Replace(exec, @"%[a-zA-Z]", "").Trim();
                    }
                    else if (line.StartsWith("Icon=", StringComparison.OrdinalIgnoreCase) && icon == null)
                        icon = line.Substring(5).Trim();
                    else if (line.StartsWith("NoDisplay=true", StringComparison.OrdinalIgnoreCase))
                        isNoDisplay = true;
                }

                // Saring entry yang bukan aplikasi, di-hide sistem, atau tidak punya nama/target eksekusi
                if (!isApp || isNoDisplay || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(exec))
                    return null;

                return new ScannedAppModel
                {
                    Title = name,
                    Target = exec,
                    AppDir = Path.GetDirectoryName(path) ?? string.Empty,
                    Icon = IconExtractor.LoadLinuxIcon(icon),
                    IsSelected = false
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
