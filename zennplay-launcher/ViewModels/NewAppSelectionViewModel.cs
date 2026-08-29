using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using zennplay_launcher.Models;
using zennplay_launcher.Services;

namespace zennplay_launcher.ViewModels
{
    public partial class NewAppSelectionViewModel : ViewModelBase
    {
        // ============================================================
        // INSTANCES & MASTER DATA
        // ============================================================
        private readonly AppEditorService _appEditorService = new();
        private readonly List<ScannedAppModel> _allScannedApps = []; // Master data cache

        public IStorageProvider? StorageProvider { get; set; }
        public Guid? TargetSectionId { get; set; }

        public event Action<Guid?>? RequestClose;

        // ============================================================
        // OBSERVABLE COLLECTIONS & PROPERTIES
        // ============================================================
        // Di-bind ke DataGrid XAML
        public ObservableCollection<ScannedAppModel> ScannedApps { get; } = [];

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        public NewAppSelectionViewModel()
        {
            _ = LoadScannedAppsAsync();
        }

        // ============================================================
        // FILTER SEARCH LOGIC
        // ============================================================
        partial void OnSearchQueryChanged(string value)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            ScannedApps.Clear();

            var query = SearchQuery?.Trim();

            IEnumerable<ScannedAppModel> filtered = string.IsNullOrWhiteSpace(query)
                ? _allScannedApps
                : _allScannedApps.Where(a =>
                    a.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    a.Target.Contains(query, StringComparison.OrdinalIgnoreCase));

            foreach (var app in filtered)
            {
                ScannedApps.Add(app);
            }
        }

        // ============================================================
        // DATA SCANNING
        // ============================================================
        private async Task LoadScannedAppsAsync()
        {
            _allScannedApps.Clear();
            ScannedApps.Clear();

            // 1. Eksekusi scanning di background thread
            var discoveredApps = await Task.Run(() =>
            {
                return App.PlatformService.ScanInstalledApps()
                                          .OrderBy(a => a.Title)
                                          .ToList();
            });

            // 2. Simpan ke master cache dan tampilkan di UI
            _allScannedApps.AddRange(discoveredApps);
            ApplyFilter();
        }

        // ============================================================
        // COMMANDS
        // ============================================================
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

            Guid newAppId = await _appEditorService.CreateAppAsync(
                appTitle: title,
                appDir: rootGameDir,
                primaryActionTarget: resolvedTargetPath,
                iconSourcePath: null,
                sectionId: TargetSectionId
            );

            RequestClose?.Invoke(newAppId);
        }

        [RelayCommand]
        public async Task AddSelectedAsync()
        {
            // Ambil data yang dicentang dari master list agar item yang terfilter tetap terakomodasi
            var selectedApps = _allScannedApps.Where(x => x.IsSelected).ToList();
            if (selectedApps.Count == 0) return;

            if (selectedApps.Count == 1)
            {
                var item = selectedApps[0];
                Guid newAppId = await _appEditorService.CreateAppAsync(
                    appTitle: item.Title,
                    appDir: item.AppDir,
                    primaryActionTarget: item.Target,
                    iconSourcePath: item.IconSourcePath,
                    sectionId: TargetSectionId
                );

                RequestClose?.Invoke(newAppId);
            }
            else
            {
                foreach (var item in selectedApps)
                {
                    await _appEditorService.CreateAppAsync(
                        appTitle: item.Title,
                        appDir: item.AppDir,
                        primaryActionTarget: item.Target,
                        iconSourcePath: item.IconSourcePath,
                        sectionId: TargetSectionId
                    );
                }

                RequestClose?.Invoke(null);
            }
        }

        [RelayCommand]
        public void Cancel() => RequestClose?.Invoke(Guid.Empty);
    }

}