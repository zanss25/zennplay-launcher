using Dapper;
using HarfBuzzSharp;
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
                          List<int>? seriesIds = null,
                          List<int>? genreIds = null,
                          List<int>? tagIds = null,
                          List<int>? developerIds = null,
                          List<int>? publisherIds = null,
                          List<int>? platformIds = null,
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
                    // Insert data utama ke tabel 'apps'
                    string sqlApp = @"
                        INSERT INTO apps (
                            guid, title, sorting_title, desc, user_rating, is_favorite, is_pinned,
                            notes, first_launched, last_launched, launch_times, completion_status,
                            release_date, age_rating, is_installed, is_game, is_excluded, version, app_dir, 
                            external_id, cover_path, background_path, icon_path
                        ) VALUES (
                            @Guid, @Title, @SortingTitle, @Desc, @UserRating, @IsFavorite, @IsPinned,
                            @Notes, @FirstLaunched, @LastLaunched, @LaunchTimes, @CompletionStatus,
                            @ReleaseDate, @AgeRating, @IsInstalled, @IsGame, @IsExcluded, @Version, @AppDir, 
                            @ExternalId, @CoverPath, @BackgroundPath, @IconPath
                        );
                        SELECT last_insert_rowid();";

                    int newAppId = conn.ExecuteScalar<int>(sqlApp, app, transaction);
                    app.Id = newAppId;

                    // Insert Relasi Sections
                    if (sectionIds != null && sectionIds.Any())
                    {
                        string sqlSection = "INSERT INTO app_sections (app_id, section_id) VALUES (@AppId, @SectionId);";
                        var sectionData = sectionIds.Select(secId => new { AppId = newAppId, SectionId = secId });
                        conn.Execute(sqlSection, sectionData, transaction);
                    }

                    // Insert Relasi Series
                    if (seriesIds != null && seriesIds.Any())
                    {
                        string sqlSeries = "INSERT INTO app_series (app_id, series_id) VALUES (@AppId, @SeriesId);";
                        var seriesData = seriesIds.Select(serId => new { AppId = newAppId, SeriesId = serId });
                        conn.Execute(sqlSeries, seriesData, transaction);
                    }

                    // Insert Relasi Genres
                    if (genreIds != null && genreIds.Any())
                    {
                        string sqlGenre = "INSERT INTO app_genres (app_id, genre_id) VALUES (@AppId, @GenreId);";
                        var genreData = genreIds.Select(gId => new { AppId = newAppId, GenreId = gId });
                        conn.Execute(sqlGenre, genreData, transaction);
                    }

                    // Insert Relasi Genres
                    if (genreIds != null && genreIds.Any())
                    {
                        string sqlGenre = "INSERT INTO app_genres (app_id, genre_id) VALUES (@AppId, @GenreId);";
                        var genreData = genreIds.Select(gId => new { AppId = newAppId, GenreId = gId });
                        conn.Execute(sqlGenre, genreData, transaction);
                    }

                    // Insert Relasi Tags
                    if (tagIds != null && tagIds.Any())
                    {
                        string sqlTag = "INSERT INTO app_tags (app_id, tag_id) VALUES (@AppId, @TagId);";
                        var tagData = tagIds.Select(tId => new { AppId = newAppId, TagId = tId });
                        conn.Execute(sqlTag, tagData, transaction);
                    }

                    // Insert App Actions
                    if (actions != null && actions.Any())
                    {
                        string sqlAction = @"
                            INSERT INTO app_actions (app_id, title, desc, action_type, target, launch_options, action_icon_path, is_game, is_primary)
                            VALUES (@AppId, @Title, @Desc, @ActionType, @Target, @LaunchOptions, @ActionIconPath, @IsGame, @IsPrimary);";

                        foreach (var act in actions) { act.AppId = newAppId; }
                        conn.Execute(sqlAction, actions, transaction);
                    }

                    // Insert App Links
                    if (links != null && links.Any())
                    {
                        string sqlLink = "INSERT INTO app_links (app_id, name, url) VALUES (@AppId, @Name, @Url);";
                        foreach (var link in links) { link.AppId = newAppId; }
                        conn.Execute(sqlLink, links, transaction);
                    }

                    // Insert Relasi Developer
                    if (developerIds != null && developerIds.Any())
                    {
                        string sqlDeveloper = "INSERT INTO app_developers (app_id, developer_id) VALUES (@AppId, @DeveloperId);";
                        var developerData = developerIds.Select(devId => new { AppId = newAppId, DeveloperId = devId });
                        conn.Execute(sqlDeveloper, developerData, transaction);
                    }

                    // Insert Relasi Publisher
                    if (publisherIds != null && publisherIds.Any())
                    {
                        string sqlPublisher = "INSERT INTO app_publishers (app_id, publisher_id) VALUES (@AppId, @PublisherId);";
                        var publisherData = publisherIds.Select(pubId => new { AppId = newAppId, PublisherId = pubId });
                        conn.Execute(sqlPublisher, publisherData, transaction);
                    }

                    // Insert Relasi Platform
                    if (platformIds != null && platformIds.Any())
                    {
                        string sqlPlatform = "INSERT INTO app_platforms (app_id, platform_id) VALUES (@AppId, @PlatformId);";
                        var platformData = platformIds.Select(pltId => new { AppId = newAppId, PlatformId = pltId });
                        conn.Execute(sqlPlatform, platformData, transaction);
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

        public IEnumerable<AppModel> GetAppsFiltered(AppFilterCriteria criteria)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                var sql = @"
            SELECT DISTINCT a.* FROM apps a
            WHERE a.is_deleted = 0 ";

                var parameters = new DynamicParameters();

                // 1. Text Search
                if (!string.IsNullOrWhiteSpace(criteria.SearchQuery))
                {
                    sql += " AND (a.title LIKE @Query OR a.sorting_title LIKE @Query)";
                    parameters.Add("Query", $"%{criteria.SearchQuery}%");
                }

                // 2. Flags & Statuses
                if (criteria.OnlyFavorites.HasValue && criteria.OnlyFavorites.Value)
                {
                    sql += " AND a.is_favorite = 1";
                }

                if (criteria.OnlyInstalled.HasValue)
                {
                    sql += " AND a.is_installed = @IsInstalled";
                    parameters.Add("IsInstalled", criteria.OnlyInstalled.Value ? 1 : 0);
                }

                if (criteria.OnlyPinned.HasValue && criteria.OnlyPinned.Value)
                {
                    sql += " AND a.is_pinned = 1";
                }

                if (criteria.IsGame.HasValue)
                {
                    sql += " AND a.is_game = @IsGame";
                    parameters.Add("IsGame", criteria.IsGame.Value ? 1 : 0);
                }

                // 3. Multi-Select Relations
                if (criteria.SectionIds != null && criteria.SectionIds.Any())
                {
                    sql += " AND a.id IN (SELECT app_id FROM app_sections WHERE section_id IN @SectionIds)";
                    parameters.Add("SectionIds", criteria.SectionIds);
                }

                if (criteria.SeriesIds != null && criteria.SeriesIds.Any())
                {
                    sql += " AND a.id IN (SELECT app_id FROM app_series WHERE series_id IN @SeriesIds)";
                    parameters.Add("SeriesIds", criteria.SeriesIds);
                }

                if (criteria.GenreIds != null && criteria.GenreIds.Any())
                {
                    sql += " AND a.id IN (SELECT app_id FROM app_genres WHERE genre_id IN @GenreIds)";
                    parameters.Add("GenreIds", criteria.GenreIds);
                }

                if (criteria.TagIds != null && criteria.TagIds.Any())
                {
                    sql += " AND a.id IN (SELECT app_id FROM app_tags WHERE tag_id IN @TagIds)";
                    parameters.Add("TagIds", criteria.TagIds);
                }

                if (criteria.DeveloperIds != null && criteria.DeveloperIds.Any())
                {
                    sql += " AND a.id IN (SELECT app_id FROM app_developers WHERE developer_id IN @DeveloperIds)";
                    parameters.Add("DeveloperIds", criteria.DeveloperIds);
                }

                if (criteria.PublisherIds != null && criteria.PublisherIds.Any())
                {
                    sql += " AND a.id IN (SELECT app_id FROM app_publishers WHERE publisher_id IN @PublisherIds)";
                    parameters.Add("PublisherIds", criteria.PublisherIds);
                }

                if (criteria.PlatformIds != null && criteria.PlatformIds.Any())
                {
                    sql += " AND a.id IN (SELECT app_id FROM app_platforms WHERE platform_id IN @PlatformIds)";
                    parameters.Add("PlatformIds", criteria.PlatformIds);
                }

                // 4. Sorting
                string sortColumn = criteria.SortBy switch
                {
                    AppSortBy.ReleaseDate => "a.release_date",
                    AppSortBy.UserRating => "a.user_rating",
                    AppSortBy.CreatedAt => "a.created_at",
                    AppSortBy.LastPlayed => "a.last_played",
                    _ => "COALESCE(NULLIF(a.sorting_title, ''), a.title) COLLATE NOCASE"
                };

                string sortDirection = criteria.SortDirection == SortDirection.Descending ? "DESC" : "ASC";

                sql += $" ORDER BY {sortColumn} {sortDirection};";

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
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // 1. Update data dasar tabel apps
                    string updateAppSql = @"
                UPDATE apps SET
                    title = @Title,
                    sorting_title = @SortingTitle,
                    desc = @Desc,
                    user_rating = @UserRating,
                    is_favorite = @IsFavorite,
                    is_pinned = @IsPinned,
                    notes = @Notes,
                    completion_status = @CompletionStatus,
                    release_date = @ReleaseDate,
                    age_rating = @AgeRating,
                    is_installed = @IsInstalled,
                    is_game = @IsGame,
                    is_excluded = @IsExcluded,
                    version = @Version,
                    app_dir = @AppDir,
                    external_id = @ExternalId,
                    cover_path = @CoverPath,
                    background_path = @BackgroundPath,
                    icon_path = @IconPath
                WHERE id = @Id;";

                    int rowsAffected = conn.Execute(updateAppSql, app, transaction: transaction);
                    if (rowsAffected == 0)
                    {
                        transaction.Rollback();
                        return false;
                    }

                    // 2. Sinkronisasi semua tabel junction (Delete Old -> Insert New)
                    SyncAppRelations(conn, transaction, app.Id, "app_developers", "developer_id", app.DeveloperIds);
                    SyncAppRelations(conn, transaction, app.Id, "app_publishers", "publisher_id", app.PublisherIds);
                    SyncAppRelations(conn, transaction, app.Id, "app_platforms", "platform_id", app.PlatformIds);
                    SyncAppRelations(conn, transaction, app.Id, "app_series", "series_id", app.SeriesIds);
                    SyncAppRelations(conn, transaction, app.Id, "app_tags", "tag_id", app.TagIds);
                    SyncAppRelations(conn, transaction, app.Id, "app_genres", "genre_id", app.GenreIds);

                    // 3. Commit seluruh transaksi jika semua berhasil
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    // TODO: Log error (ex.Message)
                    return false;
                }
            }
        }

        private void SyncAppRelations(IDbConnection conn,
                                      IDbTransaction transaction,
                                      long appId,
                                      string tableName,
                                      string foreignKeyColumn,
                                      IEnumerable<long> targetIds)
        {
            // A. Hapus semua relasi lama milik game ini di junction table
            string deleteSql = $"DELETE FROM {tableName} WHERE app_id = @AppId;";
            conn.Execute(deleteSql, new { AppId = appId }, transaction: transaction);

            // B. Masukkan data baru jika ada ID yang dipilih
            if (targetIds != null && targetIds.Any())
            {
                string insertSql = $"INSERT OR IGNORE INTO {tableName} (app_id, {foreignKeyColumn}) VALUES (@AppId, @TargetId);";

                // Dapper otomatis mengeksekusi batch jika parameter berupa collection
                var parameters = targetIds.Select(id => new { AppId = appId, TargetId = id });
                conn.Execute(insertSql, parameters, transaction: transaction);
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