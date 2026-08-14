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
        private int _appId;

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
        private readonly PropertiesMetadataView _metadataView = new();
        private readonly PropertiesFoldersView _foldersView = new();
        private readonly PropertiesMediaView _mediaView = new();

        // Method untuk inisialisasi mode
        public void Initialize(int appId)
        {
            AppId = appId;
            IsEditMode = appId >= 0;

            if (IsEditMode)
            {
                LoadAppData(appId);
            }
        }

        public AppPropertiesViewModel()
        {
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
                    CurrentView = _metadataView;
                    break;
                case "folders":
                    Debug.WriteLine("Switch case: " + value.Name);
                    CurrentView = _foldersView;
                    break;
                case "media":
                    Debug.WriteLine("Switch case: " + value.Name);
                    CurrentView = _mediaView;
                    break;
            }
        }

        // ============================================================
        // FETCH APP DATA
        // ============================================================
        public void LoadAppData(int appId)
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
