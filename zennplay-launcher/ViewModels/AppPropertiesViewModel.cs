using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Models;
using zennplay_launcher.Services;
using zennplay_launcher.Services.Repositories;
using zennplay_launcher.ViewModels.Controls;
using zennplay_launcher.Views.Controls;
using zennplay_launcher.Views.Tabs;

namespace zennplay_launcher.ViewModels
{
    public partial class AppPropertiesViewModel : ViewModelBase
    {
        // ============================================================
        // VARIABLES
        // ============================================================
        private bool isNewApp = false;
        public bool IsSaved { get; private set; } = false;

        [ObservableProperty]
        private Guid _appId = Guid.Empty;

        [ObservableProperty]
        private AppModel? _currentApp;

        private readonly AppRepository? _appRepo = new();

        // Tabs list
        [ObservableProperty]
        private ObservableCollection<BasicTabModel> _propertiesTabs = [];

        // Active static tab
        [ObservableProperty]
        private BasicTabModel? _selectedTab;

        // Properti untuk menampung View/Halaman yang sedang aktif
        [ObservableProperty]
        object? _currentView;

        // Tab View Instances
        public PropertiesMetadataViewModel MetadataViewModel { get; } = new();
        public PropertiesFoldersViewModel FoldersViewModel { get; } = new();
        public PropertiesMediaViewModel MediaViewModel { get; } = new();
        public PropertiesMetadataView MetadataView { get; } = new();
        public PropertiesFoldersView FoldersView { get; } = new();
        public PropertiesMediaView MediaView { get; } = new();

        // Method untuk inisialisasi mode
        public void Initialize(Guid appId, bool isNew = false, Guid? initialSectionId = null)
        {
            isNewApp = isNew;
            AppId = appId;
            LoadAppData(appId);
            LoadTabsData();

            if (CurrentApp != null)
            {
                MetadataViewModel.LoadFromModel(CurrentApp);
                FoldersViewModel.LoadFromModel(CurrentApp);
                FoldersViewModel.LoadAppActions(AppId);
                FoldersViewModel.LoadAppLinks(AppId);
            }

            // Auto-select section awal jika ada (khusus add new app)
            if (initialSectionId.HasValue && initialSectionId.Value != Guid.Empty)
            {
                MetadataViewModel.PreselectSection(initialSectionId.Value);
            }
        }

        public AppPropertiesViewModel()
        {
            MetadataView = new PropertiesMetadataView
            {
                DataContext = MetadataViewModel
            };

            FoldersView = new PropertiesFoldersView
            {
                DataContext = FoldersViewModel
            };

            // Load static tabs list
            PropertiesTabs = new ObservableCollection<BasicTabModel>(PropertiesTabsList.PropertiesTabs);

            if (PropertiesTabs.Any())
            {
                SelectedTab = PropertiesTabs.First();
            }
        }

        // ============================================================
        // TABS LOADING & INTERACTION FUNCTIONS
        // ============================================================
        partial void OnSelectedTabChanged(BasicTabModel? value)
        {
            if (value == null) return;

            if (value != null) Debug.WriteLine("Selected Properties Tab ID: " + value.Id);

            switch (value.Id)
            {
                case "metadata":
                    Debug.WriteLine("Switch case: " + value.Name);
                    CurrentView = MetadataView;
                    break;
                case "folders":
                    Debug.WriteLine("Switch case: " + value.Name);
                    CurrentView = FoldersView;
                    break;
                case "media":
                    Debug.WriteLine("Switch case: " + value.Name);
                    CurrentView = MediaView;
                    break;
            }
        }

        public void LoadTabsData(Guid appId = default)
        {
            MetadataViewModel.LoadData(appId);
        }

        // ============================================================
        // FETCH APP DATA
        // ============================================================
        public void LoadAppData(Guid appId)
        {
            if (appId != Guid.Empty)
            {
                AppModel? app = _appRepo.GetAppById(appId);

                if (app != null)
                {
                    CurrentApp = app;
                }
            }
        }

        // ============================================================
        // APP EDIT OPERATIONS
        // ============================================================
        public event Action? RequestClose;

        [RelayCommand]
        private void Save()
        {
            if (_appRepo == null || CurrentApp == null) return;

            // 1. Salin semua input form (Title, Desc, Rating, Status, Switches, dll) ke objek CurrentApp
            MetadataViewModel.ApplyToModel(CurrentApp);
            FoldersViewModel.ApplyToModel(CurrentApp);

            for (int i = 0; i < FoldersViewModel.AppActions.Count; i++)
            {
                FoldersViewModel.AppActions[i].SortOrder = i;
            }

            // 2. Eksekusi UpdateApp dengan menyertakan seluruh junction IDs
            bool isSuccess = _appRepo.UpdateApp(
                app: CurrentApp,
                actions: [.. FoldersViewModel.AppActions],
                links: [.. FoldersViewModel.AppLinks],
                ageRatingIds: MetadataViewModel.SelectedAgeRatingIds,
                developerIds: MetadataViewModel.SelectedDeveloperIds,
                publisherIds: MetadataViewModel.SelectedPublisherIds,
                platformIds: MetadataViewModel.SelectedPlatformIds,
                seriesIds: MetadataViewModel.SelectedSeriesIds,
                tagIds: MetadataViewModel.SelectedTagIds,
                genreIds: MetadataViewModel.SelectedGenreIds,
                sectionIds: MetadataViewModel.SelectedSectionIds
            );

            // 3. Verifikasi keberhasilan simpan
            if (isSuccess)
            {
                IsSaved = true; // Menandai agar draft tidak dihapus saat OnClosing
                RequestClose?.Invoke();
            }
            else
            {
                Debug.WriteLine("Failed to update app metadata in database.");
            }
        }

        public void DeleteDraftIfNotSaved()
        {
            if (isNewApp && !IsSaved && _appRepo != null && AppId != Guid.Empty)
            {
                _appRepo.HardDeleteApp(AppId);
            }
            else
            {
                
            }
        }
        
    }
}
