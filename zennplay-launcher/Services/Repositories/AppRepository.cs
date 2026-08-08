using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Repositories
{
    public class AppRepository
    {
        #region 1. CREATE (INSERT)

        /// <summary>
        /// Menambahkan aplikasi baru beserta relasi awal (Section, Genres, Tags, Links, Actions)
        /// menggunakan SqliteTransaction demi keamanan data.
        /// </summary>
        public int AddApp(AppModel app,
                          List<int>? sectionIds = null,
                          List<int>? genreIds = null,
                          List<int>? tagIds = null,
                          List<AppActionModel>? actions = null,
                          List<AppLinkModel>? links = null)
        {
            // Auto-generate GUID jika belum ada
            if (string.IsNullOrWhiteSpace(app.Guid))
            {
                app.Guid = Guid.NewGuid().ToString();
            }

            using (var conn = DatabaseHelper.GetOpenConnection())
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // A. Insert data utama ke tabel 'apps'
                    string sqlApp = @"
                        INSERT INTO apps (
                            guid, title, sorting_title, desc, user_rating, is_favorite, is_pinned,
                            series, notes, first_launched, last_launched, launch_times, completion_status,
                            developer, publisher, release_date, age_rating, is_installed, is_game,
                            is_excluded, version, app_dir, source_platform, external_id, cover_path,
                            background_path, icon_path
                        ) VALUES (
                            @Guid, @Title, @SortingTitle, @Desc, @UserRating, @IsFavorite, @IsPinned,
                            @Series, @Notes, @FirstLaunched, @LastLaunched, @LaunchTimes, @CompletionStatus,
                            @Developer, @Publisher, @ReleaseDate, @AgeRating, @IsInstalled, @IsGame,
                            @IsExcluded, @Version, @AppDir, @SourcePlatform, @ExternalId, @CoverPath,
                            @BackgroundPath, @IconPath
                        );
                        SELECT last_insert_rowid();";

                    int newAppId = conn.ExecuteScalar<int>(sqlApp, app, transaction);
                    app.Id = newAppId;

                    // B. Insert Relasi Sections
                    if (sectionIds != null && sectionIds.Any())
                    {
                        string sqlSection = "INSERT INTO app_sections (app_id, section_id) VALUES (@AppId, @SectionId);";
                        var sectionData = sectionIds.Select(secId => new { AppId = newAppId, SectionId = secId });
                        conn.Execute(sqlSection, sectionData, transaction);
                    }

                    // C. Insert Relasi Genres
                    if (genreIds != null && genreIds.Any())
                    {
                        string sqlGenre = "INSERT INTO app_genres (app_id, genre_id) VALUES (@AppId, @GenreId);";
                        var genreData = genreIds.Select(gId => new { AppId = newAppId, GenreId = gId });
                        conn.Execute(sqlGenre, genreData, transaction);
                    }

                    // D. Insert Relasi Tags
                    if (tagIds != null && tagIds.Any())
                    {
                        string sqlTag = "INSERT INTO app_tags (app_id, tag_id) VALUES (@AppId, @TagId);";
                        var tagData = tagIds.Select(tId => new { AppId = newAppId, TagId = tId });
                        conn.Execute(sqlTag, tagData, transaction);
                    }

                    // E. Insert App Actions
                    if (actions != null && actions.Any())
                    {
                        string sqlAction = @"
                            INSERT INTO app_actions (app_id, title, desc, action_type, target, launch_options, action_icon_path, is_game, is_primary)
                            VALUES (@AppId, @Title, @Desc, @ActionType, @Target, @LaunchOptions, @ActionIconPath, @IsGame, @IsPrimary);";

                        foreach (var act in actions) { act.AppId = newAppId; }
                        conn.Execute(sqlAction, actions, transaction);
                    }

                    // F. Insert App Links
                    if (links != null && links.Any())
                    {
                        string sqlLink = "INSERT INTO app_links (app_id, name, url) VALUES (@AppId, @Name, @Url);";
                        foreach (var link in links) { link.AppId = newAppId; }
                        conn.Execute(sqlLink, links, transaction);
                    }

                    // Commit jika semua tahap sukses
                    transaction.Commit();
                    return newAppId;
                }
                catch
                {
                    // Batalkan semua perubahan jika terjadi error di tengah jalan
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #endregion

        #region 2. READ (FETCH & QUERY)

        /// <summary>
        /// Mengambil semua aplikasi. Secara default hanya mengambil data yang belum dihapus.
        /// </summary>
        public IEnumerable<AppModel> GetAllApps(bool includeDeleted = false)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = includeDeleted
                    ? "SELECT * FROM apps ORDER BY title ASC;"
                    : "SELECT * FROM apps WHERE is_deleted = 0 ORDER BY title ASC;";

                return conn.Query<AppModel>(sql);
            }
        }

        /// <summary>
        /// Mengambil detail satu aplikasi berdasarkan ID.
        /// </summary>
        public AppModel? GetAppById(int id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "SELECT * FROM apps WHERE id = @Id LIMIT 1;";
                return conn.QueryFirstOrDefault<AppModel>(sql, new { Id = id });
            }
        }

        /// <summary>
        /// Query filter lanjutan berdasarkan pencarian, favorit, atau section tertentu.
        /// </summary>
        public IEnumerable<AppModel> GetAppsFiltered(string? searchQuery = null, int? sectionId = null, bool? onlyFavorites = null)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                var sql = @"
                    SELECT DISTINCT a.* FROM apps a
                    LEFT JOIN app_sections aps ON a.id = aps.app_id
                    WHERE a.is_deleted = 0 ";

                var parameters = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    sql += " AND (a.title LIKE @Query OR a.series LIKE @Query OR a.developer LIKE @Query)";
                    parameters.Add("Query", $"%{searchQuery}%");
                }

                if (sectionId.HasValue)
                {
                    sql += " AND aps.section_id = @SectionId";
                    parameters.Add("SectionId", sectionId.Value);
                }

                if (onlyFavorites.HasValue && onlyFavorites.Value)
                {
                    sql += " AND a.is_favorite = 1";
                }

                sql += " ORDER BY a.title ASC;";

                return conn.Query<AppModel>(sql, parameters);
            }
        }

        #endregion

        #region 3. UPDATE

        /// <summary>
        /// Mengubah data metadata aplikasi utama.
        /// </summary>
        public bool UpdateApp(AppModel app)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    UPDATE apps SET
                        title = @Title,
                        sorting_title = @SortingTitle,
                        desc = @Desc,
                        user_rating = @UserRating,
                        is_favorite = @IsFavorite,
                        is_pinned = @IsPinned,
                        series = @Series,
                        notes = @Notes,
                        completion_status = @CompletionStatus,
                        developer = @Developer,
                        publisher = @Publisher,
                        release_date = @ReleaseDate,
                        age_rating = @AgeRating,
                        is_installed = @IsInstalled,
                        is_game = @IsGame,
                        is_excluded = @IsExcluded,
                        version = @Version,
                        app_dir = @AppDir,
                        source_platform = @SourcePlatform,
                        external_id = @ExternalId,
                        cover_path = @CoverPath,
                        background_path = @BackgroundPath,
                        icon_path = @IconPath
                    WHERE id = @Id;";

                return conn.Execute(sql, app) > 0;
            }
        }

        /// <summary>
        /// Khusus untuk memperbarui statistik jam main setelah game ditutup.
        /// </summary>
        public bool UpdatePlayStatistics(int appId, string lastLaunched, int newLaunchTimes)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    UPDATE apps SET
                        first_launched = COALESCE(first_launched, @LastLaunched),
                        last_launched = @LastLaunched,
                        launch_times = @LaunchTimes
                    WHERE id = @Id;";

                return conn.Execute(sql, new { Id = appId, LastLaunched = lastLaunched, LaunchTimes = newLaunchTimes }) > 0;
            }
        }

        #endregion

        #region 4. DELETE (SOFT & HARD)

        /// <summary>
        /// Soft Delete: Menandai game sebagai hapus tanpa membuang baris data secara permanen.
        /// </summary>
        public bool SoftDeleteApp(int id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "UPDATE apps SET is_deleted = 1 WHERE id = @Id;";
                return conn.Execute(sql, new { Id = id }) > 0;
            }
        }

        /// <summary>
        /// Restore: Mengembalikan game yang sebelumnya di-soft delete.
        /// </summary>
        public bool RestoreApp(int id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "UPDATE apps SET is_deleted = 0 WHERE id = @Id;";
                return conn.Execute(sql, new { Id = id }) > 0;
            }
        }

        /// <summary>
        /// Hard Delete: Hapus permanen data dari database. 
        /// (ON DELETE CASCADE akan membersihkan relasi actions, links, tags, genres, dan sessions).
        /// </summary>
        public bool HardDeleteApp(int id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM apps WHERE id = @Id;";
                return conn.Execute(sql, new { Id = id }) > 0;
            }
        }

        #endregion

        public void RecordGameLaunch(int appId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                // 1. Cek apakah game ini baru PERTAMA KALI dimainkan
                string checkSql = "SELECT launch_times FROM apps WHERE id = @Id;";
                int launchTimes = conn.ExecuteScalar<int>(checkSql, new { Id = appId });

                if (launchTimes == 0)
                {
                    // Jika ini peluncuran pertama (launch_times masih 0), tambah counter Games Ever Played
                    IncrementGlobalCounter(conn, "stats_games_ever_played");
                }

                // 2. Tambah counter total peluncuran global
                IncrementGlobalCounter(conn, "stats_total_launch_count");
            }
        }

        private void IncrementGlobalCounter(IDbConnection conn, string keyName)
        {
            string sql = @"
        INSERT INTO db_metadata (key, value) VALUES (@Key, '1')
        ON CONFLICT(key) DO UPDATE SET value = CAST(value AS INTEGER) + 1;";

            conn.Execute(sql, new { Key = keyName });
        }

    }
}