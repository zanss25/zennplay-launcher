using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Models;
using zennplay_launcher.Services;
using zennplay_launcher.Services.Repositories;
using zennplay_launcher.Views;
using static Microsoft.IO.RecyclableMemoryStreamManager;
using static System.Collections.Specialized.BitVector32;

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

        public async Task<Guid?> AddActionBrowseAsync(IStorageProvider storageProvider)
        {
            var platformService = App.PlatformService;

            // 1. Izinkan semua tipe file (All Files + Executables + Common Configs)
            var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Target File",
                AllowMultiple = false,
                FileTypeFilter = [FilePickerFileTypes.All]
            });

            if (files.Count == 0)
                return null;

            string selectedPath = files[0].Path.LocalPath;

            // 2. Validasi keberadaan file nyata (bukan pembatasan binary executable)
            if (!File.Exists(selectedPath))
            {
                return null;
            }

            // 3. Deteksi Type & Metadata
            string actionType = DetectTypeFromExtension.DetectFileType(selectedPath);
            bool isExecutable = platformService.IsExecutable(selectedPath);

            string fileTitle;
            if (isExecutable)
            {
                (string title, _, _) = platformService.ResolveAppMetadata(selectedPath);
                fileTitle = !string.IsNullOrWhiteSpace(title) ? title : Path.GetFileNameWithoutExtension(selectedPath);
            }
            else
            {
                // Untuk file teks/config, tampilkan nama file beserta ekstensinya agar jelas (misal: "settings.ini")
                fileTitle = Path.GetFileName(selectedPath);
            }

            // 4. Tentukan Sort Order
            int nextSortOrder = AppActions.Count;

            // 5. Buat Model Action Baru
            var newAction = new AppActionModel
            {
                Id = Guid.NewGuid(),
                AppId = AppId,
                Title = fileTitle,
                ActionType = actionType,
                Target = selectedPath,
                IsGame = isExecutable, // Otomatis true jika executable, false jika config/dokumen
                SortOrder = nextSortOrder
            };

            // 6. Masukkan ke UI Collection
            AppActions.Add(newAction);

            return newAction.Id;
        }

        [RelayCommand]
        private async Task AddActionAsync()
        {
            if (StorageProvider != null)
            {
                await AddActionBrowseAsync(StorageProvider);
            }
        }

        [RelayCommand]
        private async Task EditActionAsync(AppActionModel? action)
        {
            if (action == null) return;

            // Panggil dialog melalui service
            bool isSaved = await _dialogService.ShowAppActionPropertiesAsync(action);

            if (isSaved)
            {
                // Beri tahu UI bahwa isi item telah berubah
                int index = AppActions.IndexOf(action);
                if (index >= 0)
                {
                    AppActions[index] = action;
                }
            }
        }

        [RelayCommand]
        private async Task RemoveActionAsync(AppActionModel? action)
        {
            AppActions.Remove(action);
        }

        [RelayCommand]
        public async Task AddLinkAsync()
        {
            var newLink = new AppLinkModel
            {
                Id = Guid.NewGuid(),
                AppId = AppId,
                Name = "New Link",
                Desc = String.Empty,
                Url = String.Empty
            };

            // 5. Tambahkan hanya ke koleksi UI (ObservableCollection)
            AppLinks.Add(newLink);
            await EditLinkAsync(newLink);

            if (String.IsNullOrEmpty(newLink.Url))
            {
                AppLinks.Remove(newLink);
            }
        }

        [RelayCommand]
        public async Task EditLinkAsync(AppLinkModel? link)
        {
            if (link == null) return;

            // Panggil dialog melalui service
            bool isSaved = await _dialogService.ShowAppLinkPropertiesAsync(link);

            if (isSaved)
            {
                // Beri tahu UI bahwa isi item telah berubah
                int index = AppLinks.IndexOf(link);
                if (index >= 0)
                {
                    AppLinks[index] = link;
                }
            }
        }

        [RelayCommand]
        public async Task RemoveLinkAsync(AppLinkModel? link)
        {
            AppLinks.Remove(link);
        }

    }
}
