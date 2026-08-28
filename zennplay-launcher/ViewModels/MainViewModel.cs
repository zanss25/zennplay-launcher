using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using zennplay_launcher.Models;
using zennplay_launcher.Services.Messages;
using zennplay_launcher.Services.Repositories;
using zennplay_launcher.Themes;

namespace zennplay_launcher.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    // ============================================================
    // VARIABLES & VIEWMODELS
    // ============================================================

    // Menampung ViewModel dari halaman/tab yang sedang aktif
    [ObservableProperty]
    private object? _currentView;

    // Repositories
    private readonly SectionRepository _sectionRepo = new();

    // Child ViewModels
    private readonly SectionViewModel _sectionViewModel = new();
    // Ganti ini ke ViewModel WorkInProgress jika sudah dibuat terpisah
    private readonly object _workInProgressViewModel = new();

    // Static tabs list
    [ObservableProperty]
    private ObservableCollection<SidebarTabModel> _sidebarTabs = new();

    // Active static tab
    [ObservableProperty]
    private SidebarTabModel? _selectedSidebarTab;

    // 1. Koleksi yang di-bind ke ListBox Sidebar Sections
    [ObservableProperty]
    private ObservableCollection<SectionModel> _sections = [];

    // 2. Menampung Section yang sedang dipilih user
    [ObservableProperty]
    private SectionModel? _selectedSection;

    public MainViewModel()
    {
        // Load static tabs list
        SidebarTabs = new ObservableCollection<SidebarTabModel>(SidebarTabsList.DefaultTabs);

        // Inisialisasi load data secara aman dari konstruktor
        _ = LoadSectionsAsync();

        // LISTENER BROADCAST (Asinkron)
        WeakReferenceMessenger.Default.Register<SectionsUpdatedMessage>(this, async (_, _) =>
        {
            await LoadSectionsAsync();
        });
    }

    // ============================================================
    // THEME FUNCTIONS
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

    public async Task LoadSectionsAsync()
    {
        var dbSections = await _sectionRepo.GetAllSectionsAsync();

        Sections.Clear();
        foreach (var section in dbSections)
        {
            Sections.Add(section);
        }

        // Set item pertama sebagai default selected jika list tidak kosong
        if (Sections.Any() && SelectedSection == null && SelectedSidebarTab == null)
        {
            SelectedSection = Sections.First();
        }
    }

    // Otomatis terpicu saat SelectedSection berubah
    partial void OnSelectedSectionChanged(SectionModel? value)
    {
        if (value == null) return;

        // Reset seleksi tab statis
        SelectedSidebarTab = null;

        Debug.WriteLine("Selected Section ID: " + value.Id);
        _sectionViewModel.CurrentSection = value;
        CurrentView = _sectionViewModel;
    }

    // ============================================================
    // TABS LOADING & INTERACTION FUNCTIONS
    // ============================================================
    partial void OnSelectedSidebarTabChanged(SidebarTabModel? value)
    {
        if (value == null) return;

        // Reset seleksi pada Section list
        SelectedSection = null;

        Debug.WriteLine("Selected Static Tab ID: " + value.Id);

        // Eksekusi Logika Navigasi UI
        switch (value.Id)
        {
            case "library":
            case "favorites":
            case "boards":
            case "relics":
                Debug.WriteLine("Switch case: " + value.Name);
                CurrentView = _workInProgressViewModel;
                break;
        }
    }
}