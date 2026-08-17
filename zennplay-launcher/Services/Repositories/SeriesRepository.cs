using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Repositories
{
    public class SeriesRepository
    {
        #region 1. CREATE (INSERT)

        /// <summary>
        /// Menambahkan series baru ke database.
        /// </summary>
        public int AddSeries(SeriesModel series)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sqlSeries = @"
                    INSERT INTO series (name, desc) 
                    VALUES (@Name, @Desc);
                    SELECT last_insert_rowid();";

                int newSeriesId = conn.ExecuteScalar<int>(sqlSeries, series);
                series.Id = newSeriesId;
                return newSeriesId;
            }
        }

        #endregion

        #region 2. READ (FETCH & QUERY)

        /// <summary>
        /// Mengambil semua series (diurutkan alfabetis).
        /// </summary>
        public IEnumerable<SeriesModel> GetAllSeries()
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "SELECT * FROM series ORDER BY name ASC;";
                return conn.Query<SeriesModel>(sql);
            }
        }

        /// <summary>
        /// Mengambil detail satu series berdasarkan ID.
        /// </summary>
        public SeriesModel? GetSeriesById(int id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                // PERBAIKAN: Mengubah tabel 'apps' menjadi 'series'
                string sql = "SELECT * FROM series WHERE id = @Id LIMIT 1;";
                return conn.QueryFirstOrDefault<SeriesModel>(sql, new { Id = id });
            }
        }

        /// <summary>
        /// Mengambil seluruh series yang dimiliki oleh game tertentu.
        /// </summary>
        public IEnumerable<SeriesModel> GetSeriesByAppId(int appId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    SELECT s.* FROM series s
                    INNER JOIN app_series as ON s.id = as.series_id
                    WHERE as.app_id = @AppId
                    ORDER BY s.name ASC;";

                return conn.Query<SeriesModel>(sql, new { AppId = appId });
            }
        }

        /// <summary>
        /// Mencari series berdasarkan kata kunci nama.
        /// </summary>
        public IEnumerable<SeriesModel> GetSeriesFiltered(string? searchQuery = null)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                var sql = "SELECT * FROM series";
                var parameters = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    sql += " WHERE name LIKE @Query";
                    parameters.Add("Query", $"%{searchQuery}%");
                }

                sql += " ORDER BY name ASC;";

                return conn.Query<SeriesModel>(sql, parameters);
            }
        }

        #endregion

        #region 3. UPDATE

        /// <summary>
        /// Mengubah nama dan deskripsi series.
        /// </summary>
        public bool UpdateSeries(SeriesModel series)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    UPDATE series SET
                        name = @Name,
                        desc = @Desc
                    WHERE id = @Id;";

                return conn.Execute(sql, series) > 0;
            }
        }

        #endregion

        #region 4. RELATIONAL MANAGEMENT (APP <-> GENRE)

        /// <summary>
        /// Menghubungkan game dengan suatu Series.
        /// </summary>
        public bool AssignSeriesToApp(int appId, int seriesId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "INSERT OR IGNORE INTO app_series (app_id, series_id) VALUES (@AppId, @SeriesId);";
                return conn.Execute(sql, new { AppId = appId, SeriesId = seriesId }) > 0;
            }
        }

        /// <summary>
        /// Menghapus hubungan series dari game tertentu.
        /// </summary>
        public bool RemoveSeriesFromApp(int appId, int seriesId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM app_series WHERE app_id = @AppId AND series_id = @SeriesId;";
                return conn.Execute(sql, new { AppId = appId, SeriesId = seriesId }) > 0;
            }
        }

        #endregion

        #region 5. DELETE

        /// <summary>
        /// Hapus permanen Series dari database.
        /// (ON DELETE CASCADE pada DB akan otomatis membersihkan relasi di 'app_series').
        /// </summary>
        public bool DeleteSeries(int id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM series WHERE id = @Id;";
                return conn.Execute(sql, new { Id = id }) > 0;
            }
        }

        #endregion
    }
}
