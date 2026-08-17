using Microsoft.Data.Sqlite;
using System;

namespace zennplay_launcher.Services
{
    public class DatabaseInitializer
    {
        public void Initialize()
        {
            // 1. Buat Semua Tabel Database
            GenerateMetadataTable();
            GenerateAppsTable();
            GenerateAppLinksTable();
            GenerateAppActionsTable();
            GenerateLaunchSessionsTable();
            GenerateSeriesTable();
            GenerateAppSeriesTable();
            GenerateGenresTable();
            GenerateAppGenresTable();
            GenerateTagsTable();
            GenerateAppTagsTable();
            GenerateSectionsTable();
            GenerateAppSectionsTable();
            GenerateDevelopersTable();
            GenerateAppDevelopersTable();
            GeneratePublishersTable();
            GenerateAppPublishersTable();
            GeneratePlatformsTable();
            GenerateAppPlatformsTable();
            GenerateAgeRatingsTable();

            // 2. Isi Data Awal (Seed Data)
            SeedDefaultSeries();
            SeedDefaultGenres();
            SeedDefaultTags();
            SeedDefaultSections();
            SeedDefaultDevelopers();
            SeedDefaultPublishers();
            SeedDefaultPlatforms();
            SeedDefaultAgeRatings();
        }

        #region DDL Table Generators

        public void GenerateMetadataTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS db_metadata (
                                        key TEXT PRIMARY KEY,
                                        value TEXT NOT NULL
                                        );";
                command.ExecuteNonQuery();
            }
        }

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
                                            notes TEXT,
                                            first_launched TEXT,
                                            last_launched TEXT,
                                            launch_times INTEGER DEFAULT 0,
                                            completion_status TEXT DEFAULT 'Not Completed',
                                            release_date TEXT,
                                            age_rating TEXT,
                                            is_installed INTEGER DEFAULT 1,
                                            is_game INTEGER DEFAULT NULL,
                                            is_excluded INTEGER DEFAULT NULL,
                                            version TEXT,
                                            app_dir TEXT,
                                            source TEXT,
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
                                            is_game INTEGER DEFAULT 1,
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

        public void GenerateSeriesTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS series (
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    name TEXT NOT NULL UNIQUE,
                                    desc TEXT,
                                    created_at TEXT DEFAULT (datetime('now', 'localtime'))
                                );";
                command.ExecuteNonQuery();
            }
        }

        public void GenerateAppSeriesTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS app_series (
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    app_id INTEGER NOT NULL REFERENCES apps(id) ON DELETE CASCADE,
                                    series_id INTEGER NOT NULL REFERENCES series(id) ON DELETE CASCADE,
                                    UNIQUE(app_id, series_id)
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
                                            app_type TEXT,
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

        public void GenerateDevelopersTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS developers (
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    name TEXT NOT NULL UNIQUE,
                                    desc TEXT,
                                    created_at TEXT DEFAULT (datetime('now', 'localtime'))
                                );";
                command.ExecuteNonQuery();
            }
        }

        public void GenerateAppDevelopersTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS app_developers (
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    app_id INTEGER NOT NULL REFERENCES apps(id) ON DELETE CASCADE,
                                    developer_id INTEGER NOT NULL REFERENCES developers(id) ON DELETE CASCADE,
                                    UNIQUE(app_id, developer_id)
                                );";
                command.ExecuteNonQuery();
            }
        }

        public void GeneratePublishersTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS publishers (
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    name TEXT NOT NULL UNIQUE,
                                    desc TEXT,
                                    created_at TEXT DEFAULT (datetime('now', 'localtime'))
                                );";
                command.ExecuteNonQuery();
            }
        }

        public void GenerateAppPublishersTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS app_publishers (
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    app_id INTEGER NOT NULL REFERENCES apps(id) ON DELETE CASCADE,
                                    publisher_id INTEGER NOT NULL REFERENCES publishers(id) ON DELETE CASCADE,
                                    UNIQUE(app_id, publisher_id)
                                );";
                command.ExecuteNonQuery();
            }
        }

        public void GeneratePlatformsTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS platforms (
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    name TEXT NOT NULL UNIQUE,
                                    desc TEXT,
                                    created_at TEXT DEFAULT (datetime('now', 'localtime'))
                                );";
                command.ExecuteNonQuery();
            }
        }

        public void GenerateAppPlatformsTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS app_platforms (
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    app_id INTEGER NOT NULL REFERENCES apps(id) ON DELETE CASCADE,
                                    platform_id INTEGER NOT NULL REFERENCES platforms(id) ON DELETE CASCADE,
                                    UNIQUE(app_id, platform_id)
                                );";
                command.ExecuteNonQuery();
            }
        }

        public void GenerateAgeRatingsTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS age_ratings (
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    name TEXT NOT NULL UNIQUE,
                                    desc TEXT,
                                    created_at TEXT DEFAULT (datetime('now', 'localtime'))
                                );";
                command.ExecuteNonQuery();
            }
        }

        public void GenerateAppAgeRatingsTable()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"CREATE TABLE IF NOT EXISTS app_age_ratings (
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    app_id INTEGER NOT NULL REFERENCES apps(id) ON DELETE CASCADE,
                                    age_rating_id INTEGER NOT NULL REFERENCES age_ratings(id) ON DELETE CASCADE,
                                    UNIQUE(app_id, age_rating_id)
                                );";
                command.ExecuteNonQuery();
            }
        }

        #endregion

        #region Seed Data Methods (Memasukkan Data Awal)

        private void SeedDefaultSeries()
        {
            string[] defaultSeries = new string[]
            {
        "Grand Theft Auto", "The Witcher", "Final Fantasy", "Resident Evil",
        "Assassin's Creed", "Call of Duty", "Dark Souls", "The Elder Scrolls",
        "Fallout", "Monster Hunter", "Persona", "Far Cry",
        "Need for Speed", "Yakuza / Like a Dragon", "Tomb Raider", "Half-Life"
            };

            using (var connection = DatabaseHelper.GetOpenConnection())
            {
                using (var checkCmd = connection.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT COUNT(*) FROM db_metadata WHERE key = 'seed_series_v1';";
                    long count = (long)checkCmd.ExecuteScalar()!;
                    if (count > 0) return;
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT OR IGNORE INTO series (name) VALUES (@name);";
                    var param = command.CreateParameter();
                    param.ParameterName = "@name";
                    command.Parameters.Add(param);

                    foreach (var seriesName in defaultSeries)
                    {
                        param.Value = seriesName;
                        command.ExecuteNonQuery();
                    }
                }

                using (var markCmd = connection.CreateCommand())
                {
                    markCmd.CommandText = "INSERT INTO db_metadata (key, value) VALUES ('seed_series_v1', 'completed');";
                    markCmd.ExecuteNonQuery();
                }
            }
            Console.WriteLine("Default series seeded successfully.");
        }

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
            {
                using (var checkCmd = connection.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT COUNT(*) FROM db_metadata WHERE key = 'seed_genres_v1';";
                    long count = (long)checkCmd.ExecuteScalar()!;

                    // Jika penanda sudah ada, batalkan seeding! 
                    // Meskipun tabel 'tags' sekarang kosong karena dihapus player.
                    if (count > 0) return;
                }

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

                using (var markCmd = connection.CreateCommand())
                {
                    markCmd.CommandText = "INSERT INTO db_metadata (key, value) VALUES ('seed_genres_v1', 'completed');";
                    markCmd.ExecuteNonQuery();
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
            {
                using (var checkCmd = connection.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT COUNT(*) FROM db_metadata WHERE key = 'seed_tags_v1';";
                    long count = (long)checkCmd.ExecuteScalar()!;

                    // Jika penanda sudah ada, batalkan seeding! 
                    // Meskipun tabel 'tags' sekarang kosong karena dihapus player.
                    if (count > 0) return;
                }

                using (var command = connection.CreateCommand())
                {
                    // Gunakan INSERT OR IGNORE agar tidak bentrok jika genre sudah ada
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

                using (var markCmd = connection.CreateCommand())
                {
                    markCmd.CommandText = "INSERT INTO db_metadata (key, value) VALUES ('seed_tags_v1', 'completed');";
                    markCmd.ExecuteNonQuery();
                }
            }
            Console.WriteLine("Default tags seeded successfully.");
        }

        private void SeedDefaultSections()
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            {
                // 1. Cek apakah seeding section v1 sudah pernah dijalankan sebelumnya
                using (var checkCmd = connection.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT COUNT(*) FROM db_metadata WHERE key = 'seed_sections_v1';";
                    long count = (long)checkCmd.ExecuteScalar()!;

                    // JIKA SUDAH PERNAH DI-SEED, LANGSUNG HENTIKAN (Meskipun dipanggil 100x oleh UI)
                    if (count > 0) return;
                }

                // 2. Eksekusi Insert Section Bawaan jika DB benar-benar baru
                var defaultSections = new[]
                {
                    new { Name = "Games", Desc = "Main game collection", Icon = "gamepad", AppType="Game", IsGame = 1, IsExcluded = 0 },
                    new { Name = "Tools", Desc = "Utilities, Mod Organizers, and Tweakers", Icon = "wrench", AppType="Tool", IsGame = 0, IsExcluded = 1 },
                    new { Name = "Emulators", Desc = "Console and handheld emulators", Icon = "cpu", AppType = "Emulator", IsGame = 0, IsExcluded = 1 }
        };

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"INSERT OR IGNORE INTO sections (name, desc, icon, app_type, is_game_by_default, is_excluded_by_default) 
                                    VALUES (@name, @desc, @icon, @appType, @isGameByDefault, @isExcludedByDefault);";

                    var nameParam = command.Parameters.Add("@name", SqliteType.Text);
                    var descParam = command.Parameters.Add("@desc", SqliteType.Text);
                    var iconParam = command.Parameters.Add("@icon", SqliteType.Text);
                    var appTypeParam = command.Parameters.Add("@appType", SqliteType.Text);
                    var isGameParam = command.Parameters.Add("@isGameByDefault", SqliteType.Integer);
                    var isExcludedParam = command.Parameters.Add("@isExcludedByDefault", SqliteType.Integer);

                    foreach (var sec in defaultSections)
                    {
                        nameParam.Value = sec.Name;
                        descParam.Value = sec.Desc;
                        iconParam.Value = sec.Icon;
                        appTypeParam.Value = sec.AppType;
                        isGameParam.Value = sec.IsGame;
                        isExcludedParam.Value = sec.IsExcluded;

                        command.ExecuteNonQuery();
                    }
                }

                // 3. Catat di db_metadata bahwa seeding selesai
                using (var markCmd = connection.CreateCommand())
                {
                    markCmd.CommandText = "INSERT INTO db_metadata (key, value) VALUES ('seed_sections_v1', 'completed');";
                    markCmd.ExecuteNonQuery();
                }
            }
        }

        private void SeedDefaultDevelopers()
        {
            string[] defaultDevelopers = new string[]
            {
                "CD Projekt Red", "Valve", "FromSoftware", "Rockstar Games",
                "Capcom", "Square Enix", "Bethesda Game Studios", "BioWare",
                "Ubisoft Montreal", "Naughty Dog", "Insomniac Games", "Larian Studios",
                "Kojima Productions", "Remedy Entertainment", "Supergiant Games", "Techland"
            };

            using (var connection = DatabaseHelper.GetOpenConnection())
            {
                using (var checkCmd = connection.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT COUNT(*) FROM db_metadata WHERE key = 'seed_developers_v1';";
                    long count = (long)checkCmd.ExecuteScalar()!;
                    if (count > 0) return;
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT OR IGNORE INTO developers (name) VALUES (@name);";
                    var param = command.CreateParameter();
                    param.ParameterName = "@name";
                    command.Parameters.Add(param);

                    foreach (var devName in defaultDevelopers)
                    {
                        param.Value = devName;
                        command.ExecuteNonQuery();
                    }
                }

                using (var markCmd = connection.CreateCommand())
                {
                    markCmd.CommandText = "INSERT INTO db_metadata (key, value) VALUES ('seed_developers_v1', 'completed');";
                    markCmd.ExecuteNonQuery();
                }
            }
            Console.WriteLine("Default developers seeded successfully.");
        }

        private void SeedDefaultPublishers()
        {
            string[] defaultPublishers = new string[]
            {
                "Electronic Arts", "Ubisoft", "Sony Interactive Entertainment", "Xbox Game Studios",
                "Square Enix", "Capcom", "Bandai Namco Entertainment", "Take-Two Interactive",
                "2K Games", "Sega", "Bethesda Softworks", "Warner Bros. Games",
                "Devolver Digital", "Focus Entertainment", "THQ Nordic", "505 Games"
            };

            using (var connection = DatabaseHelper.GetOpenConnection())
            {
                using (var checkCmd = connection.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT COUNT(*) FROM db_metadata WHERE key = 'seed_publishers_v1';";
                    long count = (long)checkCmd.ExecuteScalar()!;
                    if (count > 0) return;
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT OR IGNORE INTO publishers (name) VALUES (@name);";
                    var param = command.CreateParameter();
                    param.ParameterName = "@name";
                    command.Parameters.Add(param);

                    foreach (var pubName in defaultPublishers)
                    {
                        param.Value = pubName;
                        command.ExecuteNonQuery();
                    }
                }

                using (var markCmd = connection.CreateCommand())
                {
                    markCmd.CommandText = "INSERT INTO db_metadata (key, value) VALUES ('seed_publishers_v1', 'completed');";
                    markCmd.ExecuteNonQuery();
                }
            }
            Console.WriteLine("Default publishers seeded successfully.");
        }

        private void SeedDefaultPlatforms()
        {
            string[] defaultPlatforms = new string[]
            {
                "PC (Windows)", "Steam", "Epic Games Store", "GOG",
                "Ubisoft Connect", "EA App", "Battle.net", "Xbox App (Game Pass)",
                "PlayStation 1", "PlayStation 2", "PlayStation 3", "PlayStation 4",
                "Nintendo Switch", "Nintendo GameCube", "Nintendo Wii", "Nintendo 3DS",
                "PlayStation Portable (PSP)", "PlayStation Vita", "Xbox 360", "RetroArch"
            };

            using (var connection = DatabaseHelper.GetOpenConnection())
            {
                using (var checkCmd = connection.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT COUNT(*) FROM db_metadata WHERE key = 'seed_platforms_v1';";
                    long count = (long)checkCmd.ExecuteScalar()!;
                    if (count > 0) return;
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT OR IGNORE INTO platforms (name) VALUES (@name);";
                    var param = command.CreateParameter();
                    param.ParameterName = "@name";
                    command.Parameters.Add(param);

                    foreach (var platformName in defaultPlatforms)
                    {
                        param.Value = platformName;
                        command.ExecuteNonQuery();
                    }
                }

                using (var markCmd = connection.CreateCommand())
                {
                    markCmd.CommandText = "INSERT INTO db_metadata (key, value) VALUES ('seed_platforms_v1', 'completed');";
                    markCmd.ExecuteNonQuery();
                }
            }
            Console.WriteLine("Default platforms seeded successfully.");
        }

        private void SeedDefaultAgeRatings()
        {
            // Daftar default rating (ESRB & PEGI) beserta deskripsinya
            var defaultRatings = new (string Name, string Desc)[]
            {
                // --- ESRB (Entertainment Software Rating Board) ---
                ("ESRB: Everyone (E)", "Content is generally suitable for all ages. May contain minimal cartoon, fantasy or mild violence."),
                ("ESRB: Everyone 10+ (E10+)", "Content is generally suitable for ages 10 and up. May contain more cartoon, fantasy or mild violence."),
                ("ESRB: Teen (T)", "Content is generally suitable for ages 13 and up. May contain violence, suggestive themes, crude humor, or blood."),
                ("ESRB: Mature 17+ (M)", "Content is generally suitable for ages 17 and up. May contain intense violence, blood and gore, sexual content, or strong language."),
                ("ESRB: Adults Only 18+ (AO)", "Content suitable only for adults ages 18 and up. May include prolonged scenes of intense violence or graphic sexual content."),
                ("ESRB: Rating Pending (RP)", "Not yet assigned a final ESRB rating."),

                // --- PEGI (Pan European Game Information) ---
                ("PEGI 3", "Suitable for all age groups. Contains no sounds or pictures that may frighten young children."),
                ("PEGI 7", "Suitable for ages 7 and up. May contain scenes or sounds that can possibly be frightening to younger children."),
                ("PEGI 12", "Suitable for ages 12 and up. Shows slightly more explicit violence towards fantasy characters or non-graphic violence towards human characters."),
                ("PEGI 16", "Suitable for ages 16 and up. Realistic violence, sexual activity, or coarse language."),
                ("PEGI 18", "Suitable for adults 18 and up. High levels of violence, graphic sexual depictions, or depiction of drug use.")
            };

            using (var connection = DatabaseHelper.GetOpenConnection())
            {
                // 1. Cek apakah sudah pernah di-seed sebelumnya
                using (var checkCmd = connection.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT COUNT(*) FROM db_metadata WHERE key = 'seed_age_ratings_v1';";
                    long count = (long)checkCmd.ExecuteScalar()!;
                    if (count > 0) return;
                }

                // 2. Insert data default dengan transaksi agar cepat & aman
                using (var transaction = connection.BeginTransaction())
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = "INSERT OR IGNORE INTO age_ratings (name, desc) VALUES (@name, @desc);";

                    var paramName = command.CreateParameter();
                    paramName.ParameterName = "@name";
                    command.Parameters.Add(paramName);

                    var paramDesc = command.CreateParameter();
                    paramDesc.ParameterName = "@desc";
                    command.Parameters.Add(paramDesc);

                    foreach (var rating in defaultRatings)
                    {
                        paramName.Value = rating.Name;
                        paramDesc.Value = (object?)rating.Desc ?? DBNull.Value;
                        command.ExecuteNonQuery();
                    }

                    // 3. Catat di db_metadata bahwa seeding selesai
                    using (var markCmd = connection.CreateCommand())
                    {
                        markCmd.Transaction = transaction;
                        markCmd.CommandText = "INSERT INTO db_metadata (key, value) VALUES ('seed_age_ratings_v1', 'completed');";
                        markCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }

            Console.WriteLine("Default age ratings seeded successfully.");
        }

        #endregion
    }
}