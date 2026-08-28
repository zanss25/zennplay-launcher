using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Repositories
{
    public class PlatformRepository
    {
        #region 1. CREATE (INSERT)

        /// <summary>
        /// Menambahkan platform baru ke database secara asinkron.
        /// </summary>
        public async Task<Guid> AddPlatformAsync(PlatformModel platform)
        {
            if (platform.Id == Guid.Empty)
            {
                platform.Id = Guid.NewGuid();
            }

            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sqlPlatform = @"
                INSERT INTO platforms (id, name, desc) 
                VALUES (@Id, @Name, @Desc);";

            await conn.ExecuteAsync(sqlPlatform, new
            {
                Id = platform.Id.ToString(),
                platform.Name,
                platform.Desc
            });

            return platform.Id;
        }

        #endregion

        #region 2. READ (FETCH & QUERY)

        /// <summary>
        /// Mengambil semua platform (diurutkan alfabetis) secara asinkron.
        /// </summary>
        public async Task<IEnumerable<PlatformModel>> GetAllPlatformsAsync()
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "SELECT * FROM platforms ORDER BY name ASC;";
            return await conn.QueryAsync<PlatformModel>(sql);
        }

        /// <summary>
        /// Mengambil detail satu platform berdasarkan ID secara asinkron.
        /// </summary>
        public async Task<PlatformModel?> GetPlatformByIdAsync(Guid id)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "SELECT * FROM platforms WHERE id = @Id LIMIT 1;";
            return await conn.QueryFirstOrDefaultAsync<PlatformModel>(sql, new { Id = id.ToString() });
        }

        /// <summary>
        /// Mengambil seluruh platform yang dimiliki oleh game tertentu secara asinkron.
        /// </summary>
        public async Task<IEnumerable<PlatformModel>> GetPlatformsByAppIdAsync(Guid appId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"
                SELECT p.* FROM platforms p
                INNER JOIN app_platforms ap ON p.id = ap.platform_id
                WHERE ap.app_id = @AppId
                ORDER BY p.name ASC;";

            return await conn.QueryAsync<PlatformModel>(sql, new { AppId = appId.ToString() });
        }

        /// <summary>
        /// Mencari platform berdasarkan kata kunci nama secara asinkron.
        /// </summary>
        public async Task<IEnumerable<PlatformModel>> GetPlatformsFilteredAsync(string? searchQuery = null)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            var sql = "SELECT * FROM platforms";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                sql += " WHERE name LIKE @Query";
                parameters.Add("Query", $"%{searchQuery}%");
            }

            sql += " ORDER BY name ASC;";

            return await conn.QueryAsync<PlatformModel>(sql, parameters);
        }

        #endregion

        #region 3. UPDATE

        /// <summary>
        /// Mengubah nama dan deskripsi platform secara asinkron.
        /// </summary>
        public async Task<bool> UpdatePlatformAsync(PlatformModel platform)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"
                UPDATE platforms SET
                    name = @Name,
                    desc = @Desc
                WHERE id = @Id;";

            int rowsAffected = await conn.ExecuteAsync(sql, new
            {
                Id = platform.Id.ToString(),
                platform.Name,
                platform.Desc
            });

            return rowsAffected > 0;
        }

        #endregion

        #region 4. RELATIONAL MANAGEMENT (APP <-> PLATFORM)

        /// <summary>
        /// Menghubungkan game dengan suatu Platform secara asinkron.
        /// </summary>
        public async Task<bool> AssignPlatformToAppAsync(Guid appId, Guid platformId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "INSERT OR IGNORE INTO app_platforms (app_id, platform_id) VALUES (@AppId, @PlatformId);";
            int rowsAffected = await conn.ExecuteAsync(sql, new { AppId = appId.ToString(), PlatformId = platformId.ToString() });
            return rowsAffected > 0;
        }

        /// <summary>
        /// Menghapus hubungan platform dari game tertentu secara asinkron.
        /// </summary>
        public async Task<bool> RemovePlatformFromAppAsync(Guid appId, Guid platformId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "DELETE FROM app_platforms WHERE app_id = @AppId AND platform_id = @PlatformId;";
            int rowsAffected = await conn.ExecuteAsync(sql, new { AppId = appId.ToString(), PlatformId = platformId.ToString() });
            return rowsAffected > 0;
        }

        #endregion

        #region 5. DELETE

        /// <summary>
        /// Hapus permanen Platform dari database secara asinkron.
        /// </summary>
        public async Task<bool> DeletePlatformAsync(Guid id)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "DELETE FROM platforms WHERE id = @Id;";
            int rowsAffected = await conn.ExecuteAsync(sql, new { Id = id.ToString() });
            return rowsAffected > 0;
        }

        #endregion
    }
}