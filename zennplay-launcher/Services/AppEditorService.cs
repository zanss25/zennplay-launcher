using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using zennplay_launcher.Models;
using zennplay_launcher.Services.Platforms;
using zennplay_launcher.Services.Repositories;

namespace zennplay_launcher.Services
{
    public class AppEditorService
    {
        private readonly AppRepository _appRepo = new();

        public async Task<Guid> CreateAppAsync(
                                    string appTitle = "New App",
                                    string appDir = "",
                                    string primaryActionTarget = "",
                                    string? iconSourcePath = null,
                                    Guid? sectionId = null)
        {
            Guid newAppId = Guid.NewGuid();
            string appAssetsDirectory = Path.Combine(DatabaseHelper.GetAssetsDataPath(), newAppId.ToString());

            // Menentukan sumber icon untuk App utama:
            string? sourceForAppIcon = (!string.IsNullOrWhiteSpace(iconSourcePath) && File.Exists(iconSourcePath))
                ? iconSourcePath
                : primaryActionTarget;

            string? savedAppIconPath = IconExtractor.ExtractAndSaveIconToStorage(
                newAppId,
                sourceForAppIcon,
                appAssetsDirectory
            );

            var appDraft = new AppModel
            {
                Id = newAppId,
                Title = appTitle,
                AppDir = appDir,
                IconPath = savedAppIconPath,
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

                // 2. Icon Action SELALU diekstrak dari executable target aksinya langsung
                string actionsDir = DatabaseHelper.GetActionAssetsDataPath(newAppId);
                string? savedActionIconPath = IconExtractor.ExtractAndSaveIconToStorage(
                    newAppId,
                    primaryActionTarget,
                    actionsDir
                );

                appActionList = new List<AppActionModel>
        {
            new AppActionModel
            {
                Id = Guid.NewGuid(),
                AppId = newAppId,
                Title = primaryActionTitle,
                Target = primaryActionTarget,
                ActionIconPath = savedActionIconPath,
                ActionType = primaryActionType
            }
        };
            }

            List<Guid>? sectionIds = (sectionId.HasValue && sectionId.Value != Guid.Empty)
                ? new List<Guid> { sectionId.Value }
                : null;

            try
            {
                await _appRepo.AddAppAsync(appDraft, actions: appActionList, sectionIds: sectionIds);
            }
            catch
            {
                if (Directory.Exists(appAssetsDirectory))
                {
                    try { Directory.Delete(appAssetsDirectory, recursive: true); } catch { }
                }
                throw;
            }

            return newAppId;
        }


    }
}