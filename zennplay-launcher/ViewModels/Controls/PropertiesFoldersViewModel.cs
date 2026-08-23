using Avalonia;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Models;
using zennplay_launcher.Services;
using zennplay_launcher.Services.Repositories;

namespace zennplay_launcher.ViewModels.Controls
{
    public partial class PropertiesFoldersViewModel : ViewModelBase
    {
        // ============================================================
        // REPOSITORIES
        // ============================================================
        private readonly AppRepository _appRepo = new();
        private readonly SectionRepository _sectionRepo = new();
        private readonly SeriesRepository _seriesRepo = new();
        private readonly GenreRepository _genreRepo = new();
        private readonly TagRepository _tagRepo = new();
        private readonly DeveloperRepository _developerRepo = new();
        private readonly PublisherRepository _publisherRepo = new();
        private readonly PlatformRepository _platformRepo = new();
        private readonly AgeRatingRepository _ageRatingRepo = new();

        // ============================================================
        // OBSERVABLE COLLECTIONS
        // ============================================================
        [ObservableProperty] private ObservableCollection<AppActionModel> _appActions = [];
        [ObservableProperty] private ObservableCollection<AppLinkModel> _appLinks = [];

        // ============================================================
        // 1. FORM INPUT PROPERTIES
        // ============================================================
        [ObservableProperty] private Guid _appId;
        [ObservableProperty] private string _appDir;
        [ObservableProperty] private string _appVersion;
        [ObservableProperty] private bool _isInstalled;
        [ObservableProperty] private long _installSizeBytes = 0;
        [ObservableProperty] private string _installSizeDisplay = "-";

        // ============================================================
        // VARIABLES
        // ============================================================
        private readonly IDialogService _dialogService = new DialogService();
        public IStorageProvider? StorageProvider { get; set; }

        public PropertiesFoldersViewModel()
        {
            
        }

        // ============================================================
        // FUNCTIONS
        // ============================================================
        public async void LoadFromModel(AppModel app)
        {
            AppId = app.Id;
            AppDir = app.AppDir ?? string.Empty;
            AppVersion = app.Version ?? string.Empty;
            IsInstalled = app.IsInstalled;
            
            if (app.InstallSize > 0)
            {
                InstallSizeBytes = app.InstallSize;
                InstallSizeDisplay = DirectoryService.FormatBytes(app.InstallSize);
            }
            else
            {
                // 2. Jika belum ada di cache, hitung langsung
                await CalculateInstallSizeAsync();
            }

        }

        public void LoadAppActions(Guid appId)
        {
            var dbAppActions = _appRepo.GetAppActions(appId);

            AppActions.Clear();
            foreach (var appAction in dbAppActions)
            {
                AppActions.Add(appAction);
            }
        }

        public void LoadAppLinks(Guid appId)
        {
            var dbAppLinks = _appRepo.GetAppLinks(appId);
            AppLinks.Clear();
            foreach (var appLink in dbAppLinks)
            {
                AppLinks.Add(appLink);
            }
        }

        public void ApplyToModel(AppModel app)
        {
            app.AppDir = AppDir;
            app.IsInstalled = IsInstalled;
            app.InstallSize = InstallSizeBytes;
        }

        [RelayCommand]
        public async Task CalculateInstallSizeAsync()
        {
            if (string.IsNullOrWhiteSpace(AppDir))
            {
                InstallSizeDisplay = "0 B";
                InstallSizeBytes = 0;
                return;
            }

            InstallSizeDisplay = "...";

            // 🔑 Panggil helper async di background thread
            InstallSizeBytes = await DirectoryService.GetDirectorySizeAsync(AppDir);
            InstallSizeDisplay = DirectoryService.FormatBytes(InstallSizeBytes);
        }

        [RelayCommand]
        private async Task BrowseFolderAsync()
        {
            if (StorageProvider == null) return;

            var result = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Installation Directory",
                AllowMultiple = false
            });

            if (result.Count > 0)
            {
                // Ambil path lokal filesystem
                AppDir = result[0].Path.LocalPath;
            }
        }

        [RelayCommand]
        private async Task InstallSizeManualInputAsync()
        {
            string? manualInstallSize = await _dialogService.ShowInputAsync(
                "Enter Install Size",
                "Input install size in bytes (numbers only):",
                "",
                "Ok",
                "Cancel"
            );

            if (string.IsNullOrWhiteSpace(manualInstallSize))
                return;

            // Bersihkan spasi tidak sengaja
            string trimmedInput = manualInstallSize.Trim();

            // Verifikasi: Cek apakah input valid berupa angka positif
            if (long.TryParse(trimmedInput, out long parsedBytes) && parsedBytes >= 0)
            {
                InstallSizeBytes = parsedBytes;
                InstallSizeDisplay = DirectoryService.FormatBytes(InstallSizeBytes);
            }
            else
            {
                // Beri peringatan jika input bukan angka valid
                await _dialogService.ShowConfirmAsync("Invalid Input", "Please enter a valid numeric value in bytes.", "Ok", false);
            }
        }

    }
}
