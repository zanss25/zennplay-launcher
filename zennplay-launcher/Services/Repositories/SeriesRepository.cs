using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Repositories
{
    public class SeriesRepository
    {
        #region 1. CREATE (INSERT)

        /// <summary>
        /// Menambahkan series baru ke database secara asinkron.
        /// </summary>
        public async Task<Guid> AddSeriesAsync(SeriesModel series)
        {
            if (series.Id == Guid.Empty)
            {
                series.Id = Guid.NewGuid();
            }

            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sqlSeries = @"
                INSERT INTO series (id, name, desc) 
                VALUES (@Id, @Name, @Desc);";

            await conn.ExecuteAsync(sqlSeries, new
            {
                Id = series.Id.ToString(),
                series.Name,
                series.Desc
            });

            return series.Id;
        }

        #endregion

        #region 2. READ (FETCH & QUERY)

        /// <summary>
        /// Mengambil semua series (diurutkan alfabetis) secara asinkron.
        /// </summary>
        public async Task<IEnumerable<SeriesModel>> GetAllSeriesAsync()
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "SELECT * FROM series ORDER BY name ASC;";
            return await conn.QueryAsync<SeriesModel>(sql);
        }

        /// <summary>
        /// Mengambil detail satu series berdasarkan ID secara asinkron.
        /// </summary>
        public async Task<SeriesModel?> GetSeriesByIdAsync(Guid id)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "SELECT * FROM series WHERE id = @Id LIMIT 1;";
            return await conn.QueryFirstOrDefaultAsync<SeriesModel>(sql, new { Id = id.ToString() });
        }

        /// <summary>
        /// Mengambil seluruh series yang dimiliki oleh game tertentu secara asinkron.
        /// </summary>
        public async Task<IEnumerable<SeriesModel>> GetSeriesByAppIdAsync(Guid appId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"
                SELECT s.* FROM series s
                INNER JOIN app_series asr ON s.id = asr.series_id
                WHERE asr.app_id = @AppId
                ORDER BY s.name ASC;";

            return await conn.QueryAsync<SeriesModel>(sql, new { AppId = appId.ToString() });
        }

        /// <summary>
        /// Mencari series berdasarkan kata kunci nama secara asinkron.
        /// </summary>
        public async Task<IEnumerable<SeriesModel>> GetSeriesFilteredAsync(string? searchQuery = null)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            var sql = "SELECT * FROM series";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                sql += " WHERE name LIKE @Query";
                parameters.Add("Query", $"%{searchQuery}%");
            }

            sql += " ORDER BY name ASC;";

            return await conn.QueryAsync<SeriesModel>(sql, parameters);
        }

        #endregion

        #region 3. UPDATE

        /// <summary>
        /// Mengubah nama dan deskripsi series secara asinkron.
        /// </summary>
        public async Task<bool> UpdateSeriesAsync(SeriesModel series)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"
                UPDATE series SET
                    name = @Name,
                    desc = @Desc
                WHERE id = @Id;";

            int rowsAffected = await conn.ExecuteAsync(sql, new
            {
                Id = series.Id.ToString(),
                series.Name,
                series.Desc
            });

            return rowsAffected > 0;
        }

        #endregion

        #region 4. RELATIONAL MANAGEMENT (APP <-> SERIES)

        /// <summary>
        /// Menghubungkan game dengan suatu Series secara asinkron.
        /// </summary>
        public async Task<bool> AssignSeriesToAppAsync(Guid appId, Guid seriesId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "INSERT OR IGNORE INTO app_series (app_id, series_id) VALUES (@AppId, @SeriesId);";
            int rowsAffected = await conn.ExecuteAsync(sql, new { AppId = appId.ToString(), SeriesId = seriesId.ToString() });
            return rowsAffected > 0;
        }

        /// <summary>
        /// Menghapus hubungan series dari game tertentu secara asinkron.
        /// </summary>
        public async Task<bool> RemoveSeriesFromAppAsync(Guid appId, Guid seriesId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "DELETE FROM app_series WHERE app_id = @AppId AND series_id = @SeriesId;";
            int rowsAffected = await conn.ExecuteAsync(sql, new { AppId = appId.ToString(), SeriesId = seriesId.ToString() });
            return rowsAffected > 0;
        }

        #endregion

        #region 5. DELETE

        /// <summary>
        /// Hapus permanen Series dari database secara asinkron.
        /// </summary>
        public async Task<bool> DeleteSeriesAsync(Guid id)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "DELETE FROM series WHERE id = @Id;";
            int rowsAffected = await conn.ExecuteAsync(sql, new { Id = id.ToString() });
            return rowsAffected > 0;
        }

        #endregion
    }
}