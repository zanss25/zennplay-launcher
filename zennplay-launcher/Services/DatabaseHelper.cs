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
            string configDataPath = Path.Combine(GetDatabaseDir(), "config_data.db");
            return configDataPath;
        }

        public static string GetAssetsDataPath()
        {
            string assetsFolderPath = Path.Combine(GetDatabaseDir(), "user_assets");

            if (!Directory.Exists(assetsFolderPath))
            {
                Directory.CreateDirectory(assetsFolderPath);
            }

            return assetsFolderPath;
        }

        public static string GetThemesDataPath()
        {
            string themesFolderPath = Path.Combine(GetDatabaseDir(), "user_assets");

            if (!Directory.Exists(themesFolderPath))
            {
                Directory.CreateDirectory(themesFolderPath);
            }

            return themesFolderPath;
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
    }
}
