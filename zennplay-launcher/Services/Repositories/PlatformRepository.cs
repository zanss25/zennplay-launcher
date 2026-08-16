using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Repositories
{
    public class PlatformRepository
    {
        #region 1. CREATE (INSERT)

        /// <summary>
        /// Menambahkan platform baru ke database.
        /// </summary>
        public int AddPlatform(PlatformModel platform)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sqlPlatform = @"
                    INSERT INTO platforms (name, desc) 
                    VALUES (@Name, @Desc);
                    SELECT last_insert_rowid();";

                int newPlatformId = conn.ExecuteScalar<int>(sqlPlatform, platform);
                platform.Id = newPlatformId;
                return newPlatformId;
            }
        }

        #endregion

        #region 2. READ (FETCH & QUERY)

        /// <summary>
        /// Mengambil semua platform (diurutkan alfabetis).
        /// </summary>
        public IEnumerable<PlatformModel> GetAllPlatforms()
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "SELECT * FROM platforms ORDER BY name ASC;";
                return conn.Query<PlatformModel>(sql);
            }
        }

        /// <summary>
        /// Mengambil detail satu platform berdasarkan ID.
        /// </summary>
        public PlatformModel? GetPlatformById(int id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                // PERBAIKAN: Mengubah tabel 'apps' menjadi 'platforms'
                string sql = "SELECT * FROM platforms WHERE id = @Id LIMIT 1;";
                return conn.QueryFirstOrDefault<PlatformModel>(sql, new { Id = id });
            }
        }

        /// <summary>
        /// Mengambil seluruh platform yang dimiliki oleh game tertentu.
        /// </summary>
        public IEnumerable<PlatformModel> GetPlatformsByAppId(int appId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    SELECT g.* FROM platforms g
                    INNER JOIN app_platforms ag ON g.id = ag.platform_id
                    WHERE ag.app_id = @AppId
                    ORDER BY g.name ASC;";

                return conn.Query<PlatformModel>(sql, new { AppId = appId });
            }
        }

        /// <summary>
        /// Mencari platform berdasarkan kata kunci nama.
        /// </summary>
        public IEnumerable<PlatformModel> GetPlatformsFiltered(string? searchQuery = null)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                var sql = "SELECT * FROM platforms";
                var parameters = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    sql += " WHERE name LIKE @Query";
                    parameters.Add("Query", $"%{searchQuery}%");
                }

                sql += " ORDER BY name ASC;";

                return conn.Query<PlatformModel>(sql, parameters);
            }
        }

        #endregion

        #region 3. UPDATE

        /// <summary>
        /// Mengubah nama dan deskripsi platform.
        /// </summary>
        public bool UpdatePlatform(PlatformModel platform)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    UPDATE platforms SET
                        name = @Name,
                        desc = @Desc
                    WHERE id = @Id;";

                return conn.Execute(sql, platform) > 0;
            }
        }

        #endregion

        #region 4. RELATIONAL MANAGEMENT (APP <-> GENRE)

        /// <summary>
        /// Menghubungkan game dengan suatu Platform.
        /// </summary>
        public bool AssignPlatformToApp(int appId, int platformId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "INSERT OR IGNORE INTO app_platforms (app_id, platform_id) VALUES (@AppId, @PlatformId);";
                return conn.Execute(sql, new { AppId = appId, PlatformId = platformId }) > 0;
            }
        }

        /// <summary>
        /// Menghapus hubungan platform dari game tertentu.
        /// </summary>
        public bool RemovePlatformFromApp(int appId, int platformId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM app_platforms WHERE app_id = @AppId AND platform_id = @PlatformId;";
                return conn.Execute(sql, new { AppId = appId, PlatformId = platformId }) > 0;
            }
        }

        #endregion

        #region 5. DELETE

        /// <summary>
        /// Hapus permanen Platform dari database.
        /// (ON DELETE CASCADE pada DB akan otomatis membersihkan relasi di 'app_platforms').
        /// </summary>
        public bool DeletePlatform(int id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM platforms WHERE id = @Id;";
                return conn.Execute(sql, new { Id = id }) > 0;
            }
        }

        #endregion
    }
}
