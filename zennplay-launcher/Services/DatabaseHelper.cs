using Microsoft.Data.Sqlite;
using Dapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Services
{
    public static class DatabaseHelper
    {


        /// <summary>
        /// Mendapatkan path lengkap direktori dan file SQLite di %AppData%/ZennPlay/zennplay.db
        /// (Cross-platform: Otomatis menyesuaikan Windows & Linux)
        /// </summary>


        /// <summary>
        /// Fungsi Generik untuk membuat dan membuka koneksi SQLite yang siap pakai.
        /// </summary>
        /// <returns>SqliteConnection yang sudah dalam keadaan Open</returns>
        /// 

        public static string GetDatabaseDir()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string zennFolderPath = Path.Combine(appDataFolder, "ZennPlay");

            if (!Directory.Exists(zennFolderPath))
            {
                Directory.CreateDirectory(zennFolderPath);
            }

            return zennFolderPath;
        }

        public static string GetLibraryDataPath()
        {
            string libraryDataPath = Path.Combine(GetDatabaseDir(), "library_data.db");
            return libraryDataPath;
        }

        public static string GetConfigDataPath()
        {
            string configDataPath = Path.Combine(GetDatabaseDir(), "settings.json");
            return configDataPath;
        }

        public static string GetOverridesDataPath()
        {
            string overridesDataPath = Path.Combine(GetDatabaseDir(), "overrides.json");
            return overridesDataPath;
        }

        public static string GetAssetsDataPath()
        {
            string assetsFolderPath = Path.Combine(GetDatabaseDir(), "UserAssets");

            if (!Directory.Exists(assetsFolderPath))
            {
                Directory.CreateDirectory(assetsFolderPath);
            }

            return assetsFolderPath;
        }

        public static string GetIconAssetsDataPath()
        {
            string dir = GetAssetsDataPath();
            string iconsFolderPath = Path.Combine(dir, "Icons");

            if (!Directory.Exists(iconsFolderPath))
            {
                Directory.CreateDirectory(iconsFolderPath);
            }

            return iconsFolderPath;
        }

        public static string GetCoverAssetsDataPath()
        {
            string dir = GetAssetsDataPath();
            string coversFolderPath = Path.Combine(dir, "Cover");

            if (!Directory.Exists(coversFolderPath))
            {
                Directory.CreateDirectory(coversFolderPath);
            }

            return coversFolderPath;
        }

        public static string GetBackgroundAssetsDataPath()
        {
            string dir = GetAssetsDataPath();
            string backgroundFolderPath = Path.Combine(dir, "Background");

            if (!Directory.Exists(backgroundFolderPath))
            {
                Directory.CreateDirectory(backgroundFolderPath);
            }

            return backgroundFolderPath;
        }

        public static string GetSectionIconsDataPath()
        {
            string dir = GetAssetsDataPath();
            string sectionIconsFolderPath = Path.Combine(dir, "SectionIcons");

            if (!Directory.Exists(sectionIconsFolderPath))
            {
                Directory.CreateDirectory(sectionIconsFolderPath);
            }

            return sectionIconsFolderPath;
        }

        public static string GetThemesDataPath()
        {
            string themesFolderPath = Path.Combine(GetDatabaseDir(), "CustomThemes");

            if (!Directory.Exists(themesFolderPath))
            {
                Directory.CreateDirectory(themesFolderPath);
            }

            return themesFolderPath;
        }

        private static bool IsTableEmpty(SqliteConnection connection, string tableName)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM {tableName}";
            return Convert.ToInt32(cmd.ExecuteScalar()) == 0;
        }

        public static bool IsColumnExists(string conn, string tableName, string columnName)
        {
            bool exists = false;
            using (var connection = new SqliteConnection(conn))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"PRAGMA table_info({tableName})";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string currentColumnName = reader.GetString(1);
                        if (currentColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                        {
                            exists = true;
                            break;
                        }
                    }
                }
            }
            return exists;
        }

        public static SqliteConnection GetOpenConnection()
        {
            string dbPath = GetLibraryDataPath();

            var connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared
            };

            var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);

            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();
            }

            return connection;
        }

        public static void CloseAllConnections()
        {
            SqliteConnection.ClearAllPools();

            GC.Collect();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

    }
}
