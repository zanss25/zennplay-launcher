using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace zennplay_launcher.Services
{
    public static class DirectoryService
    {
        public static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n2} {suffixes[counter]}";
        }

        public static async Task<long> GetDirectorySizeAsync(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                return 0;

            return await Task.Run(() =>
            {
                try
                {
                    var dirInfo = new DirectoryInfo(folderPath);
                    // Hitung total semua file di dalam folder & subfolder
                    return dirInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                                  .Sum(file => file.Length);
                }
                catch
                {
                    // Tangani jika ada file yang terkunci (locked) atau akses ditolak (access denied)
                    return 0;
                }
            });
        }

        public static void OpenDirectory(string targetPath)
        {
            if (targetPath == null) return;

            string? folderPath = File.Exists(targetPath)
                ? Path.GetDirectoryName(targetPath)
                : targetPath;

            if (folderPath == null) return;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = folderPath,
                    UseShellExecute = true
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "xdg-open",
                    ArgumentList = { folderPath },
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }


        }

    }
}
