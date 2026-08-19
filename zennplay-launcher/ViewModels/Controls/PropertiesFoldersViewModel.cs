using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Services.Repositories;

namespace zennplay_launcher.ViewModels.Controls
{
    public partial class PropertiesFoldersViewModel : ViewModelBase
    {
        // ============================================================
        // VARIABLES
        // ============================================================

        // Repositories
        private readonly AppRepository _appRepo= new();
        private readonly SectionRepository _sectionRepo = new();

        public IStorageProvider? StorageProvider { get; set; }

        // Directories List
        // 1. Installation Directory Data
        [ObservableProperty]
        private string _installationFolder = "";

        // Data
        // 1. Install Size
        [ObservableProperty]
        private string _installSizeText = "5.7 GB";

        public PropertiesFoldersViewModel()
        {
            
        }

        // ============================================================
        // COMMANDS
        // ============================================================

        [RelayCommand]
        private async Task BrowseFolderAsync()
        {
            if (StorageProvider == null) return;

            var result = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Installation Directory",
                AllowMultiple = false
            });

            if (result.Count > 0)
            {
                // Ambil path lokal filesystem
                InstallationFolder = result[0].Path.LocalPath;
            }
        }

    }
}
