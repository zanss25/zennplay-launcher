using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Repositories
{
    public class DeveloperRepository
    {
        #region 1. CREATE (INSERT)

        /// <summary>
        /// Menambahkan developer baru ke database secara asinkron.
        /// </summary>
        public async Task<Guid> AddDeveloperAsync(DeveloperModel developer)
        {
            if (developer.Id == Guid.Empty)
            {
                developer.Id = Guid.NewGuid();
            }

            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sqlDeveloper = @"
                INSERT INTO developers (id, name, desc) 
                VALUES (@Id, @Name, @Desc);";

            await conn.ExecuteAsync(sqlDeveloper, new
            {
                Id = developer.Id.ToString(),
                developer.Name,
                developer.Desc
            });

            return developer.Id;
        }

        #endregion

        #region 2. READ (FETCH & QUERY)

        /// <summary>
        /// Mengambil semua developer (diurutkan alfabetis) secara asinkron.
        /// </summary>
        public async Task<IEnumerable<DeveloperModel>> GetAllDevelopersAsync()
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "SELECT * FROM developers ORDER BY name ASC;";
            return await conn.QueryAsync<DeveloperModel>(sql);
        }

        /// <summary>
        /// Mengambil detail satu developer berdasarkan ID secara asinkron.
        /// </summary>
        public async Task<DeveloperModel?> GetDeveloperByIdAsync(Guid id)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "SELECT * FROM developers WHERE id = @Id LIMIT 1;";
            return await conn.QueryFirstOrDefaultAsync<DeveloperModel>(sql, new { Id = id.ToString() });
        }

        /// <summary>
        /// Mengambil seluruh developer yang dimiliki oleh game tertentu secara asinkron.
        /// </summary>
        public async Task<IEnumerable<DeveloperModel>> GetDevelopersByAppIdAsync(Guid appId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"
                SELECT d.* FROM developers d
                INNER JOIN app_developers ad ON d.id = ad.developer_id
                WHERE ad.app_id = @AppId
                ORDER BY d.name ASC;";

            return await conn.QueryAsync<DeveloperModel>(sql, new { AppId = appId.ToString() });
        }

        /// <summary>
        /// Mencari developer berdasarkan kata kunci nama secara asinkron.
        /// </summary>
        public async Task<IEnumerable<DeveloperModel>> GetDevelopersFilteredAsync(string? searchQuery = null)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            var sql = "SELECT * FROM developers";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                sql += " WHERE name LIKE @Query";
                parameters.Add("Query", $"%{searchQuery}%");
            }

            sql += " ORDER BY name ASC;";

            return await conn.QueryAsync<DeveloperModel>(sql, parameters);
        }

        #endregion

        #region 3. UPDATE

        /// <summary>
        /// Mengubah nama dan deskripsi developer secara asinkron.
        /// </summary>
        public async Task<bool> UpdateDeveloperAsync(DeveloperModel developer)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"
                UPDATE developers SET
                    name = @Name,
                    desc = @Desc
                WHERE id = @Id;";

            int rowsAffected = await conn.ExecuteAsync(sql, new
            {
                Id = developer.Id.ToString(),
                developer.Name,
                developer.Desc
            });

            return rowsAffected > 0;
        }

        #endregion

        #region 4. RELATIONAL MANAGEMENT (APP <-> DEVELOPER)

        /// <summary>
        /// Menghubungkan game dengan suatu Developer secara asinkron.
        /// </summary>
        public async Task<bool> AssignDeveloperToAppAsync(Guid appId, Guid developerId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "INSERT OR IGNORE INTO app_developers (app_id, developer_id) VALUES (@AppId, @DeveloperId);";
            int rowsAffected = await conn.ExecuteAsync(sql, new { AppId = appId.ToString(), DeveloperId = developerId.ToString() });
            return rowsAffected > 0;
        }

        /// <summary>
        /// Menghapus hubungan developer dari game tertentu secara asinkron.
        /// </summary>
        public async Task<bool> RemoveDeveloperFromAppAsync(Guid appId, Guid developerId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "DELETE FROM app_developers WHERE app_id = @AppId AND developer_id = @DeveloperId;";
            int rowsAffected = await conn.ExecuteAsync(sql, new { AppId = appId.ToString(), DeveloperId = developerId.ToString() });
            return rowsAffected > 0;
        }

        #endregion

        #region 5. DELETE

        /// <summary>
        /// Hapus permanen Developer dari database secara asinkron.
        /// </summary>
        public async Task<bool> DeleteDeveloperAsync(Guid id)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "DELETE FROM developers WHERE id = @Id;";
            int rowsAffected = await conn.ExecuteAsync(sql, new { Id = id.ToString() });
            return rowsAffected > 0;
        }

        #endregion
    }
}