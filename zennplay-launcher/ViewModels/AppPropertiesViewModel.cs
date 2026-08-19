using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Models;
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
        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private Guid _appId = Guid.Empty;

        [ObservableProperty]
        private AppModel? _currentApp;

        private readonly AppRepository? _appRepo;

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
        public void Initialize(Guid appId)
        {
            AppId = appId;
            IsEditMode = appId != Guid.Empty;

            if (IsEditMode)
            {
                LoadAppData(appId);
            }

            LoadTabsData();
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

        public void LoadTabsData(int appId = 0)
        {
            MetadataViewModel.LoadData(appId);
        }

        // ============================================================
        // FETCH APP DATA
        // ============================================================
        public void LoadAppData(Guid appId)
        {
            AppModel? app = _appRepo.GetAppById(appId);
            if (app != null)
            {
                CurrentApp = app;
            }
        }

        // ============================================================
        // APP EDIT OPERATIONS
        // ============================================================


    }
}
