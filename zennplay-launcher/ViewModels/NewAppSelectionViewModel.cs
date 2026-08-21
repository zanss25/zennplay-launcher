using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Services;

namespace zennplay_launcher.ViewModels
{
    public class NewAppSelectionViewModel : ViewModelBase
    {
        // Instances
        private readonly AppEditorService _appEditorService = new();

        public NewAppSelectionViewModel()
        {

        }

        public async Task<Guid?> ManualAppBrowseAsync(IStorageProvider storageProvider)
        {
            var platformService = App.PlatformService;

            var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Application / Game",
                AllowMultiple = false,
                FileTypeFilter = platformService.GetExecutableFileFilters()
            });

            // Jika user klik Cancel di file picker
            if (files.Count == 0)
                return null;

            string selectedPath = files[0].Path.LocalPath;

            if (!platformService.IsExecutable(selectedPath))
            {
                return null;
            }

            var (title, directory) = platformService.ResolveAppMetadata(selectedPath);

            // Buat draft dan kembalikan ID barunya
            Guid newAppId = _appEditorService.CreateDraft(
                appTitle: title,
                appDir: directory,
                primaryActionTarget: selectedPath
            );

            return newAppId;
        }

    }
}
