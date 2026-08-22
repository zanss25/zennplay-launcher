using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using zennplay_launcher.Models;
using zennplay_launcher.Services;
using zennplay_launcher.Services.Repositories;
using zennplay_launcher.Themes;
using zennplay_launcher.Views;
using zennplay_launcher.Views.Tabs;
using static System.Collections.Specialized.BitVector32;

namespace zennplay_launcher.ViewModels
{
    public partial class SectionViewModel : ViewModelBase
    {
        [ObservableProperty] private SectionModel _currentSection = new();
        private readonly IDialogService _dialogService = new DialogService();

        // Properti untuk menampung apps
        private readonly AppRepository _appRepo = new();
        [ObservableProperty] private ObservableCollection<AppModel> _displayedApps = new();

        public Guid SectionId => CurrentSection.Id;
        public string SectionName => CurrentSection.Name;

        public SectionViewModel() { }

        public SectionViewModel(SectionModel section)
        {
            CurrentSection = section;
            LoadApps();
        }

        partial void OnCurrentSectionChanged(SectionModel value)
        {
            if (value != null && value.Id != Guid.Empty)
            {
                LoadApps();
            }
        }

        public void LoadApps()
        {
            var criteria = new AppFilterCriteria { SectionIds = new List<Guid> { SectionId } };

            IEnumerable<AppModel> apps = _appRepo.GetAppsFiltered(criteria);
            DisplayedApps = new ObservableCollection<AppModel>(apps);
        }

        [RelayCommand]
        private async Task AddGameAsync()
        {
            // 1. Minta service buka window seleksi
            Guid? resultAppId = await _dialogService.ShowNewAppSelectionAsync();
            if (!resultAppId.HasValue || resultAppId.Value == Guid.Empty) return;

            // 2. Minta service buka properties window
            bool isSaved = await _dialogService.ShowAppPropertiesAsync(resultAppId.Value, isNew: true, SectionId);

            // 3. ViewModel sendiri yang memutuskan kapan harus reload data
            if (isSaved)
            {
                LoadApps();
            }
        }

        [RelayCommand]
        private async Task EditAppAsync(AppModel? app)
        {
            if (app == null) return;

            bool isSaved = await _dialogService.ShowAppPropertiesAsync(app.Id, isNew: false, SectionId);
            if (isSaved)
            {
                LoadApps();
            }
        }

        [RelayCommand]
        private void SoftDeleteApp(AppModel? app)
        {
            if (app == null) return;

            _appRepo.SoftDeleteApp(app.Id);
            LoadApps();
        }

        [RelayCommand]
        private void HardDeleteApp(AppModel? app)
        {
            if (app == null) return;

            _appRepo.HardDeleteApp(app.Id);
            LoadApps();
        }

    }
}
