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
            _ = LoadAppsAsync();
        }

        partial void OnCurrentSectionChanged(SectionModel value)
        {
            if (value != null && value.Id != Guid.Empty)
            {
                _ = LoadAppsAsync();
            }
        }

        public async Task LoadAppsAsync()
        {
            var criteria = new AppFilterCriteria { SectionIds = new List<Guid> { SectionId } };

            IEnumerable<AppModel> apps = await _appRepo.GetAppsFilteredAsync(criteria);
            DisplayedApps = new ObservableCollection<AppModel>(apps);
        }

        [RelayCommand]
        private async Task AddGameAsync()
        {
            Guid? resultAppId = await _dialogService.ShowNewAppSelectionAsync(SectionId);

            if (resultAppId == Guid.Empty) return;

            if (resultAppId.HasValue)
            {
                bool isSaved = await _dialogService.ShowAppPropertiesAsync(resultAppId.Value, isNew: true, SectionId);
                await LoadAppsAsync();
            }
            else
            {
                await LoadAppsAsync();
            }
        }

        [RelayCommand]
        private async Task EditAppAsync(AppModel? app)
        {
            if (app == null) return;

            bool isSaved = await _dialogService.ShowAppPropertiesAsync(app.Id, isNew: false, SectionId);
            if (isSaved)
            {
                await LoadAppsAsync();
            }
        }

        [RelayCommand]
        private async Task SoftDeleteAppAsync(AppModel? app)
        {
            if (app == null) return;

            await _appRepo.SoftDeleteAppAsync(app.Id);
            await LoadAppsAsync();
        }

        [RelayCommand]
        private async Task HardDeleteAppAsync(AppModel? app)
        {
            if (app == null) return;

            await _appRepo.HardDeleteAppAsync(app.Id);
            await LoadAppsAsync();
        }
    }
}