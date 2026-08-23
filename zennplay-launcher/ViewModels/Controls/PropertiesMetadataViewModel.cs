using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using zennplay_launcher.Models;
using zennplay_launcher.Services.Messages;
using zennplay_launcher.Services.Repositories;

namespace zennplay_launcher.ViewModels.Controls
{
    public partial class PropertiesMetadataViewModel : ViewModelBase
    {
        // ============================================================
        // REPOSITORIES
        // ============================================================
        private readonly SectionRepository _sectionRepo = new();
        private readonly SeriesRepository _seriesRepo = new();
        private readonly GenreRepository _genreRepo = new();
        private readonly TagRepository _tagRepo = new();
        private readonly DeveloperRepository _developerRepo = new();
        private readonly PublisherRepository _publisherRepo = new();
        private readonly PlatformRepository _platformRepo = new();
        private readonly AgeRatingRepository _ageRatingRepo = new();

        // ============================================================
        // OBSERVABLE COLLECTIONS
        // ============================================================
        [ObservableProperty] private ObservableCollection<SelectableItem> _availableSections = new();
        [ObservableProperty] private ObservableCollection<SelectableItem> _availableSeries = new();
        [ObservableProperty] private ObservableCollection<SelectableItem> _availableGenres = new();
        [ObservableProperty] private ObservableCollection<SelectableItem> _availableTags = new();
        [ObservableProperty] private ObservableCollection<SelectableItem> _availableDevelopers = new();
        [ObservableProperty] private ObservableCollection<SelectableItem> _availablePublishers = new();
        [ObservableProperty] private ObservableCollection<SelectableItem> _availablePlatforms = new();
        [ObservableProperty] private ObservableCollection<SelectableItem> _availableAgeRatings = new();
        [ObservableProperty] private ObservableCollection<AppSourceModel> _availableSources = [];
        [ObservableProperty] private ObservableCollection<AppCompletionStatusModel> _availableCompletionStatuses = [];

        // ============================================================
        // 1. FORM INPUT PROPERTIES
        // ============================================================
        [ObservableProperty] private string _title = string.Empty;
        [ObservableProperty] private string _sortingTitle = string.Empty;
        [ObservableProperty] private string? _userRating;
        [ObservableProperty] private string _description = string.Empty;
        [ObservableProperty] private string _notes = string.Empty;
        [ObservableProperty] private string? _releaseDate;

        // Read-only stats
        [ObservableProperty] private string _firstLaunched = "-";
        [ObservableProperty] private string _lastLaunched = "-";
        [ObservableProperty] private string _launchTimes = "0";

        // Switches / Flags (Gunakan bool standar non-nullable)
        [ObservableProperty] private bool _isNotGame;
        [ObservableProperty] private bool _isExcluded;
        [ObservableProperty] private bool _isPinned;

        // Dropdowns
        [ObservableProperty] private AppSourceModel? _selectedSource;
        [ObservableProperty] private AppCompletionStatusModel? _selectedCompletionStatus;

        // Multi-Select Display Text
        [ObservableProperty] private string _selectedSeriesText = string.Empty;
        [ObservableProperty] private string _selectedPlatformsText = string.Empty;
        [ObservableProperty] private string _selectedDevelopersText = string.Empty;
        [ObservableProperty] private string _selectedPublishersText = string.Empty;
        [ObservableProperty] private string _selectedAgeRatingsText = string.Empty;
        [ObservableProperty] private string _selectedGenresText = string.Empty;
        [ObservableProperty] private string _selectedTagsText = string.Empty;
        [ObservableProperty] private string _selectedSectionsText = string.Empty;

        // 🔑 Multi-Select Junction IDs: Diekstrak otomatis dari item yang IsSelected = true
        public List<Guid> SelectedSectionIds => AvailableSections.Where(x => x.IsSelected).Select(x => x.Id).ToList();
        public List<Guid> SelectedSeriesIds => AvailableSeries.Where(x => x.IsSelected).Select(x => x.Id).ToList();
        public List<Guid> SelectedPlatformIds => AvailablePlatforms.Where(x => x.IsSelected).Select(x => x.Id).ToList();
        public List<Guid> SelectedDeveloperIds => AvailableDevelopers.Where(x => x.IsSelected).Select(x => x.Id).ToList();
        public List<Guid> SelectedPublisherIds => AvailablePublishers.Where(x => x.IsSelected).Select(x => x.Id).ToList();
        public List<Guid> SelectedAgeRatingIds => AvailableAgeRatings.Where(x => x.IsSelected).Select(x => x.Id).ToList();
        public List<Guid> SelectedGenreIds => AvailableGenres.Where(x => x.IsSelected).Select(x => x.Id).ToList();
        public List<Guid> SelectedTagIds => AvailableTags.Where(x => x.IsSelected).Select(x => x.Id).ToList();

        public PropertiesMetadataViewModel()
        {
            AvailableSources = new ObservableCollection<AppSourceModel>(AppSourceList.AvailableSources);
            AvailableCompletionStatuses = new ObservableCollection<AppCompletionStatusModel>(AppCompletionStatusList.AvailableCompletionStatuses);
        }

        // ============================================================
        // COMMANDS & POPULATE DATA
        // ============================================================
        public void LoadData(Guid appId = default)
        {
            // Ambil master items dari masing-masing repo
            var sectionsFromDb = _sectionRepo.GetAllSections();
            var seriesFromDb = _seriesRepo.GetAllSeries();
            var genresFromDb = _genreRepo.GetAllGenres();
            var tagsFromDb = _tagRepo.GetAllTags();
            var developersFromDb = _developerRepo.GetAllDevelopers();
            var publishersFromDb = _publisherRepo.GetAllPublishers();
            var platformsFromDb = _platformRepo.GetAllPlatforms();
            var ageRatingsFromDb = _ageRatingRepo.GetAllAgeRatings();

            AvailableSections = new ObservableCollection<SelectableItem>(
                sectionsFromDb.Select(n => new SelectableItem(n.Name, n.Id, n.Desc, false)));

            AvailableSeries = new ObservableCollection<SelectableItem>(
                seriesFromDb.Select(s => new SelectableItem(s.Name, s.Id, s.Desc, false)));

            AvailableGenres = new ObservableCollection<SelectableItem>(
                genresFromDb.Select(g => new SelectableItem(g.Name, g.Id, g.Desc, false)));

            AvailableTags = new ObservableCollection<SelectableItem>(
                tagsFromDb.Select(t => new SelectableItem(t.Name, t.Id, t.Desc, false)));

            AvailableDevelopers = new ObservableCollection<SelectableItem>(
                developersFromDb.Select(d => new SelectableItem(d.Name, d.Id, d.Desc, false)));

            AvailablePublishers = new ObservableCollection<SelectableItem>(
                publishersFromDb.Select(p => new SelectableItem(p.Name, p.Id, p.Desc, false)));

            AvailablePlatforms = new ObservableCollection<SelectableItem>(
                platformsFromDb.Select(l => new SelectableItem(l.Name, l.Id, l.Desc, false)));

            AvailableAgeRatings = new ObservableCollection<SelectableItem>(
                ageRatingsFromDb.Select(r => new SelectableItem(r.Name, r.Id, r.Desc, false)));
        }

        // ==========================================
        // 2. TRANSFER DATA (LOAD & POPULATE TO MODEL)
        // ==========================================

        public void LoadFromModel(AppModel app)
        {
            Title = app.Title ?? string.Empty;
            SortingTitle = app.SortingTitle ?? string.Empty;
            Description = app.Desc ?? string.Empty;
            Notes = app.Notes ?? string.Empty;
            UserRating = app.UserRating >= 0 ? app.UserRating.ToString() : string.Empty;
            ReleaseDate = app.ReleaseDate;

            // Mapping Dropdowns
            SelectedSource = AvailableSources.FirstOrDefault(s =>
                string.Equals(s.Id, app.Source, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(s.Name, app.Source, StringComparison.OrdinalIgnoreCase))
                ?? AvailableSources.FirstOrDefault();

            SelectedCompletionStatus = AvailableCompletionStatuses.FirstOrDefault(c =>
                string.Equals(c.Id, app.CompletionStatus, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Name, app.CompletionStatus, StringComparison.OrdinalIgnoreCase))
                ?? AvailableCompletionStatuses.FirstOrDefault(c => c.Id == "not_played")
                ?? AvailableCompletionStatuses.FirstOrDefault();

            // Mapping Switches
            IsNotGame = !app.IsGame;
            IsExcluded = app.IsExcluded;
            IsPinned = app.IsPinned;

            FirstLaunched = app.FirstLaunched?.ToString("yyyy-MM-dd HH:mm") ?? "-";
            LastLaunched = app.LastLaunched?.ToString("yyyy-MM-dd HH:mm") ?? "-";
            LaunchTimes = app.LaunchTimes.ToString();

            // Sinkronkan Checkbox & Text MultiSelect langsung dari AppModel
            SyncSelection(AvailableSections, app.SectionIds, t => SelectedSectionsText = t);
            SyncSelection(AvailableSeries, app.SeriesIds, t => SelectedSeriesText = t);
            SyncSelection(AvailableGenres, app.GenreIds, t => SelectedGenresText = t);
            SyncSelection(AvailableTags, app.TagIds, t => SelectedTagsText = t);
            SyncSelection(AvailableDevelopers, app.DeveloperIds, t => SelectedDevelopersText = t);
            SyncSelection(AvailablePublishers, app.PublisherIds, t => SelectedPublishersText = t);
            SyncSelection(AvailablePlatforms, app.PlatformIds, t => SelectedPlatformsText = t);
            SyncSelection(AvailableAgeRatings, app.AgeRatingIds, t => SelectedAgeRatingsText = t);
        }

        private void SyncSelection(ObservableCollection<SelectableItem> collection, IEnumerable<Guid> selectedIds, Action<string> setText)
        {
            var idSet = selectedIds.ToHashSet();
            foreach (var item in collection)
            {
                item.IsSelected = idSet.Contains(item.Id);
            }

            var selectedNames = collection.Where(x => x.IsSelected).Select(x => x.Name);
            setText(string.Join(", ", selectedNames));
        }

        public void ApplyToModel(AppModel app)
        {
            app.Title = Title;
            app.SortingTitle = string.IsNullOrWhiteSpace(SortingTitle) ? Title : SortingTitle;
            app.Desc = Description;
            app.Notes = Notes;
            app.UserRating = int.TryParse(UserRating, out int rating) ? rating : -1;
            app.ReleaseDate = ReleaseDate;

            app.Source = (SelectedSource?.Id == "empty") ? null : SelectedSource?.Id;
            app.CompletionStatus = (SelectedCompletionStatus?.Id == "empty")
                ? "not_played"
                : (SelectedCompletionStatus?.Id ?? "not_played");

            app.IsGame = !IsNotGame;
            app.IsExcluded = IsExcluded;
            app.IsPinned = IsPinned;

            // Masukkan list ID terpilih kembali ke objek model
            app.SectionIds = SelectedSectionIds;
            app.SeriesIds = SelectedSeriesIds;
            app.GenreIds = SelectedGenreIds;
            app.TagIds = SelectedTagIds;
            app.DeveloperIds = SelectedDeveloperIds;
            app.PublisherIds = SelectedPublisherIds;
            app.PlatformIds = SelectedPlatformIds;
            app.AgeRatingIds = SelectedAgeRatingIds;
        }

        public void PreselectSection(Guid sectionId)
        {
            if (sectionId == Guid.Empty) return;

            var matchedItem = AvailableSections.FirstOrDefault(s => s.Id == sectionId);
            if (matchedItem != null)
            {
                matchedItem.IsSelected = true;
            }

            var selectedNames = AvailableSections.Where(s => s.IsSelected).Select(s => s.Name);
            SelectedSectionsText = string.Join(", ", selectedNames);
        }

        // 🔑 FUNGSI GENERIK TERPUSAT: Menangani Insert DB + Update UI
        private void AddLookupItem<TModel>(
            string name,
            Func<TModel, Guid> insertDbFunc,
            Func<string, TModel> createModelFunc,
            ObservableCollection<SelectableItem> targetCollection,
            Action<string>? setTextAction = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return;

            // 1. Simpan ke database
            var model = createModelFunc(name);
            Guid newId = insertDbFunc(model);

            // 2. Tambah ke collection dan centang
            var newItem = new SelectableItem(name, newId, "", true);
            targetCollection.Add(newItem);

            // 3. Update tampilan string di TextBox input
            if (setTextAction != null)
            {
                var selectedNames = targetCollection.Where(x => x.IsSelected).Select(x => x.Name);
                setTextAction(string.Join(", ", selectedNames));
            }
        }

        [RelayCommand]
        private void AddSeries(string name) =>
            AddLookupItem(name, _seriesRepo.AddSeries, n => new SeriesModel { Name = n }, 
                            AvailableSeries, t => SelectedSeriesText = t);

        [RelayCommand]
        private void AddPlatform(string name) =>
            AddLookupItem(name, _platformRepo.AddPlatform, n => new PlatformModel { Name = n }, 
                            AvailablePlatforms, t => SelectedPlatformsText = t);

        [RelayCommand]
        private void AddDeveloper(string name) =>
            AddLookupItem(name, _developerRepo.AddDeveloper, n => new DeveloperModel { Name = n }, 
                            AvailableDevelopers, t => SelectedDevelopersText = t);

        [RelayCommand]
        private void AddPublisher(string name) =>
            AddLookupItem(name, _publisherRepo.AddPublisher, n => new PublisherModel { Name = n }, 
                            AvailablePublishers, t => SelectedPublishersText = t);

        [RelayCommand]
        private void AddAgeRating(string name) =>
            AddLookupItem(name, _ageRatingRepo.AddAgeRating, n => new AgeRatingModel { Name = n }, 
                            AvailableAgeRatings, t => SelectedAgeRatingsText = t);

        [RelayCommand]
        private void AddGenre(string name) =>
            AddLookupItem(name, _genreRepo.AddGenre, n => new GenreModel { Name = n }, AvailableGenres, 
                            t => SelectedGenresText = t);

        [RelayCommand]
        private void AddTag(string name) =>
            AddLookupItem(name, _tagRepo.AddTag, n => new TagModel { Name = n }, AvailableTags, 
                            t => SelectedTagsText = t);

        [RelayCommand]
        private void AddSection(string name)
        {
            AddLookupItem(name, _sectionRepo.AddSection, n => new SectionModel { Name = n }, AvailableSections, t => SelectedSectionsText = t);

            // BROADCAST KE MAINVIEWMODEL untuk refresh section tabs
            WeakReferenceMessenger.Default.Send(new SectionsUpdatedMessage());
        }

    }
}