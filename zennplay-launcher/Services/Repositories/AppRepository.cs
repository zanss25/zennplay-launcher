using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Repositories
{
    public class AppRepository
    {
        #region 1. CREATE (INSERT)

        /// <summary>
        /// Menambahkan aplikasi baru beserta relasi awal (Section, Genres, Tags, Links, Actions)
        /// menggunakan SqliteTransaction secara asinkron demi keamanan data.
        /// </summary>
        public async Task<Guid> AddAppAsync(AppModel app,
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
            if (app.Id == Guid.Empty)
            {
                app.Id = Guid.NewGuid();
            }

            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // 1. Insert data utama ke tabel 'apps'
                string sqlApp = @"
                    INSERT INTO apps (
                        id, title, sorting_title, desc, user_rating, is_favorite, is_pinned,
                        notes, first_launched, last_launched, launch_times, completion_status,
                        release_date, is_installed, is_game, is_excluded, version, app_dir, install_size, 
                        source, external_id, cover_path, background_path, icon_path
                    ) VALUES (
                        @Id, @Title, @SortingTitle, @Desc, @UserRating, @IsFavorite, @IsPinned,
                        @Notes, @FirstLaunched, @LastLaunched, @LaunchTimes, @CompletionStatus,
                        @ReleaseDate, @IsInstalled, @IsGame, @IsExcluded, @Version, @AppDir, @InstallSize,
                        @Source, @ExternalId, @CoverPath, @BackgroundPath, @IconPath
                    );";

                await conn.ExecuteAsync(sqlApp, new
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
                    app.InstallSize,
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
                    await conn.ExecuteAsync(sqlSection, sectionData, transaction);
                }

                // 3. Insert Relasi Series
                if (seriesIds != null && seriesIds.Any())
                {
                    string sqlSeries = "INSERT OR IGNORE INTO app_series (app_id, series_id) VALUES (@AppId, @SeriesId);";
                    var seriesData = seriesIds.Select(serId => new { AppId = appIdStr, SeriesId = serId.ToString() });
                    await conn.ExecuteAsync(sqlSeries, seriesData, transaction);
                }

                // 4. Insert Relasi Genres
                if (genreIds != null && genreIds.Any())
                {
                    string sqlGenre = "INSERT OR IGNORE INTO app_genres (app_id, genre_id) VALUES (@AppId, @GenreId);";
                    var genreData = genreIds.Select(gId => new { AppId = appIdStr, GenreId = gId.ToString() });
                    await conn.ExecuteAsync(sqlGenre, genreData, transaction);
                }

                // 5. Insert Relasi Tags
                if (tagIds != null && tagIds.Any())
                {
                    string sqlTag = "INSERT OR IGNORE INTO app_tags (app_id, tag_id) VALUES (@AppId, @TagId);";
                    var tagData = tagIds.Select(tId => new { AppId = appIdStr, TagId = tId.ToString() });
                    await conn.ExecuteAsync(sqlTag, tagData, transaction);
                }

                // 6. Insert Relasi Developer
                if (developerIds != null && developerIds.Any())
                {
                    string sqlDeveloper = "INSERT OR IGNORE INTO app_developers (app_id, developer_id) VALUES (@AppId, @DeveloperId);";
                    var developerData = developerIds.Select(devId => new { AppId = appIdStr, DeveloperId = devId.ToString() });
                    await conn.ExecuteAsync(sqlDeveloper, developerData, transaction);
                }

                // 7. Insert Relasi Publisher
                if (publisherIds != null && publisherIds.Any())
                {
                    string sqlPublisher = "INSERT OR IGNORE INTO app_publishers (app_id, publisher_id) VALUES (@AppId, @PublisherId);";
                    var publisherData = publisherIds.Select(pubId => new { AppId = appIdStr, PublisherId = pubId.ToString() });
                    await conn.ExecuteAsync(sqlPublisher, publisherData, transaction);
                }

                // 8. Insert Relasi Platform
                if (platformIds != null && platformIds.Any())
                {
                    string sqlPlatform = "INSERT OR IGNORE INTO app_platforms (app_id, platform_id) VALUES (@AppId, @PlatformId);";
                    var platformData = platformIds.Select(pltId => new { AppId = appIdStr, PlatformId = pltId.ToString() });
                    await conn.ExecuteAsync(sqlPlatform, platformData, transaction);
                }

                // 9. Insert Relasi Age Rating
                if (ageRatingIds != null && ageRatingIds.Any())
                {
                    string sqlAgeRating = "INSERT OR IGNORE INTO app_age_ratings (app_id, age_rating_id) VALUES (@AppId, @AgeRatingId);";
                    var ageRatingData = ageRatingIds.Select(artId => new { AppId = appIdStr, AgeRatingId = artId.ToString() });
                    await conn.ExecuteAsync(sqlAgeRating, ageRatingData, transaction);
                }

                // 10. Insert App Actions
                if (actions != null && actions.Any())
                {
                    string sqlAction = @"
                        INSERT INTO app_actions (id, app_id, title, desc, action_type, target, launch_options, action_icon_path, is_game, sort_order)
                        VALUES (@Id, @AppId, @Title, @Desc, @ActionType, @Target, @LaunchOptions, @ActionIconPath, @IsGame, @SortOrder);";

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
                        act.SortOrder
                    });

                    await conn.ExecuteAsync(sqlAction, actionData, transaction);
                }

                // 11. Insert App Links
                if (links != null && links.Any())
                {
                    string sqlLink = "INSERT INTO app_links (id, app_id, name, desc, url, sort_order) VALUES (@Id, @AppId, @Name, @Desc, @Url, @SortOrder);";

                    var linkData = links.Select(link => new
                    {
                        Id = (link.Id == Guid.Empty ? Guid.NewGuid() : link.Id).ToString(),
                        AppId = appIdStr,
                        link.Name,
                        link.Desc,
                        link.Url,
                        link.SortOrder
                    });

                    await conn.ExecuteAsync(sqlLink, linkData, transaction);
                }

                await transaction.CommitAsync();
                return app.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        #endregion

        #region 2. READ (FETCH & QUERY)

        /// <summary>
        /// Mengambil semua aplikasi secara asinkron.
        /// </summary>
        public async Task<IEnumerable<AppModel>> GetAllAppsAsync(bool includeDeleted = false)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = includeDeleted
                ? "SELECT * FROM apps ORDER BY title ASC;"
                : "SELECT * FROM apps WHERE is_deleted = 0 ORDER BY title ASC;";

            return await conn.QueryAsync<AppModel>(sql);
        }

        /// <summary>
        /// Mengambil detail satu aplikasi beserta seluruh junction ID relasinya secara asinkron.
        /// </summary>
        public async Task<AppModel?> GetAppByIdAsync(Guid id)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string idStr = id.ToString();
            string sql = "SELECT * FROM apps WHERE id = @Id LIMIT 1;";
            var app = await conn.QueryFirstOrDefaultAsync<AppModel>(sql, new { Id = idStr });

            if (app != null)
            {
                var sectionTask = conn.QueryAsync<string>("SELECT section_id FROM app_sections WHERE app_id = @Id;", new { Id = idStr });
                var seriesTask = conn.QueryAsync<string>("SELECT series_id FROM app_series WHERE app_id = @Id;", new { Id = idStr });
                var genreTask = conn.QueryAsync<string>("SELECT genre_id FROM app_genres WHERE app_id = @Id;", new { Id = idStr });
                var tagTask = conn.QueryAsync<string>("SELECT tag_id FROM app_tags WHERE app_id = @Id;", new { Id = idStr });
                var developerTask = conn.QueryAsync<string>("SELECT developer_id FROM app_developers WHERE app_id = @Id;", new { Id = idStr });
                var publisherTask = conn.QueryAsync<string>("SELECT publisher_id FROM app_publishers WHERE app_id = @Id;", new { Id = idStr });
                var platformTask = conn.QueryAsync<string>("SELECT platform_id FROM app_platforms WHERE app_id = @Id;", new { Id = idStr });
                var ageRatingTask = conn.QueryAsync<string>("SELECT age_rating_id FROM app_age_ratings WHERE app_id = @Id;", new { Id = idStr });

                await Task.WhenAll(sectionTask, seriesTask, genreTask, tagTask, developerTask, publisherTask, platformTask, ageRatingTask);

                app.SectionIds = (await sectionTask).Select(Guid.Parse).ToList();
                app.SeriesIds = (await seriesTask).Select(Guid.Parse).ToList();
                app.GenreIds = (await genreTask).Select(Guid.Parse).ToList();
                app.TagIds = (await tagTask).Select(Guid.Parse).ToList();
                app.DeveloperIds = (await developerTask).Select(Guid.Parse).ToList();
                app.PublisherIds = (await publisherTask).Select(Guid.Parse).ToList();
                app.PlatformIds = (await platformTask).Select(Guid.Parse).ToList();
                app.AgeRatingIds = (await ageRatingTask).Select(Guid.Parse).ToList();
            }

            return app;
        }

        /// <summary>
        /// Query filter lanjutan berdasarkan kriteria filter secara asinkron.
        /// </summary>
        public async Task<IEnumerable<AppModel>> GetAppsFilteredAsync(AppFilterCriteria criteria)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
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

            return await conn.QueryAsync<AppModel>(sql, parameters);
        }

        public async Task<Guid> AddAppActionAsync(AppActionModel action)
        {
            if (action.Id == Guid.Empty)
            {
                action.Id = Guid.NewGuid();
            }

            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"
                INSERT INTO app_actions (id, app_id, title, desc, action_type, target, launch_options, action_icon_path, is_game, sort_order)
                VALUES (@Id, @AppId, @Title, @Desc, @ActionType, @Target, @LaunchOptions, @ActionIconPath, @IsGame, @SortOrder);";

            await conn.ExecuteAsync(sql, new
            {
                Id = action.Id.ToString(),
                AppId = action.AppId.ToString(),
                action.Title,
                action.Desc,
                action.ActionType,
                action.Target,
                action.LaunchOptions,
                action.ActionIconPath,
                IsGame = action.IsGame ? 1 : 0,
                action.SortOrder
            });

            return action.Id;
        }

        public async Task<List<AppActionModel>> GetAppActionsAsync(Guid appId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "SELECT * FROM app_actions WHERE app_id = @AppId ORDER BY sort_order ASC, title ASC;";
            var result = await conn.QueryAsync<AppActionModel>(sql, new { AppId = appId.ToString() });
            return result.ToList();
        }

        public async Task<List<AppLinkModel>> GetAppLinksAsync(Guid appId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "SELECT * FROM app_links WHERE app_id = @AppId ORDER BY sort_order ASC, name ASC, url ASC;";
            var result = await conn.QueryAsync<AppLinkModel>(sql, new { AppId = appId.ToString() });
            return result.ToList();
        }

        #endregion

        #region 3. UPDATE

        /// <summary>
        /// Mengubah data metadata aplikasi utama secara asinkron.
        /// </summary>
        public async Task<bool> UpdateAppAsync(AppModel app,
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
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            await using var transaction = await conn.BeginTransactionAsync();

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
                        install_size = @InstallSize,
                        source = @Source,
                        external_id = @ExternalId,
                        cover_path = @CoverPath,
                        background_path = @BackgroundPath,
                        icon_path = @IconPath
                    WHERE id = @Id;";

                int rowsAffected = await conn.ExecuteAsync(updateAppSql, new
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
                    app.InstallSize,
                    app.Source,
                    app.ExternalId,
                    app.CoverPath,
                    app.BackgroundPath,
                    app.IconPath
                }, transaction: transaction);

                if (rowsAffected == 0)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // 2. Sinkronisasi semua tabel junction
                if (sectionIds != null)
                    await SyncAppRelationsAsync(conn, transaction, app.Id, "app_sections", "section_id", sectionIds);

                if (seriesIds != null)
                    await SyncAppRelationsAsync(conn, transaction, app.Id, "app_series", "series_id", seriesIds);

                if (genreIds != null)
                    await SyncAppRelationsAsync(conn, transaction, app.Id, "app_genres", "genre_id", genreIds);

                if (tagIds != null)
                    await SyncAppRelationsAsync(conn, transaction, app.Id, "app_tags", "tag_id", tagIds);

                if (developerIds != null)
                    await SyncAppRelationsAsync(conn, transaction, app.Id, "app_developers", "developer_id", developerIds);

                if (publisherIds != null)
                    await SyncAppRelationsAsync(conn, transaction, app.Id, "app_publishers", "publisher_id", publisherIds);

                if (platformIds != null)
                    await SyncAppRelationsAsync(conn, transaction, app.Id, "app_platforms", "platform_id", platformIds);

                if (ageRatingIds != null)
                    await SyncAppRelationsAsync(conn, transaction, app.Id, "app_age_ratings", "age_rating_id", ageRatingIds);

                // 3. Sinkronisasi child entities
                if (actions != null)
                    await SyncAppActionsAsync(conn, transaction, app.Id, actions);

                if (links != null)
                    await SyncAppLinksAsync(conn, transaction, app.Id, links);

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        private async Task SyncAppActionsAsync(DbConnection conn, DbTransaction transaction, Guid appId, IEnumerable<AppActionModel> actions)
        {
            string appIdStr = appId.ToString();

            // 1. Hapus action lama
            string deleteSql = "DELETE FROM app_actions WHERE app_id = @AppId;";
            await conn.ExecuteAsync(deleteSql, new { AppId = appIdStr }, transaction: transaction);

            // 2. Simpan action baru
            if (actions != null && actions.Any())
            {
                string insertSql = @"
                    INSERT INTO app_actions (id, app_id, title, desc, action_type, target, launch_options, action_icon_path, is_game, sort_order)
                    VALUES (@Id, @AppId, @Title, @Desc, @ActionType, @Target, @LaunchOptions, @ActionIconPath, @IsGame, @SortOrder);";

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
                    IsGame = act.IsGame ? 1 : 0,
                    act.SortOrder
                });

                await conn.ExecuteAsync(insertSql, actionData, transaction: transaction);
            }
        }

        private async Task SyncAppLinksAsync(DbConnection conn, DbTransaction transaction, Guid appId, IEnumerable<AppLinkModel> links)
        {
            string appIdStr = appId.ToString();

            // 1. Hapus link lama
            string deleteSql = "DELETE FROM app_links WHERE app_id = @AppId;";
            await conn.ExecuteAsync(deleteSql, new { AppId = appIdStr }, transaction: transaction);

            // 2. Simpan link baru
            if (links != null && links.Any())
            {
                string insertSql = "INSERT INTO app_links (id, app_id, name, desc, url, sort_order) VALUES (@Id, @AppId, @Name, @Desc, @Url, @SortOrder);";

                var linkData = links.Select(link => new
                {
                    Id = (link.Id == Guid.Empty ? Guid.NewGuid() : link.Id).ToString(),
                    AppId = appIdStr,
                    link.Name,
                    link.Desc,
                    link.Url,
                    link.SortOrder
                });

                await conn.ExecuteAsync(insertSql, linkData, transaction: transaction);
            }
        }

        private async Task SyncAppRelationsAsync(DbConnection conn,
                                                DbTransaction transaction,
                                                Guid appId,
                                                string tableName,
                                                string foreignKeyColumn,
                                                IEnumerable<Guid> targetIds)
        {
            string appIdStr = appId.ToString();

            // A. Hapus semua relasi lama
            string deleteSql = $"DELETE FROM {tableName} WHERE app_id = @AppId;";
            await conn.ExecuteAsync(deleteSql, new { AppId = appIdStr }, transaction: transaction);

            // B. Masukkan data baru
            if (targetIds != null && targetIds.Any())
            {
                string insertSql = $"INSERT OR IGNORE INTO {tableName} (app_id, {foreignKeyColumn}) VALUES (@AppId, @TargetId);";
                var parameters = targetIds.Select(id => new { AppId = appIdStr, TargetId = id.ToString() });
                await conn.ExecuteAsync(insertSql, parameters, transaction: transaction);
            }
        }

        public async Task<bool> UpdatePlayStatisticsAsync(Guid appId, string lastLaunched, int newLaunchTimes)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"
                UPDATE apps SET
                    first_launched = COALESCE(first_launched, @LastLaunched),
                    last_launched = @LastLaunched,
                    launch_times = @LaunchTimes
                WHERE id = @Id;";

            return await conn.ExecuteAsync(sql, new { Id = appId.ToString(), LastLaunched = lastLaunched, LaunchTimes = newLaunchTimes }) > 0;
        }

        #endregion

        #region 4. DELETE (SOFT & HARD)

        public async Task<bool> SoftDeleteAppAsync(Guid id)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "UPDATE apps SET is_deleted = 1 WHERE id = @Id;";
            return await conn.ExecuteAsync(sql, new { Id = id.ToString() }) > 0;
        }

        public async Task<bool> RestoreAppAsync(Guid id)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "UPDATE apps SET is_deleted = 0 WHERE id = @Id;";
            return await conn.ExecuteAsync(sql, new { Id = id.ToString() }) > 0;
        }

        public async Task<bool> HardDeleteAppAsync(Guid id)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "DELETE FROM apps WHERE id = @Id;";
            return await conn.ExecuteAsync(sql, new { Id = id.ToString() }) > 0;
        }

        #endregion

        #region 5. LAUNCH TRACKING

        public async Task RecordGameLaunchAsync(Guid appId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();

            // 1. Cek peluncuran pertama
            string checkSql = "SELECT launch_times FROM apps WHERE id = @Id;";
            int launchTimes = await conn.ExecuteScalarAsync<int>(checkSql, new { Id = appId.ToString() });

            if (launchTimes == 0)
            {
                await IncrementGlobalCounterAsync(conn, "stats_games_ever_played");
            }

            // 2. Tambah counter total peluncuran
            await IncrementGlobalCounterAsync(conn, "stats_total_launch_count");
        }

        private async Task IncrementGlobalCounterAsync(IDbConnection conn, string keyName)
        {
            string sql = @"
                INSERT INTO db_metadata (key, value) VALUES (@Key, '1')
                ON CONFLICT(key) DO UPDATE SET value = CAST(value AS INTEGER) + 1;";

            await conn.ExecuteAsync(sql, new { Key = keyName });
        }

        #endregion
    }
}