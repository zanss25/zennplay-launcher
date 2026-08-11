using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using zennplay_launcher.Models;
using zennplay_launcher.Services.Repositories;
using zennplay_launcher.Themes;
using zennplay_launcher.Views;
using zennplay_launcher.Views.Tabs;

namespace zennplay_launcher.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        // Load Sections list
        _sectionRepo = new SectionRepository();
        LoadSections();

        // Load static tabs list
        SidebarTabs = new ObservableCollection<SidebarTabModel>(SidebarTabsList.DefaultTabs);
    }

    // ============================================================
    // VARIABLES
    // ============================================================

    // Properti untuk menampung View/Halaman yang sedang aktif
    [ObservableProperty]
    object? _currentView;

    // Properti untuk menampung apps
    private readonly AppRepository _appRepo = new();

    // Properti untuk menampung sections
    private readonly SectionRepository _sectionRepo = new();

    // Properti koleksi yang di-bind oleh XAML
    [ObservableProperty]
    private ObservableCollection<AppModel> _displayedApps = new();

    // Tab View Instances
    private readonly SectionView _sectionView = new();
    private readonly WorkInProgress _workInProgress = new();

    // Static tabs list
    [ObservableProperty]
    private ObservableCollection<SidebarTabModel> _sidebarTabs = new();

    // Active static tab
    [ObservableProperty]
    private SidebarTabModel? _selectedSidebarTab;

    // ============================================================
    // THEME FUNCTIONS (WILL BE MOVED LATER)
    // ============================================================
    public void SetTheme()
    {
        try
        {
            ThemeManager.SetColorScheme("ObsidianPurple");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }


    // ============================================================
    // SECTION LOADING FUNCTIONS
    // ============================================================

    // 1. Koleksi yang di-bind ke ListBox Sidebar di XAML
    [ObservableProperty]
    private ObservableCollection<SectionModel> _sections = [];

    // 2. Menampung Section yang sedang dipilih/diklik user di sidebar
    [ObservableProperty]
    private SectionModel? _selectedSection;

    public void LoadSections()
    {
        var dbSections = _sectionRepo.GetAllSections();

        Sections.Clear();
        foreach (var sec in dbSections)
        {
            Sections.Add(sec);
        }

        // Set item pertama sebagai default selected jika list tidak kosong
        if (Sections.Any())
        {
            SelectedSection = Sections.First();
        }
    }

    // ============================================================
    // TABS LOADING & INTERACTION FUNCTIONS
    // ============================================================
    partial void OnSelectedSidebarTabChanged(SidebarTabModel? tab)
    {
        if (tab == null) return;

        // Reset seleksi pada SidebarSections secara otomatis
        SelectedSection = null;

        if (tab != null) Debug.WriteLine("Selected Static Tab ID: " + tab.Id);

        // Eksekusi Logika Navigasi UI
        switch (tab.Id)
        {
            case "library":
                Debug.WriteLine("Switch case: " + tab.Name);
                CurrentView = _workInProgress;
                break;
            case "favorites":
                Debug.WriteLine("Switch case: " + tab.Name);
                CurrentView = _workInProgress;
                break;
            case "boards":
                Debug.WriteLine("Switch case: " + tab.Name);
                CurrentView = _workInProgress;
                break;
            case "relics":
                Debug.WriteLine("Switch case: " + tab.Name);
                CurrentView = _workInProgress;
                break;
        }
    }

    // Otomatis terpicu saat SelectedSection berubah
    partial void OnSelectedSectionChanged(SectionModel? section)
    {
        if (section == null) return;

        // Reset seleksi tab statis
        SelectedSidebarTab = null;

        Debug.WriteLine("Selected Section ID: " + section.Id);

        IEnumerable<AppModel> apps = _appRepo.GetAppsFiltered(section.Id);
        DisplayedApps = new ObservableCollection<AppModel>(apps);
        CurrentView = _sectionView;
    }
}
