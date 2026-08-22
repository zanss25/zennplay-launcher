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
        public Guid AddApp(AppModel app,
                           List<AppActionModel>? actions = null,
                           List<AppLinkModel>? links = null,
                           List<Guid>? sectionIds = null,
                           List<Guid>? seriesIds = null,
                           List<Guid>? genreIds = null,
                           List<Guid>? tagIds = null,
                           List<Guid>? developerIds = null,
                           List<Guid>? publisherIds = null,
                           List<Guid>? platformIds = null,
                           List<Guid>? ageRatingIds = null)
        {
            // Auto-generate GUID jika belum diinisialisasi
            if (app.Id == Guid.Empty)
            {
                app.Id = Guid.NewGuid();
            }

            using (var conn = DatabaseHelper.GetOpenConnection())
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // 1. Insert data utama ke tabel 'apps'
                    string sqlApp = @"
                        INSERT INTO apps (
                            id, title, sorting_title, desc, user_rating, is_favorite, is_pinned,
                            notes, first_launched, last_launched, launch_times, completion_status,
                            release_date, is_installed, is_game, is_excluded, version, app_dir, 
                            source, external_id, cover_path, background_path, icon_path
                        ) VALUES (
                            @Id, @Title, @SortingTitle, @Desc, @UserRating, @IsFavorite, @IsPinned,
                            @Notes, @FirstLaunched, @LastLaunched, @LaunchTimes, @CompletionStatus,
                            @ReleaseDate, @IsInstalled, @IsGame, @IsExcluded, @Version, @AppDir, 
                            @Source, @ExternalId, @CoverPath, @BackgroundPath, @IconPath
                        );";

                    conn.Execute(sqlApp, new
                    {
                        Id = app.Id.ToString(),
                        app.Title,
                        app.SortingTitle,
                        app.Desc,
                        app.UserRating,
                        app.IsFavorite,
                        app.IsPinned,
                        app.Notes,
                        app.FirstLaunched,
                        app.LastLaunched,
                        app.LaunchTimes,
                        app.CompletionStatus,
                        app.ReleaseDate,
                        app.IsInstalled,
                        app.IsGame,
                        app.IsExcluded,
                        app.Version,
                        app.AppDir,
                        app.Source,
                        app.ExternalId,
                        app.CoverPath,
                        app.BackgroundPath,
                        app.IconPath
                    }, transaction);

                    string appIdStr = app.Id.ToString();

                    // 2. Insert Relasi Sections
                    if (sectionIds != null && sectionIds.Any())
                    {
                        string sqlSection = "INSERT OR IGNORE INTO app_sections (app_id, section_id) VALUES (@AppId, @SectionId);";
                        var sectionData = sectionIds.Select(secId => new { AppId = appIdStr, SectionId = secId.ToString() });
                        conn.Execute(sqlSection, sectionData, transaction);
                    }

                    // 3. Insert Relasi Series
                    if (seriesIds != null && seriesIds.Any())
                    {
                        string sqlSeries = "INSERT OR IGNORE INTO app_series (app_id, series_id) VALUES (@AppId, @SeriesId);";
                        var seriesData = seriesIds.Select(serId => new { AppId = appIdStr, SeriesId = serId.ToString() });
                        conn.Execute(sqlSeries, seriesData, transaction);
                    }

                    // 4. Insert Relasi Genres
                    if (genreIds != null && genreIds.Any())
                    {
                        string sqlGenre = "INSERT OR IGNORE INTO app_genres (app_id, genre_id) VALUES (@AppId, @GenreId);";
                        var genreData = genreIds.Select(gId => new { AppId = appIdStr, GenreId = gId.ToString() });
                        conn.Execute(sqlGenre, genreData, transaction);
                    }

                    // 5. Insert Relasi Tags
                    if (tagIds != null && tagIds.Any())
                    {
                        string sqlTag = "INSERT OR IGNORE INTO app_tags (app_id, tag_id) VALUES (@AppId, @TagId);";
                        var tagData = tagIds.Select(tId => new { AppId = appIdStr, TagId = tId.ToString() });
                        conn.Execute(sqlTag, tagData, transaction);
                    }

                    // 6. Insert Relasi Developer
                    if (developerIds != null && developerIds.Any())
                    {
                        string sqlDeveloper = "INSERT OR IGNORE INTO app_developers (app_id, developer_id) VALUES (@AppId, @DeveloperId);";
                        var developerData = developerIds.Select(devId => new { AppId = appIdStr, DeveloperId = devId.ToString() });
                        conn.Execute(sqlDeveloper, developerData, transaction);
                    }

                    // 7. Insert Relasi Publisher
                    if (publisherIds != null && publisherIds.Any())
                    {
                        string sqlPublisher = "INSERT OR IGNORE INTO app_publishers (app_id, publisher_id) VALUES (@AppId, @PublisherId);";
                        var publisherData = publisherIds.Select(pubId => new { AppId = appIdStr, PublisherId = pubId.ToString() });
                        conn.Execute(sqlPublisher, publisherData, transaction);
                    }

                    // 8. Insert Relasi Platform
                    if (platformIds != null && platformIds.Any())
                    {
                        string sqlPlatform = "INSERT OR IGNORE INTO app_platforms (app_id, platform_id) VALUES (@AppId, @PlatformId);";
                        var platformData = platformIds.Select(pltId => new { AppId = appIdStr, PlatformId = pltId.ToString() });
                        conn.Execute(sqlPlatform, platformData, transaction);
                    }

                    // 9. Insert Relasi Age Rating
                    if (ageRatingIds != null && ageRatingIds.Any())
                    {
                        string sqlAgeRating = "INSERT OR IGNORE INTO app_age_ratings (app_id, age_rating_id) VALUES (@AppId, @AgeRatingId);";
                        var ageRatingData = ageRatingIds.Select(artId => new { AppId = appIdStr, AgeRatingId = artId.ToString() });
                        conn.Execute(sqlAgeRating, ageRatingData, transaction);
                    }

                    // 10. Insert App Actions
                    if (actions != null && actions.Any())
                    {
                        string sqlAction = @"
                            INSERT INTO app_actions (id, app_id, title, desc, action_type, target, launch_options, action_icon_path, is_game, is_primary)
                            VALUES (@Id, @AppId, @Title, @Desc, @ActionType, @Target, @LaunchOptions, @ActionIconPath, @IsGame, @IsPrimary);";

                        var actionData = actions.Select(act => new
                        {
                            Id = (act.Id == Guid.Empty ? Guid.NewGuid() : act.Id).ToString(),
                            AppId = appIdStr,
                            act.Title,
                            act.Desc,
                            act.ActionType,
                            act.Target,
                            act.LaunchOptions,
                            act.ActionIconPath,
                            act.IsGame,
                            act.IsPrimary
                        });

                        conn.Execute(sqlAction, actionData, transaction);
                    }

                    // 11. Insert App Links
                    if (links != null && links.Any())
                    {
                        string sqlLink = "INSERT INTO app_links (id, app_id, name, url) VALUES (@Id, @AppId, @Name, @Url);";

                        var linkData = links.Select(link => new
                        {
                            Id = (link.Id == Guid.Empty ? Guid.NewGuid() : link.Id).ToString(),
                            AppId = appIdStr,
                            link.Name,
                            link.Url
                        });

                        conn.Execute(sqlLink, linkData, transaction);
                    }

                    // Commit jika semua tahap sukses
                    transaction.Commit();
                    return app.Id;
                }
                catch
                {
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
        /// Mengambil detail satu aplikasi berdasarkan ID (GUID).
        /// </summary>
        public AppModel? GetAppById(Guid id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string idStr = id.ToString();
                string sql = "SELECT * FROM apps WHERE id = @Id LIMIT 1;";
                var app = conn.QueryFirstOrDefault<AppModel>(sql, new { Id = idStr });

                if (app != null)
                {
                    // Isi list ID relasi langsung ke dalam objek app
                    app.SectionIds = conn.Query<string>("SELECT section_id FROM app_sections WHERE app_id = @Id;", new { Id = idStr }).Select(Guid.Parse).ToList();
                    app.SeriesIds = conn.Query<string>("SELECT series_id FROM app_series WHERE app_id = @Id;", new { Id = idStr }).Select(Guid.Parse).ToList();
                    app.GenreIds = conn.Query<string>("SELECT genre_id FROM app_genres WHERE app_id = @Id;", new { Id = idStr }).Select(Guid.Parse).ToList();
                    app.TagIds = conn.Query<string>("SELECT tag_id FROM app_tags WHERE app_id = @Id;", new { Id = idStr }).Select(Guid.Parse).ToList();
                    app.DeveloperIds = conn.Query<string>("SELECT developer_id FROM app_developers WHERE app_id = @Id;", new { Id = idStr }).Select(Guid.Parse).ToList();
                    app.PublisherIds = conn.Query<string>("SELECT publisher_id FROM app_publishers WHERE app_id = @Id;", new { Id = idStr }).Select(Guid.Parse).ToList();
                    app.PlatformIds = conn.Query<string>("SELECT platform_id FROM app_platforms WHERE app_id = @Id;", new { Id = idStr }).Select(Guid.Parse).ToList();
                    app.AgeRatingIds = conn.Query<string>("SELECT age_rating_id FROM app_age_ratings WHERE app_id = @Id;", new { Id = idStr }).Select(Guid.Parse).ToList();
                }

                return app;
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

                // 3. Multi-Select Relations (Format string ID untuk Dapper IN clause)
                if (criteria.SectionIds != null && criteria.SectionIds.Any())
                {
                    sql += " AND a.id IN (SELECT app_id FROM app_sections WHERE section_id IN @SectionIds)";
                    parameters.Add("SectionIds", criteria.SectionIds.Select(id => id.ToString()));
                }

                if (criteria.SeriesIds != null && criteria.SeriesIds.Any())
                {
                    sql += " AND a.id IN (SELECT app_id FROM app_series WHERE series_id IN @SeriesIds)";
                    parameters.Add("SeriesIds", criteria.SeriesIds.Select(id => id.ToString()));
                }

                if (criteria.GenreIds != null && criteria.GenreIds.Any())
                {
                    sql += " AND a.id IN (SELECT app_id FROM app_genres WHERE genre_id IN @GenreIds)";
                    parameters.Add("GenreIds", criteria.GenreIds.Select(id => id.ToString()));
                }

                if (criteria.TagIds != null && criteria.TagIds.Any())
                {
                    sql += " AND a.id IN (SELECT app_id FROM app_tags WHERE tag_id IN @TagIds)";
                    parameters.Add("TagIds", criteria.TagIds.Select(id => id.ToString()));
                }

                if (criteria.DeveloperIds != null && criteria.DeveloperIds.Any())
                {
                    sql += " AND a.id IN (SELECT app_id FROM app_developers WHERE developer_id IN @DeveloperIds)";
                    parameters.Add("DeveloperIds", criteria.DeveloperIds.Select(id => id.ToString()));
                }

                if (criteria.PublisherIds != null && criteria.PublisherIds.Any())
                {
                    sql += " AND a.id IN (SELECT app_id FROM app_publishers WHERE publisher_id IN @PublisherIds)";
                    parameters.Add("PublisherIds", criteria.PublisherIds.Select(id => id.ToString()));
                }

                if (criteria.PlatformIds != null && criteria.PlatformIds.Any())
                {
                    sql += " AND a.id IN (SELECT app_id FROM app_platforms WHERE platform_id IN @PlatformIds)";
                    parameters.Add("PlatformIds", criteria.PlatformIds.Select(id => id.ToString()));
                }

                if (criteria.AgeRatingIds != null && criteria.AgeRatingIds.Any())
                {
                    sql += " AND a.id IN (SELECT app_id FROM app_age_ratings WHERE age_rating_id IN @AgeRatingIds)";
                    parameters.Add("AgeRatingIds", criteria.AgeRatingIds.Select(id => id.ToString()));
                }

                // 4. Sorting
                string sortColumn = criteria.SortBy switch
                {
                    AppSortBy.ReleaseDate => "a.release_date",
                    AppSortBy.UserRating => "a.user_rating",
                    AppSortBy.CreatedAt => "a.created_at",
                    AppSortBy.LastPlayed => "a.last_launched",
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
        public bool UpdateApp(AppModel app,
                      List<AppActionModel>? actions = null,
                      List<AppLinkModel>? links = null,
                      List<Guid>? sectionIds = null,
                      List<Guid>? seriesIds = null,
                      List<Guid>? genreIds = null,
                      List<Guid>? tagIds = null,
                      List<Guid>? developerIds = null,
                      List<Guid>? publisherIds = null,
                      List<Guid>? platformIds = null,
                      List<Guid>? ageRatingIds = null)
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
                            is_installed = @IsInstalled,
                            is_game = @IsGame,
                            is_excluded = @IsExcluded,
                            version = @Version,
                            app_dir = @AppDir,
                            source = @Source,
                            external_id = @ExternalId,
                            cover_path = @CoverPath,
                            background_path = @BackgroundPath,
                            icon_path = @IconPath
                        WHERE id = @Id;";

                    int rowsAffected = conn.Execute(updateAppSql, new
                    {
                        Id = app.Id.ToString(),
                        app.Title,
                        app.SortingTitle,
                        app.Desc,
                        app.UserRating,
                        IsFavorite = app.IsFavorite ? 1 : 0,
                        IsPinned = app.IsPinned ? 1 : 0,
                        app.Notes,
                        app.CompletionStatus,
                        app.ReleaseDate,
                        IsInstalled = app.IsInstalled ? 1 : 0,
                        IsGame = app.IsGame ? 1 : 0,
                        IsExcluded = app.IsExcluded ? 1 : 0,
                        app.Version,
                        app.AppDir,
                        app.Source,
                        app.ExternalId,
                        app.CoverPath,
                        app.BackgroundPath,
                        app.IconPath
                    }, transaction: transaction);

                    if (rowsAffected == 0)
                    {
                        transaction.Rollback();
                        return false;
                    }

                    // 2. Sinkronisasi semua tabel junction jika list disediakan
                    if (sectionIds != null)
                        SyncAppRelations(conn, transaction, app.Id, "app_sections", "section_id", sectionIds);

                    if (seriesIds != null)
                        SyncAppRelations(conn, transaction, app.Id, "app_series", "series_id", seriesIds);

                    if (genreIds != null)
                        SyncAppRelations(conn, transaction, app.Id, "app_genres", "genre_id", genreIds);

                    if (tagIds != null)
                        SyncAppRelations(conn, transaction, app.Id, "app_tags", "tag_id", tagIds);

                    if (developerIds != null)
                        SyncAppRelations(conn, transaction, app.Id, "app_developers", "developer_id", developerIds);

                    if (publisherIds != null)
                        SyncAppRelations(conn, transaction, app.Id, "app_publishers", "publisher_id", publisherIds);

                    if (platformIds != null)
                        SyncAppRelations(conn, transaction, app.Id, "app_platforms", "platform_id", platformIds);

                    if (ageRatingIds != null)
                        SyncAppRelations(conn, transaction, app.Id, "app_age_ratings", "age_rating_id", ageRatingIds);
                    /*
                    // 3. Sinkronisasi entitas child (Actions & Links)
                    if (actions != null)
                        SyncAppActions(conn, transaction, app.Id, actions);

                    if (links != null)
                        SyncAppLinks(conn, transaction, app.Id, links);
                    */

                    // 4. Commit seluruh transaksi
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        private void SyncAppRelations(IDbConnection conn,
                                      IDbTransaction transaction,
                                      Guid appId,
                                      string tableName,
                                      string foreignKeyColumn,
                                      IEnumerable<Guid> targetIds)
        {
            string appIdStr = appId.ToString();

            // A. Hapus semua relasi lama milik game ini di junction table
            string deleteSql = $"DELETE FROM {tableName} WHERE app_id = @AppId;";
            conn.Execute(deleteSql, new { AppId = appIdStr }, transaction: transaction);

            // B. Masukkan data baru jika ada ID yang dipilih
            if (targetIds != null && targetIds.Any())
            {
                string insertSql = $"INSERT OR IGNORE INTO {tableName} (app_id, {foreignKeyColumn}) VALUES (@AppId, @TargetId);";

                var parameters = targetIds.Select(id => new { AppId = appIdStr, TargetId = id.ToString() });
                conn.Execute(insertSql, parameters, transaction: transaction);
            }
        }

        /// <summary>
        /// Khusus untuk memperbarui statistik jam main setelah game ditutup.
        /// </summary>
        public bool UpdatePlayStatistics(Guid appId, string lastLaunched, int newLaunchTimes)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    UPDATE apps SET
                        first_launched = COALESCE(first_launched, @LastLaunched),
                        last_launched = @LastLaunched,
                        launch_times = @LaunchTimes
                    WHERE id = @Id;";

                return conn.Execute(sql, new { Id = appId.ToString(), LastLaunched = lastLaunched, LaunchTimes = newLaunchTimes }) > 0;
            }
        }

        #endregion

        #region 4. DELETE (SOFT & HARD)

        /// <summary>
        /// Soft Delete: Menandai game sebagai hapus tanpa membuang baris data secara permanen.
        /// </summary>
        public bool SoftDeleteApp(Guid id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "UPDATE apps SET is_deleted = 1 WHERE id = @Id;";
                return conn.Execute(sql, new { Id = id.ToString() }) > 0;
            }
        }

        /// <summary>
        /// Restore: Mengembalikan game yang sebelumnya di-soft delete.
        /// </summary>
        public bool RestoreApp(Guid id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "UPDATE apps SET is_deleted = 0 WHERE id = @Id;";
                return conn.Execute(sql, new { Id = id.ToString() }) > 0;
            }
        }

        /// <summary>
        /// Hard Delete: Hapus permanen data dari database. 
        /// (ON DELETE CASCADE akan membersihkan relasi actions, links, tags, genres, dan sessions).
        /// </summary>
        public bool HardDeleteApp(Guid id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM apps WHERE id = @Id;";
                return conn.Execute(sql, new { Id = id.ToString() }) > 0;
            }
        }

        #endregion

        #region 5. LAUNCH TRACKING

        public void RecordGameLaunch(Guid appId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                // 1. Cek apakah game ini baru PERTAMA KALI dimainkan
                string checkSql = "SELECT launch_times FROM apps WHERE id = @Id;";
                int launchTimes = conn.ExecuteScalar<int>(checkSql, new { Id = appId.ToString() });

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

        #endregion
    }
}