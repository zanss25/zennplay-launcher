using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Models;
using zennplay_launcher.Services;

namespace zennplay_launcher.ViewModels
{
    public partial class NewAppSelectionViewModel : ViewModelBase
    {
        private readonly AppEditorService _appEditorService = new();
        public IStorageProvider? StorageProvider { get; set; }

        // 🔑 Section ID yang ditargetkan
        public Guid? TargetSectionId { get; set; }

        public event Action<Guid?>? RequestClose;

        public ObservableCollection<ScannedAppModel> ScannedApps { get; } = [];

        public NewAppSelectionViewModel()
        {
            LoadDummyScannedApps();
        }

        private void LoadDummyScannedApps()
        {
            // Path icon kustom
            string iconPath = @"D:\Games\_MODS_\_ICO_\mw.png";
            Bitmap? dummyIcon = null;

            // Guard clause jika file ada di disk
            if (File.Exists(iconPath))
            {
                try
                {
                    dummyIcon = new Bitmap(iconPath);
                }
                catch
                {
                    dummyIcon = null;
                }
            }

            ScannedApps.Add(new ScannedAppModel
            {
                IsSelected = true,
                Icon = dummyIcon,
                Title = "Need for Speed: Most Wanted (2005)",
                Target = @"D:\Games\NFS Most Wanted\speed.exe",
                AppDir = @"D:\Games\NFS Most Wanted"
            });

            ScannedApps.Add(new ScannedAppModel
            {
                IsSelected = false,
                Icon = dummyIcon,
                Title = "Cyberpunk 2077",
                Target = @"D:\Games\Cyberpunk 2077\bin\x64\Cyberpunk2077.exe",
                AppDir = @"D:\Games\Cyberpunk 2077"
            });

            ScannedApps.Add(new ScannedAppModel
            {
                IsSelected = false,
                Icon = dummyIcon,
                Title = "Grand Theft Auto: San Andreas",
                Target = @"D:\Games\GTA San Andreas\gta_sa.exe",
                AppDir = @"D:\Games\GTA San Andreas"
            });

            ScannedApps.Add(new ScannedAppModel
            {
                IsSelected = true,
                Icon = dummyIcon,
                Title = "Hollow Knight",
                Target = @"D:\Games\Hollow Knight\hollow_knight.exe",
                AppDir = @"D:\Games\Hollow Knight"
            });

            ScannedApps.Add(new ScannedAppModel
            {
                IsSelected = false,
                Icon = dummyIcon,
                Title = "The Witcher 3: Wild Hunt",
                Target = @"D:\Games\The Witcher 3\bin\x64\witcher3.exe",
                AppDir = @"D:\Games\The Witcher 3"
            });
        }

        [RelayCommand]
        public async Task BrowseManuallyAsync()
        {
            if (StorageProvider == null) return;

            var platformService = App.PlatformService;
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Application, Game, or Shortcut",
                AllowMultiple = false,
                FileTypeFilter = platformService.GetExecutableFileFilters()
            });

            if (files.Count == 0) return;

            string selectedPath = files[0].Path.LocalPath;
            if (!platformService.IsExecutable(selectedPath)) return;

            var (title, resolvedTargetPath, workingDir) = platformService.ResolveAppMetadata(selectedPath);
            string rootGameDir = platformService.DetectRootAppDirectory(resolvedTargetPath, workingDir);

            // Buat Draft dengan menyertakan TargetSectionId
            Guid newAppId = await _appEditorService.CreateDraftAsync(
                appTitle: title,
                appDir: rootGameDir,
                primaryActionTarget: resolvedTargetPath,
                sectionId: TargetSectionId
            );

            RequestClose?.Invoke(newAppId);
        }

        [RelayCommand]
        public async Task AddSelectedAsync()
        {
            var selectedApps = ScannedApps.Where(x => x.IsSelected).ToList();
            if (selectedApps.Count == 0) return;

            if (selectedApps.Count == 1)
            {
                // Single-selection: Simpan & kirim ID agar parent membuka Edit Properties Window
                var item = selectedApps[0];
                Guid newAppId = await _appEditorService.CreateDraftAsync(
                    item.Title,
                    item.AppDir,
                    item.Target,
                    TargetSectionId
                );

                RequestClose?.Invoke(newAppId);
            }
            else
            {
                // Multi-selection (Batch Import): Simpan semua & tutup tanpa buka dialog properties
                foreach (var item in selectedApps)
                {
                    await _appEditorService.CreateDraftAsync(
                        item.Title,
                        item.AppDir,
                        item.Target,
                        TargetSectionId
                    );
                }

                RequestClose?.Invoke(null); // null menandakan batch selesai
            }
        }

        [RelayCommand]
        public void Cancel() => RequestClose?.Invoke(Guid.Empty);

    }
}
