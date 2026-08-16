using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Models;
using zennplay_launcher.Services.Repositories;

namespace zennplay_launcher.ViewModels.Controls
{
    public partial class PropertiesMetadataViewModel : ViewModelBase
    {
        // ============================================================
        // VARIABLES
        // ============================================================

        // Repositories
        private readonly SeriesRepository _seriesRepo = new();
        private readonly GenreRepository _genreRepo = new();
        private readonly TagRepository _tagRepo = new();
        private readonly DeveloperRepository _developerRepo = new();
        private readonly PublisherRepository _publisherRepo = new();
        private readonly PlatformRepository _platformRepo = new();

        // Series list
        // 1. Data Master (Untuk ItemsSource)
        [ObservableProperty]
        private ObservableCollection<SelectableItem> _availableSeries = new();

        // 2. Teks Terpilih (Untuk Contents / Text)
        [ObservableProperty]
        private string? _selectedSeriesText = string.Empty;


        // Genres list
        // 1. Data Master (Untuk ItemsSource)
        [ObservableProperty]
        private ObservableCollection<SelectableItem> _availableGenres = new();

        // 2. Teks Terpilih (Untuk Contents / Text)
        [ObservableProperty]
        private string? _selectedGenresText = string.Empty;

        // Tags list
        // 1. Data Master (Untuk ItemsSource)
        [ObservableProperty]
        private ObservableCollection<SelectableItem> _availableTags = new();

        // 2. Teks Terpilih (Untuk Contents / Text)
        [ObservableProperty]
        private string? _selectedTagsText = string.Empty;

        // Developers list
        // 1. Data Master (Untuk ItemsSource)
        [ObservableProperty]
        private ObservableCollection<SelectableItem> _availableDevelopers = new();

        // 2. Teks Terpilih (Untuk Contents / Text)
        [ObservableProperty]
        private string? _selectedDevelopersText = string.Empty;

        // Publishers list
        // 1. Data Master (Untuk ItemsSource)
        [ObservableProperty]
        private ObservableCollection<SelectableItem> _availablePublishers = new();

        // 2. Teks Terpilih (Untuk Contents / Text)
        [ObservableProperty]
        private string? _selectedPublishersText = string.Empty;

        // Platforms list
        // 1. Data Master (Untuk ItemsSource)
        [ObservableProperty]
        private ObservableCollection<SelectableItem> _availablePlatforms = new();

        // 2. Teks Terpilih (Untuk Contents / Text)
        [ObservableProperty]
        private string? _selectedPlatformsText = string.Empty;

        // ============================================================
        // FUNCTIONS
        // ============================================================
        [RelayCommand]
        private async Task OpenAddNewDialogAsync(string _targetTable)
        {
            // Logika untuk menampilkan popup/input genre baru
        }

        // Method load data awal
        public void LoadData(int appId = 0)
        {
            var seriesFromDb = _seriesRepo.GetAllSeries();
            var selectedSeriesSet = (SelectedSeriesText ?? string.Empty)
                                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            AvailableSeries = new ObservableCollection<SelectableItem>(
                seriesFromDb.Select(s => new SelectableItem(
                    name: s.Name,
                    id: s.Id,
                    description: s.Desc,
                    isSelected: selectedSeriesSet.Contains(s.Name)
                ))
            );

            var genresFromDb = _genreRepo.GetAllGenres();
            var selectedGenresSet = (SelectedGenresText ?? string.Empty)
                                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            AvailableGenres = new ObservableCollection<SelectableItem>(
                genresFromDb.Select(g => new SelectableItem(
                    name: g.Name,
                    id: g.Id,
                    description: g.Desc,
                    isSelected: selectedGenresSet.Contains(g.Name)
                ))
            );

            var tagsFromDb = _tagRepo.GetAllTags();
            var selectedTagsSet = (SelectedTagsText ?? string.Empty)
                                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            AvailableTags = new ObservableCollection<SelectableItem>(
                tagsFromDb.Select(g => new SelectableItem(
                    name: g.Name,
                    id: g.Id,
                    description: g.Desc,
                    isSelected: selectedTagsSet.Contains(g.Name)
                ))
            );

            var developersFromDb = _developerRepo.GetAllDevelopers();
            var selectedDevelopersSet = (SelectedDevelopersText ?? string.Empty)
                                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            AvailableDevelopers = new ObservableCollection<SelectableItem>(
                developersFromDb.Select(d => new SelectableItem(
                    name: d.Name,
                    id: d.Id,
                    description: d.Desc,
                    isSelected: selectedDevelopersSet.Contains(d.Name)
                ))
            );

            var publishersFromDb = _publisherRepo.GetAllPublishers();
            var selectedPublishersSet = (SelectedPublishersText ?? string.Empty)
                                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            AvailablePublishers = new ObservableCollection<SelectableItem>(
                publishersFromDb.Select(p => new SelectableItem(
                    name: p.Name,
                    id: p.Id,
                    description: p.Desc,
                    isSelected: selectedPublishersSet.Contains(p.Name)
                ))
            );

            var platformsFromDb = _platformRepo.GetAllPlatforms();
            var selecteTagsSet = (SelectedPlatformsText ?? string.Empty)
                                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            AvailablePlatforms = new ObservableCollection<SelectableItem>(
                platformsFromDb.Select(l => new SelectableItem(
                    name: l.Name,
                    id: l.Id,
                    description: l.Desc,
                    isSelected: selectedGenresSet.Contains(l.Name)
                ))
            );

        }

    }
}
