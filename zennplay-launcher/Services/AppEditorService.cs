using Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Models;
using zennplay_launcher.Services.Repositories;

namespace zennplay_launcher.Services
{
    public class AppEditorService
    {
        // Repositories
        private readonly AppRepository appRepo = new();

        public Guid CreateDraft(string appTitle = "New App",
                         string appDir = "",
                         string primaryActionTarget = "")
        {
            Guid newAppId = Guid.NewGuid();
            var appDraft = new AppModel
            {
                Id = newAppId,
                Title = appTitle,
                AppDir = appDir,
                IsGame = true,
                IsExcluded = false,
                IsPinned = false,
                CompletionStatus = "Not Played",
                CreatedAt = DateTime.UtcNow.ToString("o")
            };

            AppActionModel? primaryAppAction = null;

            if (!string.IsNullOrWhiteSpace(primaryActionTarget))
            {
                string extension = Path.GetExtension(primaryActionTarget).ToLowerInvariant();
                string primaryActionType = DetectTypeFromExtension.DetectFileType(primaryActionTarget);

                // Ambil nama file tanpa ekstensi untuk dijadikan judul Action (misal: "Dragon Age")
                string fileName = Path.GetFileNameWithoutExtension(primaryActionTarget);
                string primaryActionTitle = string.IsNullOrWhiteSpace(fileName) ? "Launch Game" : fileName;

                primaryAppAction = new AppActionModel
                {
                    Id = Guid.NewGuid(),
                    AppId = newAppId,
                    Title = primaryActionTitle,
                    Target = primaryActionTarget,
                    ActionType = primaryActionType
                };
            }

            // Simpan ke Repository
            if (primaryAppAction != null)
            {
                var appActionList = new List<AppActionModel> { primaryAppAction };
                appRepo.AddApp(appDraft, appActionList);
            }
            else
            {
                appRepo.AddApp(appDraft);
            }

            return newAppId;
        }

    }
}
