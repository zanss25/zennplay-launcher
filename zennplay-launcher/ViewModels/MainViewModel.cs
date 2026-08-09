using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using zennplay_launcher.Models;
using zennplay_launcher.Services.Repositories;
using zennplay_launcher.Themes;

namespace zennplay_launcher.ViewModels;

public partial class MainViewModel : ViewModelBase
{
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
    private readonly SectionRepository _sectionRepo;

    // 1. Koleksi yang di-bind ke ListBox Sidebar di XAML
    [ObservableProperty]
    private ObservableCollection<SectionModel> _sections = [];

    // 2. Menampung Section yang sedang dipilih/diklik user di sidebar
    [ObservableProperty]
    private SectionModel? _selectedSection;

    public MainViewModel()
    {
        _sectionRepo = new SectionRepository();

        LoadSections();
    }

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

    // Method ini otomatis ter panggil oleh CommunityToolkit saat SelectedSection berubah
    partial void OnSelectedSectionChanged(SectionModel? value)
    {
        if (value == null) return;

        Debug.WriteLine($"User berpindah ke Section: {value.Name} (ID: {value.Id})");
    }

}
