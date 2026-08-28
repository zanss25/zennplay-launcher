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
    public class TagRepository
    {
        #region 1. CREATE (INSERT)

        /// <summary>
        /// Menambahkan Tag baru ke database secara asinkron.
        /// </summary>
        public async Task<Guid> AddTagAsync(TagModel tag)
        {
            // Pastikan entitas memiliki ID sebelum disimpan
            if (tag.Id == Guid.Empty)
            {
                tag.Id = Guid.NewGuid();
            }

            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sqlTag = @"
                INSERT INTO tags (id, name, desc) 
                VALUES (@Id, @Name, @Desc);";

            await conn.ExecuteAsync(sqlTag, new
            {
                Id = tag.Id.ToString(),
                tag.Name,
                tag.Desc
            });

            return tag.Id;
        }

        #endregion

        #region 2. READ (FETCH & QUERY)

        /// <summary>
        /// Mengambil semua Tag (diurutkan alfabetis) secara asinkron.
        /// </summary>
        public async Task<IEnumerable<TagModel>> GetAllTagsAsync()
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "SELECT * FROM tags ORDER BY name ASC;";
            return await conn.QueryAsync<TagModel>(sql);
        }

        /// <summary>
        /// Mengambil detail satu Tag berdasarkan ID secara asinkron.
        /// </summary>
        public async Task<TagModel?> GetTagByIdAsync(Guid id)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "SELECT * FROM tags WHERE id = @Id LIMIT 1;";
            return await conn.QueryFirstOrDefaultAsync<TagModel>(sql, new { Id = id.ToString() });
        }

        /// <summary>
        /// Mengambil seluruh Tag yang dimiliki oleh game tertentu secara asinkron.
        /// (Untuk menampilkan chip/badge tag di UI Game Detail)
        /// </summary>
        public async Task<IEnumerable<TagModel>> GetTagsByAppIdAsync(Guid appId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"
                SELECT t.* FROM tags t
                INNER JOIN app_tags at ON t.id = at.tag_id
                WHERE at.app_id = @AppId
                ORDER BY t.name ASC;";

            return await conn.QueryAsync<TagModel>(sql, new { AppId = appId.ToString() });
        }

        /// <summary>
        /// Mencari Tag berdasarkan kata kunci nama secara asinkron.
        /// </summary>
        public async Task<IEnumerable<TagModel>> GetTagsFilteredAsync(string? searchQuery = null)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            var sql = "SELECT * FROM tags";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                sql += " WHERE name LIKE @Query";
                parameters.Add("Query", $"%{searchQuery}%");
            }

            sql += " ORDER BY name ASC;";

            return await conn.QueryAsync<TagModel>(sql, parameters);
        }

        #endregion

        #region 3. UPDATE

        /// <summary>
        /// Mengubah nama dan deskripsi Tag secara asinkron.
        /// </summary>
        public async Task<bool> UpdateTagAsync(TagModel tag)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"
                UPDATE tags SET
                    name = @Name,
                    desc = @Desc
                WHERE id = @Id;";

            int rowsAffected = await conn.ExecuteAsync(sql, new
            {
                Id = tag.Id.ToString(),
                tag.Name,
                tag.Desc
            });

            return rowsAffected > 0;
        }

        #endregion

        #region 4. RELATIONAL MANAGEMENT (APP <-> TAG)

        /// <summary>
        /// Menghubungkan game dengan suatu Tag secara asinkron.
        /// </summary>
        public async Task<bool> AssignTagToAppAsync(Guid appId, Guid tagId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "INSERT OR IGNORE INTO app_tags (app_id, tag_id) VALUES (@AppId, @TagId);";
            int rowsAffected = await conn.ExecuteAsync(sql, new { AppId = appId.ToString(), TagId = tagId.ToString() });
            return rowsAffected > 0;
        }

        /// <summary>
        /// Menghapus hubungan Tag dari game tertentu secara asinkron.
        /// </summary>
        public async Task<bool> RemoveTagFromAppAsync(Guid appId, Guid tagId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "DELETE FROM app_tags WHERE app_id = @AppId AND tag_id = @TagId;";
            int rowsAffected = await conn.ExecuteAsync(sql, new { AppId = appId.ToString(), TagId = tagId.ToString() });
            return rowsAffected > 0;
        }

        #endregion

        #region 5. DELETE

        /// <summary>
        /// Hapus permanen Tag dari database secara asinkron.
        /// </summary>
        public async Task<bool> DeleteTagAsync(Guid id)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "DELETE FROM tags WHERE id = @Id;";
            int rowsAffected = await conn.ExecuteAsync(sql, new { Id = id.ToString() });
            return rowsAffected > 0;
        }

        #endregion
    }
}