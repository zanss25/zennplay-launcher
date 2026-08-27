using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using zennplay_launcher.Models;
using zennplay_launcher.Services;

namespace zennplay_launcher.ViewModels.Controls;

public partial class PropertiesMediaViewModel : ViewModelBase
{
    // ============================================================
    // 0. APP IDENTITY & SNAPSHOT ORIGINAL STATE
    // ============================================================
    [ObservableProperty] private Guid _appId = Guid.Empty;

    private string _initialIconPath = string.Empty;
    private string _initialCoverPath = string.Empty;
    private string _initialBackgroundPath = string.Empty;

    // Menampung semua file baru yang dicopy selama sesi edit ini
    private readonly HashSet<string> _pendingCreatedAssets = new(StringComparer.OrdinalIgnoreCase);

    // ============================================================
    // 1. RAW PATH PROPERTIES (Staged in UserAssets)
    // ============================================================
    [ObservableProperty] private string _iconPath = string.Empty;
    [ObservableProperty] private string _coverPath = string.Empty;
    [ObservableProperty] private string _backgroundPath = string.Empty;

    // ============================================================
    // 2. BITMAP PREVIEW PROPERTIES (UI Display)
    // ============================================================
    [ObservableProperty] private Bitmap? _iconBitmap;
    [ObservableProperty] private Bitmap? _coverBitmap;
    [ObservableProperty] private Bitmap? _backgroundBitmap;

    // ============================================================
    // 3. STORAGE PROVIDER FOR BROWSE
    // ============================================================
    public IStorageProvider? StorageProvider { get; set; }

    // ============================================================
    // LIFECYCLE & SYNC METHODS
    // ============================================================
    public void LoadFromModel(AppModel app)
    {
        AppId = app.Id;

        // Snapshot path awal dari database
        _initialIconPath = app.IconPath ?? string.Empty;
        _initialCoverPath = app.CoverPath ?? string.Empty;
        _initialBackgroundPath = app.BackgroundPath ?? string.Empty;

        // Reset tracking draft
        _pendingCreatedAssets.Clear();

        IconPath = _initialIconPath;
        CoverPath = _initialCoverPath;
        BackgroundPath = _initialBackgroundPath;

        IconBitmap = LoadBitmapSafe(IconPath);
        CoverBitmap = LoadBitmapSafe(CoverPath);
        BackgroundBitmap = LoadBitmapSafe(BackgroundPath);
    }

    public void ApplyToModel(AppModel app)
    {
        app.IconPath = IconPath;
        app.CoverPath = CoverPath;
        app.BackgroundPath = BackgroundPath;
    }

    // ============================================================
    // TRANSACTION: COMMIT & ROLLBACK
    // ============================================================
    /// <summary>
    /// Dipanggil saat user menekan SAVE. Menghapus file fisik aset lama yang sudah diganti/dihapus.
    /// </summary>
    public void CommitChanges()
    {
        // 1. Hapus icon lama jika berubah/direset
        if (!string.IsNullOrWhiteSpace(_initialIconPath) && !string.Equals(_initialIconPath, IconPath, StringComparison.OrdinalIgnoreCase))
        {
            DeleteAssetSafe(_initialIconPath);
        }

        // 2. Hapus cover lama jika berubah/direset
        if (!string.IsNullOrWhiteSpace(_initialCoverPath) && !string.Equals(_initialCoverPath, CoverPath, StringComparison.OrdinalIgnoreCase))
        {
            DeleteAssetSafe(_initialCoverPath);
        }

        // 3. Hapus background lama jika berubah/direset
        if (!string.IsNullOrWhiteSpace(_initialBackgroundPath) && !string.Equals(_initialBackgroundPath, BackgroundPath, StringComparison.OrdinalIgnoreCase))
        {
            DeleteAssetSafe(_initialBackgroundPath);
        }

        // Semua file baru sekarang resmi disimpan permanen
        _pendingCreatedAssets.Clear();
    }

    /// <summary>
    /// Dipanggil saat CANCEL atau window ditutup tanpa save. Menghapus semua file baru yang sempat dicopy.
    /// </summary>
    public void RollbackChanges()
    {
        foreach (var newAssetPath in _pendingCreatedAssets)
        {
            DeleteAssetSafe(newAssetPath);
        }
        _pendingCreatedAssets.Clear();
    }

    // ============================================================
    // BROWSE COMMANDS (Staging copy)
    // ============================================================
    [RelayCommand]
    private async Task BrowseIconAsync()
    {
        var sourcePath = await BrowseImageFileAsync("Select Icon Image");
        if (!string.IsNullOrEmpty(sourcePath))
        {
            var savedPath = StageAssetToUserStorage(sourcePath);
            if (!string.IsNullOrEmpty(savedPath))
            {
                IconPath = savedPath;
                IconBitmap?.Dispose();
                IconBitmap = LoadBitmapSafe(savedPath);
            }
        }
    }

    [RelayCommand]
    private async Task BrowseCoverAsync()
    {
        var sourcePath = await BrowseImageFileAsync("Select Cover Image");
        if (!string.IsNullOrEmpty(sourcePath))
        {
            var savedPath = StageAssetToUserStorage(sourcePath);
            if (!string.IsNullOrEmpty(savedPath))
            {
                CoverPath = savedPath;
                CoverBitmap?.Dispose();
                CoverBitmap = LoadBitmapSafe(savedPath);
            }
        }
    }

    [RelayCommand]
    private async Task BrowseBackgroundAsync()
    {
        var sourcePath = await BrowseImageFileAsync("Select Background Image");
        if (!string.IsNullOrEmpty(sourcePath))
        {
            var savedPath = StageAssetToUserStorage(sourcePath);
            if (!string.IsNullOrEmpty(savedPath))
            {
                BackgroundPath = savedPath;
                BackgroundBitmap?.Dispose();
                BackgroundBitmap = LoadBitmapSafe(savedPath);
            }
        }
    }

    // ============================================================
    // REMOVE COMMANDS (Hanya hapus display UI, file fisik dihapus saat Save)
    // ============================================================
    [RelayCommand]
    public void RemoveIcon()
    {
        IconPath = string.Empty;
        IconBitmap?.Dispose();
        IconBitmap = null;
    }

    [RelayCommand]
    public void RemoveCover()
    {
        CoverPath = string.Empty;
        CoverBitmap?.Dispose();
        CoverBitmap = null;
    }

    [RelayCommand]
    public void RemoveBackground()
    {
        BackgroundPath = string.Empty;
        BackgroundBitmap?.Dispose();
        BackgroundBitmap = null;
    }

    [RelayCommand]
    public void ResetAllMedia()
    {
        RemoveIcon();
        RemoveCover();
        RemoveBackground();
    }

    // ============================================================
    // ASSET MANAGEMENT HELPERS
    // ============================================================
    private string? StageAssetToUserStorage(string sourceFilePath)
    {
        if (AppId == Guid.Empty || !File.Exists(sourceFilePath))
            return null;

        try
        {
            string appAssetsDir = Path.Combine(DatabaseHelper.GetAssetsDataPath(), AppId.ToString());
            if (!Directory.Exists(appAssetsDir))
            {
                Directory.CreateDirectory(appAssetsDir);
            }

            string extension = Path.GetExtension(sourceFilePath).ToLowerInvariant();
            string newFileName = $"{Guid.NewGuid()}{extension}";
            string destinationPath = Path.Combine(appAssetsDir, newFileName);

            File.Copy(sourceFilePath, destinationPath, overwrite: true);

            // Catat agar jika user cancel, file ini dibersihkan
            _pendingCreatedAssets.Add(destinationPath);

            return destinationPath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to stage asset: {ex.Message}");
            return null;
        }
    }

    private void DeleteAssetSafe(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || AppId == Guid.Empty)
            return;

        try
        {
            string appAssetsDir = Path.Combine(DatabaseHelper.GetAssetsDataPath(), AppId.ToString());
            string fullPath = Path.GetFullPath(path);
            string expectedPrefix = Path.GetFullPath(appAssetsDir);

            if (fullPath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to delete asset ({path}): {ex.Message}");
        }
    }

    private async Task<string?> BrowseImageFileAsync(string title)
    {
        if (StorageProvider == null) return null;

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Image Files")
                {
                    Patterns = ["*.png", "*.jpg", "*.jpeg", "*.webp", "*.bmp", "*.ico"]
                }
            ]
        });

        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }

    private static Bitmap? LoadBitmapSafe(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;

        try
        {
            using var stream = File.OpenRead(path);
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }
}