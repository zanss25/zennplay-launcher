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
                Title = "Select Application, Game, or Shortcut",
                AllowMultiple = false,
                FileTypeFilter = platformService.GetExecutableFileFilters()
            });

            if (files.Count == 0)
                return null;

            string selectedPath = files[0].Path.LocalPath;

            if (!platformService.IsExecutable(selectedPath))
                return null;

            // 1. Resolusi shortcut (.lnk/.desktop), ekstrak path asli, title, dan working dir
            var (title, resolvedTargetPath, workingDir) = platformService.ResolveAppMetadata(selectedPath);

            // 2. Deteksi Root Directory untuk kalkulasi size instalasi
            string rootGameDir = platformService.DetectRootGameDirectory(resolvedTargetPath, workingDir);

            // 3. Simpan ke Draft
            Guid newAppId = _appEditorService.CreateDraft(
                appTitle: title,
                appDir: rootGameDir,
                primaryActionTarget: resolvedTargetPath
            );

            return newAppId;
        }

    }
}
