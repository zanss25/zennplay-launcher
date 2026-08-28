using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using zennplay_launcher.Models;
using zennplay_launcher.Services.Repositories;

namespace zennplay_launcher.Services
{
    public class AppEditorService
    {
        private readonly AppRepository _appRepo = new();

        public async Task<Guid> CreateDraftAsync(
            string appTitle = "New App",
            string appDir = "",
            string primaryActionTarget = "",
            Guid? sectionId = null)
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
                CompletionStatus = "not_played",
                CreatedAt = DateTime.UtcNow.ToString("o")
            };

            List<AppActionModel>? appActionList = null;

            if (!string.IsNullOrWhiteSpace(primaryActionTarget))
            {
                string primaryActionType = DetectTypeFromExtension.DetectFileType(primaryActionTarget);
                string fileName = Path.GetFileNameWithoutExtension(primaryActionTarget);
                string primaryActionTitle = string.IsNullOrWhiteSpace(fileName) ? "Launch Game" : fileName;

                appActionList = new List<AppActionModel>
                {
                    new AppActionModel
                    {
                        Id = Guid.NewGuid(),
                        AppId = newAppId,
                        Title = primaryActionTitle,
                        Target = primaryActionTarget,
                        ActionType = primaryActionType
                    }
                };
            }

            // Siapkan relasi Section jika diberikan
            List<Guid>? sectionIds = null;
            if (sectionId.HasValue && sectionId.Value != Guid.Empty)
            {
                sectionIds = new List<Guid> { sectionId.Value };
            }

            // Simpan ke Repository lengkap dengan Action dan Section
            await _appRepo.AddAppAsync(appDraft, actions: appActionList, sectionIds: sectionIds);

            return newAppId;
        }
    }
}