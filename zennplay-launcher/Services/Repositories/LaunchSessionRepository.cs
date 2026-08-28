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
    public class LaunchSessionRepository
    {
        #region 1. SESSION MANAGEMENT (START & END)

        /// <summary>
        /// Dipanggil saat tombol "PLAY" diklik. 
        /// Mencatat awal sesi bermain game di tabel 'launch_sessions' secara asinkron.
        /// </summary>
        public async Task<Guid> StartSessionAsync(Guid appId)
        {
            Guid sessionId = Guid.NewGuid();

            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();

            // Format standar ISO 8601 (YYYY-MM-DD HH:mm:ss) untuk SQLite Text
            string startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string sql = @"
                INSERT INTO launch_sessions (id, app_id, session_start)
                VALUES (@Id, @AppId, @SessionStart);";

            await conn.ExecuteAsync(sql, new
            {
                Id = sessionId.ToString(),
                AppId = appId.ToString(),
                SessionStart = startTime
            });

            return sessionId;
        }

        /// <summary>
        /// Dipanggil saat proses game ditutup.
        /// Mengisi session_end, menghitung duration_seconds, dan memperbarui statistik secara asinkron.
        /// </summary>
        public async Task<bool> EndSessionAsync(Guid sessionId, Guid appId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // 1. Ambil session_start untuk menghitung durasi
                string getStartSql = "SELECT session_start FROM launch_sessions WHERE id = @SessionId;";
                string? startStr = await conn.ExecuteScalarAsync<string>(getStartSql, new { SessionId = sessionId.ToString() }, transaction);

                int durationSeconds = 0;
                if (DateTime.TryParse(startStr, out DateTime startTime))
                {
                    durationSeconds = (int)(DateTime.Now - startTime).TotalSeconds;
                    if (durationSeconds < 0) durationSeconds = 0; // Guard clause
                }

                // 2. Update baris launch_sessions ini
                string updateSessionSql = @"
                    UPDATE launch_sessions SET
                        session_end = @SessionEnd,
                        duration_seconds = @DurationSeconds
                    WHERE id = @Id;";

                await conn.ExecuteAsync(updateSessionSql, new
                {
                    Id = sessionId.ToString(),
                    SessionEnd = endTime,
                    DurationSeconds = durationSeconds
                }, transaction);

                // 3. Update statistik pada tabel 'apps' (last_launched & launch_times)
                string updateAppStatsSql = @"
                    UPDATE apps SET
                        first_launched = COALESCE(first_launched, @EndTime),
                        last_launched = @EndTime,
                        launch_times = launch_times + 1
                    WHERE id = @AppId;";

                await conn.ExecuteAsync(updateAppStatsSql, new
                {
                    AppId = appId.ToString(),
                    EndTime = endTime
                }, transaction);

                // 4. Update Counter Global di 'db_metadata' (Total Playtime & Total Launches)
                await IncrementGlobalPlaytimeAsync(conn, transaction, durationSeconds);

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        #endregion

        #region 2. READ & ANALYTICS

        /// <summary>
        /// Mengambil seluruh riwayat sesi bermain untuk 1 game tertentu secara asinkron.
        /// </summary>
        public async Task<IEnumerable<LaunchSessionModel>> GetSessionsByAppIdAsync(Guid appId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"
                SELECT * FROM launch_sessions 
                WHERE app_id = @AppId 
                ORDER BY session_start DESC;";

            return await conn.QueryAsync<LaunchSessionModel>(sql, new { AppId = appId.ToString() });
        }

        /// <summary>
        /// Menghitung total durasi jam main (dalam detik) untuk 1 game tertentu secara asinkron.
        /// </summary>
        public async Task<int> GetTotalPlaytimeSecondsByAppIdAsync(Guid appId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"
                SELECT COALESCE(SUM(duration_seconds), 0) 
                FROM launch_sessions 
                WHERE app_id = @AppId;";

            return await conn.ExecuteScalarAsync<int>(sql, new { AppId = appId.ToString() });
        }

        /// <summary>
        /// Mengambil riwayat sesi bermain terkini dari SELURUH game secara asinkron.
        /// </summary>
        public async Task<IEnumerable<LaunchSessionModel>> GetRecentGlobalSessionsAsync(int limit = 10)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"
                SELECT * FROM launch_sessions 
                ORDER BY session_start DESC 
                LIMIT @Limit;";

            return await conn.QueryAsync<LaunchSessionModel>(sql, new { Limit = limit });
        }

        #endregion

        #region 3. DELETE

        /// <summary>
        /// Menghapus 1 baris riwayat sesi tertentu secara asinkron.
        /// </summary>
        public async Task<bool> DeleteSessionAsync(Guid sessionId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "DELETE FROM launch_sessions WHERE id = @Id;";
            int rowsAffected = await conn.ExecuteAsync(sql, new { Id = sessionId.ToString() });
            return rowsAffected > 0;
        }

        /// <summary>
        /// Menghapus seluruh riwayat jam main untuk 1 game tertentu secara asinkron.
        /// </summary>
        public async Task<bool> ClearAppSessionsAsync(Guid appId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "DELETE FROM launch_sessions WHERE app_id = @AppId;";
            int rowsAffected = await conn.ExecuteAsync(sql, new { AppId = appId.ToString() });
            return rowsAffected > 0;
        }

        #endregion

        #region 4. PRIVATE HELPER

        private async Task IncrementGlobalPlaytimeAsync(DbConnection conn, DbTransaction transaction, int addedSeconds)
        {
            string sql = @"
                INSERT INTO db_metadata (key, value) VALUES ('stats_total_playtime_seconds', CAST(@AddedSeconds AS TEXT))
                ON CONFLICT(key) DO UPDATE SET value = CAST(CAST(value AS INTEGER) + @AddedSeconds AS TEXT);";

            await conn.ExecuteAsync(sql, new { AddedSeconds = addedSeconds }, transaction);
        }

        #endregion
    }
}