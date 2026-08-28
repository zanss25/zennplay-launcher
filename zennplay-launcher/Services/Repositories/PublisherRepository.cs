using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Repositories
{
    public class PublisherRepository
    {
        #region 1. CREATE (INSERT)

        /// <summary>
        /// Menambahkan publisher baru ke database secara asinkron.
        /// </summary>
        public async Task<Guid> AddPublisherAsync(PublisherModel publisher)
        {
            if (publisher.Id == Guid.Empty)
            {
                publisher.Id = Guid.NewGuid();
            }

            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sqlPublisher = @"
                INSERT INTO publishers (id, name, desc) 
                VALUES (@Id, @Name, @Desc);";

            await conn.ExecuteAsync(sqlPublisher, new
            {
                Id = publisher.Id.ToString(),
                publisher.Name,
                publisher.Desc
            });

            return publisher.Id;
        }

        #endregion

        #region 2. READ (FETCH & QUERY)

        /// <summary>
        /// Mengambil semua publisher (diurutkan alfabetis) secara asinkron.
        /// </summary>
        public async Task<IEnumerable<PublisherModel>> GetAllPublishersAsync()
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "SELECT * FROM publishers ORDER BY name ASC;";
            return await conn.QueryAsync<PublisherModel>(sql);
        }

        /// <summary>
        /// Mengambil detail satu publisher berdasarkan ID secara asinkron.
        /// </summary>
        public async Task<PublisherModel?> GetPublisherByIdAsync(Guid id)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "SELECT * FROM publishers WHERE id = @Id LIMIT 1;";
            return await conn.QueryFirstOrDefaultAsync<PublisherModel>(sql, new { Id = id.ToString() });
        }

        /// <summary>
        /// Mengambil seluruh publisher yang dimiliki oleh game tertentu secara asinkron.
        /// </summary>
        public async Task<IEnumerable<PublisherModel>> GetPublishersByAppIdAsync(Guid appId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"
                SELECT p.* FROM publishers p
                INNER JOIN app_publishers ap ON p.id = ap.publisher_id
                WHERE ap.app_id = @AppId
                ORDER BY p.name ASC;";

            return await conn.QueryAsync<PublisherModel>(sql, new { AppId = appId.ToString() });
        }

        /// <summary>
        /// Mencari publisher berdasarkan kata kunci nama secara asinkron.
        /// </summary>
        public async Task<IEnumerable<PublisherModel>> GetPublishersFilteredAsync(string? searchQuery = null)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            var sql = "SELECT * FROM publishers";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                sql += " WHERE name LIKE @Query";
                parameters.Add("Query", $"%{searchQuery}%");
            }

            sql += " ORDER BY name ASC;";

            return await conn.QueryAsync<PublisherModel>(sql, parameters);
        }

        #endregion

        #region 3. UPDATE

        /// <summary>
        /// Mengubah nama dan deskripsi publisher secara asinkron.
        /// </summary>
        public async Task<bool> UpdatePublisherAsync(PublisherModel publisher)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"
                UPDATE publishers SET
                    name = @Name,
                    desc = @Desc
                WHERE id = @Id;";

            int rowsAffected = await conn.ExecuteAsync(sql, new
            {
                Id = publisher.Id.ToString(),
                publisher.Name,
                publisher.Desc
            });

            return rowsAffected > 0;
        }

        #endregion

        #region 4. RELATIONAL MANAGEMENT (APP <-> PUBLISHER)

        /// <summary>
        /// Menghubungkan game dengan suatu Publisher secara asinkron.
        /// </summary>
        public async Task<bool> AssignPublisherToAppAsync(Guid appId, Guid publisherId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "INSERT OR IGNORE INTO app_publishers (app_id, publisher_id) VALUES (@AppId, @PublisherId);";
            int rowsAffected = await conn.ExecuteAsync(sql, new { AppId = appId.ToString(), PublisherId = publisherId.ToString() });
            return rowsAffected > 0;
        }

        /// <summary>
        /// Menghapus hubungan publisher dari game tertentu secara asinkron.
        /// </summary>
        public async Task<bool> RemovePublisherFromAppAsync(Guid appId, Guid publisherId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "DELETE FROM app_publishers WHERE app_id = @AppId AND publisher_id = @PublisherId;";
            int rowsAffected = await conn.ExecuteAsync(sql, new { AppId = appId.ToString(), PublisherId = publisherId.ToString() });
            return rowsAffected > 0;
        }

        #endregion

        #region 5. DELETE

        /// <summary>
        /// Hapus permanen Publisher dari database secara asinkron.
        /// </summary>
        public async Task<bool> DeletePublisherAsync(Guid id)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "DELETE FROM publishers WHERE id = @Id;";
            int rowsAffected = await conn.ExecuteAsync(sql, new { Id = id.ToString() });
            return rowsAffected > 0;
        }

        #endregion
    }
}