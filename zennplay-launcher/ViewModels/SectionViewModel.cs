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
        [ObservableProperty] private ObservableCollection<AppModel> _displayedApps = [];

        // Properti app terpilih
        [ObservableProperty] private AppModel? _selectedApp;

        // Properti untuk menampung sort by
        [ObservableProperty]
        private ObservableCollection<UISortCriteriaModel> _sortByCriterias = new(UISortByList.UISortBy);

        // Properti sort by terpilih
        [ObservableProperty] private UISortCriteriaModel? _selectedSortBy;

        // Properti untuk menampung sort by
        [ObservableProperty]
        private ObservableCollection<UISortCriteriaModel> _sortDirections = new(UISortDirectionList.UISortDirection);

        // Properti Asc/Desc terpilih
        [ObservableProperty] private UISortCriteriaModel? _selectedSortDirection;

        // Properti untuk menampung view modes
        [ObservableProperty]
        private ObservableCollection<BasicTabModel> _appViewModes = new(AppViewModesList.AppViewModes);

        // Active static view mode
        [ObservableProperty]
        private BasicTabModel? _selectedViewMode;

        // Properti pembantu untuk binding IsVisible
        public bool IsGridMode => SelectedViewMode?.Id == "grid";
        public bool IsListMode => SelectedViewMode?.Id == "list";
        public bool IsPanelMode => SelectedViewMode?.Id == "panel";

        public Guid SectionId => CurrentSection.Id;
        public string SectionName => CurrentSection.Name;

        public SectionViewModel() 
        {
            SelectedViewMode = AppViewModes.First();
            SelectedSortBy = SortByCriterias.FirstOrDefault();
            SelectedSortDirection = SortDirections.FirstOrDefault();
        }

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
            AppSortBy currentSortBy = AppSortBy.Title;
            SortDirection currentSortDirection = SortDirection.Ascending;
            if (SelectedSortBy != null) currentSortBy = (AppSortBy)SelectedSortBy.CriteriaId;
            if (SelectedSortDirection != null) currentSortDirection = (SortDirection)SelectedSortDirection.CriteriaId;

            var criteria = new AppFilterCriteria { SectionIds = new List<Guid> { SectionId },
                                                   SortBy = currentSortBy, 
                                                   SortDirection = currentSortDirection};

            IEnumerable<AppModel> apps = await _appRepo.GetAppsFilteredAsync(criteria);
            var sortedApps = apps.OrderByDescending(a => a.IsPinned);

            DisplayedApps = new ObservableCollection<AppModel>(sortedApps);
        }

        partial void OnSelectedAppChanged(AppModel? value)
        {
            if (value == null) return;

            // Logika saat game dipilih (misal: load screenshot, update detail pane)
            Debug.WriteLine($"Selected game: {value.Title}");
        }

        partial void OnSelectedSortByChanged(UISortCriteriaModel? value)
        {
            _ = LoadAppsAsync();
        }

        partial void OnSelectedSortDirectionChanged(UISortCriteriaModel? value)
        {
            _ = LoadAppsAsync();
        }

        partial void OnSelectedViewModeChanged(BasicTabModel? value)
        {
            // Beritahu UI untuk mengevaluasi ulang ketiga properti boolean
            OnPropertyChanged(nameof(IsGridMode));
            OnPropertyChanged(nameof(IsListMode));
            OnPropertyChanged(nameof(IsPanelMode));
        }

        [RelayCommand]
        private async Task AddAppAsync()
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
        private async Task PinAppAsync(AppModel? app)
        {
            if (app == null) return;
            bool isPinned = app.IsPinned;
            if (!isPinned)
            {
                app.IsPinned = true;
            }
            else
            {
                app.IsPinned = false;
            }

            await _appRepo.UpdateAppAsync(app);
            await LoadAppsAsync();
        }

        [RelayCommand]
        private static void OpenAppFolder(AppModel? app)
        {
            if (app != null && app.AppDir != null)
            {
                DirectoryService.OpenDirectory(app.AppDir);
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