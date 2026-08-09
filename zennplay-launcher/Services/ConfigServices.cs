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
    public static class ConfigServices
    {
        // ============================================================
        // OPTIONS
        // ============================================================
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        // ============================================================
        // MAIN CONFIG FUNCTIONS
        // ============================================================
        public static void SaveConfig(UserConfigModel configData)
        {
            try
            {
                string configPath = DatabaseHelper.GetConfigDataPath();

                string? directoryPath = Path.GetDirectoryName(configPath);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string jsonString = JsonSerializer.Serialize(configData, _jsonOptions);
                File.WriteAllText(configPath, jsonString);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Saving Config: {ex.Message}");
            }
        }

        public static UserConfigModel LoadConfig()
        {
            string configPath = DatabaseHelper.GetConfigDataPath();

            if (!File.Exists(configPath))
            {
                var defaultConfig = new UserConfigModel();
                SaveConfig(defaultConfig);
                return defaultConfig;
            }

            try
            {
                string json = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<UserConfigModel>(json, _jsonOptions);

                return config ?? new UserConfigModel();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Loading Config (Corrupted JSON?): {ex.Message}");

                var fallbackConfig = new UserConfigModel();
                SaveConfig(fallbackConfig);
                return fallbackConfig;
            }
        }

        public static void CreateDefaultConfig()
        {
            var defaultConfig = new UserConfigModel();

            string defaultConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "default_config.json");

            if (!File.Exists(defaultConfigPath))
            {
                string jsonString = JsonSerializer.Serialize(defaultConfig, _jsonOptions);
                File.WriteAllText(defaultConfigPath, jsonString);
            }
        }

    }
}
