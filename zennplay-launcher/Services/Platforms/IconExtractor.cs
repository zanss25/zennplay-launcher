using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace zennplay_launcher.Services.Platforms
{
    public static class IconExtractor
    {
        [SupportedOSPlatform("windows")]
        public static Bitmap? ExtractFromWindowsExecutable(string exeOrIcoPath)
        {
            if (string.IsNullOrWhiteSpace(exeOrIcoPath) || !File.Exists(exeOrIcoPath))
                return null;

            try
            {
                if (OperatingSystem.IsWindows())
                {
                    // Ekstrak icon bawaan .exe atau file .ico menggunakan System.Drawing.Common
                    using var sysIcon = Icon.ExtractAssociatedIcon(exeOrIcoPath);
                    if (sysIcon == null) return null;

                    using var bitmap = sysIcon.ToBitmap();
                    using var memoryStream = new MemoryStream();
                    bitmap.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Position = 0;

                    // Konversi ke Avalonia Bitmap
                    return new Bitmap(memoryStream);
                }
            }
            catch
            {
                // Fallback jika file terproteksi atau korup
            }

            return null;
        }

        [SupportedOSPlatform("linux")]
        public static Bitmap? LoadLinuxIcon(string? iconNameOrPath)
        {
            if (string.IsNullOrWhiteSpace(iconNameOrPath))
                return null;

            try
            {
                // 1. Jika icon sudah berupa absolute path (misal /usr/share/pixmaps/game.png)
                if (File.Exists(iconNameOrPath))
                {
                    return new Bitmap(iconNameOrPath);
                }

                // 2. Jika icon berupa nama tema (misal "steam", "firefox"), cari di folder icon standar
                string[] searchDirs =
                {
                    "/usr/share/pixmaps",
                    "/usr/share/icons/hicolor/256x256/apps",
                    "/usr/share/icons/hicolor/128x128/apps",
                    "/usr/share/icons/hicolor/64x64/apps",
                    "/usr/share/icons/hicolor/48x48/apps",
                    "/usr/share/icons/hicolor/scalable/apps",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/icons")
                };

                string[] extensions = { ".png", ".xpm", ".svg" };

                foreach (var dir in searchDirs)
                {
                    if (!Directory.Exists(dir)) continue;

                    foreach (var ext in extensions)
                    {
                        string testPath = Path.Combine(dir, iconNameOrPath + ext);
                        if (File.Exists(testPath) && ext != ".svg") // Avalonia Bitmap bawaan langsung membaca format raster (png/xpm)
                        {
                            return new Bitmap(testPath);
                        }
                    }
                }
            }
            catch
            {
                // Fallback jika tidak ditemukan
            }

            return null;
        }

        /// <summary>
        /// Mengekstrak icon dari target executable dan menyimpannya sebagai file PNG di storage aset aplikasi.
        /// </summary>
        public static string? ExtractAndSaveIconToStorage(Guid appId, string sourceExecutablePath, string assetDirectory)
        {
            if (appId == Guid.Empty || string.IsNullOrWhiteSpace(sourceExecutablePath) || !File.Exists(sourceExecutablePath))
                return null;

            try
            {
                // 1. Pastikan folder data aset game tujuan tersedia
                if (!Directory.Exists(assetDirectory))
                {
                    Directory.CreateDirectory(assetDirectory);
                }

                string destinationPath = Path.Combine(assetDirectory, $"{Guid.NewGuid()}.png");

                // 2. Ekstraksi untuk Windows (.exe / .ico)
                if (OperatingSystem.IsWindows())
                {
                    using var sysIcon = Icon.ExtractAssociatedIcon(sourceExecutablePath);
                    if (sysIcon != null)
                    {
                        using var bmp = sysIcon.ToBitmap();
                        bmp.Save(destinationPath, ImageFormat.Png);
                        return destinationPath;
                    }
                }
                // 3. Fallback jika file sumber sudah berupa gambar (.png/.ico/.jpg)
                else if (File.Exists(sourceExecutablePath))
                {
                    string ext = Path.GetExtension(sourceExecutablePath).ToLowerInvariant();
                    if (ext is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".webp")
                    {
                        File.Copy(sourceExecutablePath, destinationPath, overwrite: true);
                        return destinationPath;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to extract and stage app icon: {ex.Message}");
            }

            return null;
        }

    }
}
