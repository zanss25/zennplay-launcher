using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services
{
    public static class OverridesServices
    {
        // ============================================================
        // OPTIONS
        // ============================================================
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // ============================================================
        // CONFIG OVERRIDES FUNCTIONS
        // ============================================================
        public static UserOverridesModel LoadAllOverrides()
        {
            string overridesPath = DatabaseHelper.GetOverridesDataPath();
            if (!File.Exists(overridesPath)) return new UserOverridesModel();

            try
            {
                string json = File.ReadAllText(overridesPath);
                return JsonSerializer.Deserialize<UserOverridesModel>(json, _jsonOptions)
                       ?? new UserOverridesModel();
            }
            catch
            {
                return new UserOverridesModel();
            }
        }

        // --- METHOD KHUSUS SAVE APP OVERRIDE ---
        public static void SaveAppOverride(int appId, AppOverridesModel appSettings)
        {
            var allData = LoadAllOverrides();
            allData.AppOverrides[appId.ToString()] = appSettings;
            SaveToFile(allData);
        }

        // --- METHOD KHUSUS SAVE SECTION OVERRIDE ---
        public static void SaveSectionOverride(int sectionId, SectionOverridesModel sectionSettings)
        {
            var allData = LoadAllOverrides();
            allData.SectionOverrides[sectionId.ToString()] = sectionSettings;
            SaveToFile(allData);
        }

        private static void SaveToFile(UserOverridesModel data)
        {
            string overridesPath = DatabaseHelper.GetOverridesDataPath();
            string? directoryPath = Path.GetDirectoryName(overridesPath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string jsonString = JsonSerializer.Serialize(data, _jsonOptions);
            File.WriteAllText(overridesPath, jsonString);
        }

        public static void RemoveAppOverride(int appId)
        {
            var allData = LoadAllOverrides();

            // Hapus key App ID jika ada
            if (allData.AppOverrides.Remove(appId.ToString()))
            {
                SaveToFile(allData);
            }
        }

        public static void RemoveSectionOverride(int appId)
        {
            var allData = LoadAllOverrides();

            // Hapus key App ID jika ada
            if (allData.SectionOverrides.Remove(appId.ToString()))
            {
                SaveToFile(allData);
            }
        }

    }
}
