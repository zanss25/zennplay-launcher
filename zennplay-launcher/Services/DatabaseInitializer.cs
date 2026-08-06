using Microsoft.Data.Sqlite;
using System;

namespace zennplay_launcher.Services
{
    public class DatabaseInitializer
    {
        public void Initialize()
        {
            // 1. Buat Semua Tabel Database
            GenerateAppsTable();
            GenerateAppLinksTable();
            GenerateAppActionsTable();
            GenerateLaunchSessionsTable();
            GenerateGenresTable();
            GenerateAppGenresTable();
            GenerateTagsTable();
            GenerateAppTagsTable();
            GenerateSectionsTable();
            GenerateAppSectionsTable();

            // 2. Isi Data Awal (Seed Data)
            SeedDefaultGenres();
            SeedDefaultTags();
            SeedDefaultSections();
        }

        #region DDL Table Generators

        public void GenerateAppsTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS apps (
                                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            guid TEXT NOT NULL UNIQUE,
                                            title TEXT NOT NULL,
                                            sorting_title TEXT,
                                            desc TEXT,
                                            user_rating INTEGER DEFAULT -1,
                                            is_favorite INTEGER DEFAULT 0,
                                            is_pinned INTEGER DEFAULT 0,
                                            series TEXT,
                                            notes TEXT,
                                            first_launched TEXT,
                                            last_launched TEXT,
                                            launch_times INTEGER DEFAULT 0,
                                            completion_status TEXT DEFAULT 'Not Completed',
                                            developer TEXT,
                                            publisher TEXT,
                                            release_date TEXT,
                                            age_rating TEXT,
                                            is_installed INTEGER DEFAULT 1,
                                            is_game INTEGER DEFAULT NULL,
                                            is_excluded INTEGER DEFAULT NULL,
                                            version TEXT,
                                            app_dir TEXT,
                                            source_platform TEXT,
                                            external_id TEXT,
                                            cover_path TEXT,
                                            background_path TEXT,
                                            icon_path TEXT,
                                            created_at TEXT DEFAULT (datetime('now', 'localtime')),
                                            is_deleted INTEGER DEFAULT 0
                                            );";
                command.ExecuteNonQuery();
            }
        }

        public void GenerateAppLinksTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS app_links (
                                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            app_id INTEGER NOT NULL REFERENCES apps(id) ON DELETE CASCADE,
                                            name TEXT,
                                            url TEXT NOT NULL
                                            );";
                command.ExecuteNonQuery();
            }
        }

        public void GenerateAppActionsTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS app_actions (
                                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            app_id INTEGER NOT NULL REFERENCES apps(id) ON DELETE CASCADE,
                                            title TEXT NOT NULL,
                                            desc TEXT,
                                            action_type TEXT NOT NULL,
                                            target TEXT NOT NULL,
                                            launch_options TEXT,
                                            action_icon_path TEXT,
                                            is_primary INTEGER DEFAULT 0
                                            );";
                command.ExecuteNonQuery();
            }
        }

        public void GenerateLaunchSessionsTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS launch_sessions (
                                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            app_id INTEGER NOT NULL REFERENCES apps(id) ON DELETE CASCADE,
                                            session_start TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
                                            session_end TEXT,
                                            duration_seconds INTEGER
                                            );";
                command.ExecuteNonQuery();
            }
        }

        public void GenerateGenresTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS genres (
                                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            name TEXT NOT NULL UNIQUE,
                                            desc TEXT,
                                            created_at TEXT DEFAULT (datetime('now', 'localtime'))
                                            );";
                command.ExecuteNonQuery();
            }
        }

        public void GenerateAppGenresTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS app_genres (
                                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            app_id INTEGER NOT NULL REFERENCES apps(id) ON DELETE CASCADE,
                                            genre_id INTEGER NOT NULL REFERENCES genres(id) ON DELETE CASCADE,
                                            UNIQUE(app_id, genre_id)
                                            );";
                command.ExecuteNonQuery();
            }
        }

        public void GenerateTagsTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS tags (
                                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            name TEXT NOT NULL UNIQUE,
                                            desc TEXT,
                                            created_at TEXT DEFAULT (datetime('now', 'localtime'))
                                            );";
                command.ExecuteNonQuery();
            }
        }

        public void GenerateAppTagsTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS app_tags (
                                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            app_id INTEGER NOT NULL REFERENCES apps(id) ON DELETE CASCADE,
                                            tag_id INTEGER NOT NULL REFERENCES tags(id) ON DELETE CASCADE,
                                            UNIQUE(app_id, tag_id)
                                            );";
                command.ExecuteNonQuery();
            }
        }

        public void GenerateSectionsTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS sections (
                                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            name TEXT NOT NULL UNIQUE,
                                            desc TEXT,
                                            icon TEXT,
                                            is_game_by_default INTEGER DEFAULT 1,
                                            is_excluded_by_default INTEGER DEFAULT 0,
                                            created_at TEXT DEFAULT (datetime('now', 'localtime'))
                                            );";
                command.ExecuteNonQuery();
            }
        }

        public void GenerateAppSectionsTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS app_sections (
                                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            app_id INTEGER NOT NULL REFERENCES apps(id) ON DELETE CASCADE,
                                            section_id INTEGER NOT NULL REFERENCES sections(id) ON DELETE CASCADE,
                                            UNIQUE(app_id, section_id)
                                            );";
                command.ExecuteNonQuery();
            }
        }

        #endregion

        #region Seed Data Methods (Memasukkan Data Awal)

        private void SeedDefaultGenres()
        {
            string[] defaultGenres = new string[]
            {
                "Action", "Adventure", "Role-Playing (RPG)", "Strategy",
                "Simulation", "Shooter", "Puzzle", "Platformer",
                "Racing", "Sports", "Fighting", "Survival",
                "Horror", "Visual Novel", "Casual", "Card & Board"
            };

            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                // Gunakan INSERT OR IGNORE agar tidak bentrok jika genre sudah ada
                command.CommandText = "INSERT OR IGNORE INTO genres (name) VALUES (@name);";
                var param = command.CreateParameter();
                param.ParameterName = "@name";
                command.Parameters.Add(param);

                foreach (var genreName in defaultGenres)
                {
                    param.Value = genreName;
                    command.ExecuteNonQuery();
                }
            }
            Console.WriteLine("Default genres seeded successfully.");
        }

        private void SeedDefaultTags()
        {
            string[] defaultTags = new string[]
            {
                "Singleplayer", "Multiplayer", "Co-op", "Open World",
                "Story Rich", "Atmospheric", "Moddable", "Pixel Art",
                "Indie", "Turn-Based", "Real-Time", "VR", "Anime"
            };

            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INSERT OR IGNORE INTO tags (name) VALUES (@name);";
                var param = command.CreateParameter();
                param.ParameterName = "@name";
                command.Parameters.Add(param);

                foreach (var tagName in defaultTags)
                {
                    param.Value = tagName;
                    command.ExecuteNonQuery();
                }
            }
            Console.WriteLine("Default tags seeded successfully.");
        }

        private void SeedDefaultSections()
        {
            // Menyiapkan 3 Section Bawaan ZennPlay
            var defaultSections = new[]
            {
                new { Name = "Games", Desc = "Main game collection", Icon = "gamepad", IsGame = 1, IsExcluded = 0 },
                new { Name = "Tools", Desc = "Utilities, Mod Organizers, and Tweakers", Icon = "wrench", IsGame = 0, IsExcluded = 0 },
                new { Name = "Emulators", Desc = "Console and handheld emulators", Icon = "cpu", IsGame = 1, IsExcluded = 0 }
            };

            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"INSERT OR IGNORE INTO sections (name, desc, icon, is_game_by_default, is_excluded_by_default) 
                                        VALUES (@name, @desc, @icon, @isGame, @isExcluded);";

                var nameParam = command.Parameters.Add("@name", SqliteType.Text);
                var descParam = command.Parameters.Add("@desc", SqliteType.Text);
                var iconParam = command.Parameters.Add("@icon", SqliteType.Text);
                var isGameParam = command.Parameters.Add("@isGame", SqliteType.Integer);
                var isExcludedParam = command.Parameters.Add("@isExcluded", SqliteType.Integer);

                foreach (var sec in defaultSections)
                {
                    nameParam.Value = sec.Name;
                    descParam.Value = sec.Desc;
                    iconParam.Value = sec.Icon;
                    isGameParam.Value = sec.IsGame;
                    isExcludedParam.Value = sec.IsExcluded;

                    command.ExecuteNonQuery();
                }
            }
            Console.WriteLine("Default sections seeded successfully.");
        }

        #endregion
    }
}