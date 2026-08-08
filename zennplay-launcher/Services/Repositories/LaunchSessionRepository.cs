using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Repositories
{
    public class LaunchSessionRepository
    {
        #region 1. SESSION MANAGEMENT (START & END)

        /// <summary>
        /// Dipanggil saat tombol "PLAY" diklik. 
        /// Mencatat awal sesi bermain game di tabel 'launch_sessions'.
        /// </summary>
        public int StartSession(int appId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                // Format standar ISO 8601 (YYYY-MM-DD HH:mm:ss) untuk SQLite Text
                string startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                string sql = @"
                    INSERT INTO launch_sessions (app_id, session_start)
                    VALUES (@AppId, @SessionStart);
                    SELECT last_insert_rowid();";

                return conn.ExecuteScalar<int>(sql, new { AppId = appId, SessionStart = startTime });
            }
        }

        /// <summary>
        /// Dipanggil saat proses game ditutup.
        /// Mengisi session_end, menghitung duration_seconds, dan memperbarui statistik di tabel 'apps' & 'db_metadata'.
        /// </summary>
        public bool EndSession(int sessionId, int appId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    string endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    // 1. Ambil session_start untuk menghitung durasi
                    string getStartSql = "SELECT session_start FROM launch_sessions WHERE id = @SessionId;";
                    string? startStr = conn.ExecuteScalar<string>(getStartSql, new { SessionId = sessionId }, transaction);

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

                    conn.Execute(updateSessionSql, new { Id = sessionId, SessionEnd = endTime, DurationSeconds = durationSeconds }, transaction);

                    // 3. Update statistik pada tabel 'apps' (last_launched & launch_times)
                    string updateAppStatsSql = @"
                        UPDATE apps SET
                            first_launched = COALESCE(first_launched, @EndTime),
                            last_launched = @EndTime,
                            launch_times = launch_times + 1
                        WHERE id = @AppId;";

                    conn.Execute(updateAppStatsSql, new { AppId = appId, EndTime = endTime }, transaction);

                    // 4. Update Counter Global di 'db_metadata' (Total Playtime & Total Launches)
                    IncrementGlobalPlaytime(conn, transaction, durationSeconds);

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #endregion

        #region 2. READ & ANALYTICS

        /// <summary>
        /// Mengambil seluruh riwayat sesi bermain untuk 1 game tertentu.
        /// </summary>
        public IEnumerable<LaunchSessionModel> GetSessionsByAppId(int appId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    SELECT * FROM launch_sessions 
                    WHERE app_id = @AppId 
                    ORDER BY session_start DESC;";

                return conn.Query<LaunchSessionModel>(sql, new { AppId = appId });
            }
        }

        /// <summary>
        /// Menghitung total durasi jam main (dalam detik) untuk 1 game tertentu.
        /// </summary>
        public int GetTotalPlaytimeSecondsByAppId(int appId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    SELECT COALESCE(SUM(duration_seconds), 0) 
                    FROM launch_sessions 
                    WHERE app_id = @AppId;";

                return conn.ExecuteScalar<int>(sql, new { AppId = appId });
            }
        }

        /// <summary>
        /// Mengambil riwayat sesi bermain terkini dari SELURUH game (untuk Activity Log Dashboard).
        /// </summary>
        public IEnumerable<LaunchSessionModel> GetRecentGlobalSessions(int limit = 10)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    SELECT * FROM launch_sessions 
                    ORDER BY session_start DESC 
                    LIMIT @Limit;";

                return conn.Query<LaunchSessionModel>(sql, new { Limit = limit });
            }
        }

        #endregion

        #region 3. DELETE

        /// <summary>
        /// Menghapus 1 baris riwayat sesi tertentu.
        /// </summary>
        public bool DeleteSession(int sessionId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM launch_sessions WHERE id = @Id;";
                return conn.Execute(sql, new { Id = sessionId }) > 0;
            }
        }

        /// <summary>
        /// Menghapus seluruh riwayat jam main untuk 1 game tertentu (Reset Playtime History).
        /// </summary>

        public bool ClearAppSessions(int appId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM launch_sessions WHERE app_id = @AppId;";
                return conn.Execute(sql, new { AppId = appId }) > 0;
            }
        }

        #endregion

        #region 4. PRIVATE HELPER

        private void IncrementGlobalPlaytime(IDbConnection conn, IDbTransaction transaction, int addedSeconds)
        {
            string sql = @"
                INSERT INTO db_metadata (key, value) VALUES ('stats_total_playtime_seconds', CAST(@AddedSeconds AS TEXT))
                ON CONFLICT(key) DO UPDATE SET value = CAST(CAST(value AS INTEGER) + @AddedSeconds AS TEXT);";

            conn.Execute(sql, new { AddedSeconds = addedSeconds }, transaction);
        }

        #endregion
    }
}