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
        public Guid AddPlatform(PlatformModel platform)
        {
            // Pastikan entitas memiliki ID sebelum disimpan
            if (platform.Id == Guid.Empty)
            {
                platform.Id = Guid.NewGuid();
            }

            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sqlPlatform = @"
                    INSERT INTO platforms (id, name, desc) 
                    VALUES (@Id, @Name, @Desc);";

                conn.Execute(sqlPlatform, new
                {
                    Id = platform.Id.ToString(),
                    platform.Name,
                    platform.Desc
                });

                return platform.Id;
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
        public PlatformModel? GetPlatformById(Guid id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "SELECT * FROM platforms WHERE id = @Id LIMIT 1;";
                return conn.QueryFirstOrDefault<PlatformModel>(sql, new { Id = id.ToString() });
            }
        }

        /// <summary>
        /// Mengambil seluruh platform yang dimiliki oleh game tertentu.
        /// </summary>
        public IEnumerable<PlatformModel> GetPlatformsByAppId(Guid appId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    SELECT p.* FROM platforms p
                    INNER JOIN app_platforms ap ON p.id = ap.platform_id
                    WHERE ap.app_id = @AppId
                    ORDER BY p.name ASC;";

                return conn.Query<PlatformModel>(sql, new { AppId = appId.ToString() });
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

                return conn.Execute(sql, new
                {
                    Id = platform.Id.ToString(),
                    platform.Name,
                    platform.Desc
                }) > 0;
            }
        }

        #endregion

        #region 4. RELATIONAL MANAGEMENT (APP <-> PLATFORM)

        /// <summary>
        /// Menghubungkan game dengan suatu Platform.
        /// </summary>
        public bool AssignPlatformToApp(Guid appId, Guid platformId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "INSERT OR IGNORE INTO app_platforms (app_id, platform_id) VALUES (@AppId, @PlatformId);";
                return conn.Execute(sql, new { AppId = appId.ToString(), PlatformId = platformId.ToString() }) > 0;
            }
        }

        /// <summary>
        /// Menghapus hubungan platform dari game tertentu.
        /// </summary>
        public bool RemovePlatformFromApp(Guid appId, Guid platformId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM app_platforms WHERE app_id = @AppId AND platform_id = @PlatformId;";
                return conn.Execute(sql, new { AppId = appId.ToString(), PlatformId = platformId.ToString() }) > 0;
            }
        }

        #endregion

        #region 5. DELETE

        /// <summary>
        /// Hapus permanen Platform dari database.
        /// (ON DELETE CASCADE pada DB akan otomatis membersihkan relasi di 'app_platforms').
        /// </summary>
        public bool DeletePlatform(Guid id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM platforms WHERE id = @Id;";
                return conn.Execute(sql, new { Id = id.ToString() }) > 0;
            }
        }

        #endregion
    }
}