using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using zennplay_launcher.Models;
using zennplay_launcher.Services;

namespace zennplay_launcher.ViewModels;

public partial class AppActionPropertiesViewModel : ViewModelBase
{
    // ============================================================
    // OBSERVABLE COLLECTIONS & DROPDOWNS
    // ============================================================
    [ObservableProperty] private ObservableCollection<StringPropertyModel> _availableActionTypes;
    [ObservableProperty] private StringPropertyModel? _selectedActionType;

    // ============================================================
    // FORM INPUT PROPERTIES
    // ============================================================
    [ObservableProperty] private Guid _id;
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _desc = string.Empty;
    [ObservableProperty] private string _target = string.Empty;
    [ObservableProperty] private string _launchOptions = string.Empty;
    [ObservableProperty] private string _iconPath = string.Empty;
    [ObservableProperty] private bool _isNotGame = false;

    // ============================================================
    // VARIABLES & DIALOG CONTROL
    // ============================================================
    private AppActionModel? _editingAction;
    public bool IsSaved { get; private set; } = false;
    public event Action? RequestClose;
    public IStorageProvider? StorageProvider { get; set; }

    public AppActionPropertiesViewModel()
    {
        AvailableActionTypes = new ObservableCollection<StringPropertyModel>(AppActionTypes.AvailableActionTypes);
    }

    [RelayCommand]
    private async Task BrowseTargetAsync()
    {
        if (StorageProvider == null) return;

        var result = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Action File Target",
            AllowMultiple = false
        });

        if (result.Count > 0)
        {
            Target = result[0].Path.LocalPath;
        }
    }

    // Memuat Data dari Model ke Form
    public void LoadFromModel(AppActionModel action)
    {
        _editingAction = action; // Simpan referensi objek asli

        Id = action.Id;
        Title = action.Title ?? string.Empty;
        Desc = action.Desc ?? string.Empty;
        Target = action.Target ?? string.Empty;
        LaunchOptions = action.LaunchOptions ?? string.Empty;
        IconPath = action.ActionIconPath ?? string.Empty;

        SelectedActionType = AvailableActionTypes.FirstOrDefault(t =>
            string.Equals(t.Id, action.ActionType, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(t.Name, action.ActionType, StringComparison.OrdinalIgnoreCase))
            ?? AvailableActionTypes.FirstOrDefault();

        IsNotGame = !action.IsGame;
    }

    // Menerapkan Nilai Form Kembali ke Model
    public void ApplyToModel(AppActionModel action)
    {
        action.Title = Title;
        action.Desc = Desc;
        action.Target = Target;
        action.LaunchOptions = LaunchOptions;
        action.ActionIconPath = IconPath;
        action.ActionType = SelectedActionType?.Id ?? "executable";
        action.IsGame = !IsNotGame;
    }

    // Command Save saat user klik tombol Save di jendela Edit Action
    [RelayCommand]
    private void Save()
    {
        if (_editingAction != null
            && !String.IsNullOrEmpty(Title)
            && !String.IsNullOrEmpty(Target))
        {
            ApplyToModel(_editingAction);

            IsSaved = true;
            RequestClose?.Invoke();
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        IsSaved = false;
        RequestClose?.Invoke();
    }
}