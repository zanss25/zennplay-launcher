using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Repositories
{
    public class TagRepository
    {
        #region 1. CREATE (INSERT)

        /// <summary>
        /// Menambahkan Tag baru ke database.
        /// </summary>
        public int AddTag(TagModel tag)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sqlTag = @"
                    INSERT INTO tags (name, desc) 
                    VALUES (@Name, @Desc);
                    SELECT last_insert_rowid();";

                int newTagId = conn.ExecuteScalar<int>(sqlTag, tag);
                tag.Id = newTagId;
                return newTagId;
            }
        }

        #endregion

        #region 2. READ (FETCH & QUERY)

        /// <summary>
        /// Mengambil semua Tag (diurutkan alfabetis).
        /// </summary>
        public IEnumerable<TagModel> GetAllTags()
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "SELECT * FROM tags ORDER BY name ASC;";
                return conn.Query<TagModel>(sql);
            }
        }

        /// <summary>
        /// Mengambil detail satu Tag berdasarkan ID.
        /// </summary>
        public TagModel? GetTagById(int id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "SELECT * FROM tags WHERE id = @Id LIMIT 1;";
                return conn.QueryFirstOrDefault<TagModel>(sql, new { Id = id });
            }
        }

        /// <summary>
        /// Mengambil seluruh Tag yang dimiliki oleh game tertentu.
        /// (Untuk menampilkan chip/badge tag di UI Game Detail)
        /// </summary>
        public IEnumerable<TagModel> GetTagsByAppId(int appId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    SELECT t.* FROM tags t
                    INNER JOIN app_tags at ON t.id = at.tag_id
                    WHERE at.app_id = @AppId
                    ORDER BY t.name ASC;";

                return conn.Query<TagModel>(sql, new { AppId = appId });
            }
        }

        /// <summary>
        /// Mencari Tag berdasarkan kata kunci nama.
        /// </summary>
        public IEnumerable<TagModel> GetTagsFiltered(string? searchQuery = null)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                var sql = "SELECT * FROM tags";
                var parameters = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    sql += " WHERE name LIKE @Query";
                    parameters.Add("Query", $"%{searchQuery}%");
                }

                sql += " ORDER BY name ASC;";

                return conn.Query<TagModel>(sql, parameters);
            }
        }

        #endregion

        #region 3. UPDATE

        /// <summary>
        /// Mengubah nama dan deskripsi Tag.
        /// </summary>
        public bool UpdateTag(TagModel tag)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    UPDATE tags SET
                        name = @Name,
                        desc = @Desc
                    WHERE id = @Id;";

                return conn.Execute(sql, tag) > 0;
            }
        }

        #endregion

        #region 4. RELATIONAL MANAGEMENT (APP <-> TAG)

        /// <summary>
        /// Menghubungkan game dengan suatu Tag.
        /// </summary>
        public bool AssignTagToApp(int appId, int tagId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "INSERT OR IGNORE INTO app_tags (app_id, tag_id) VALUES (@AppId, @TagId);";
                return conn.Execute(sql, new { AppId = appId, TagId = tagId }) > 0;
            }
        }

        /// <summary>
        /// Menghapus hubungan Tag dari game tertentu.
        /// </summary>
        public bool RemoveTagFromApp(int appId, int tagId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM app_tags WHERE app_id = @AppId AND tag_id = @TagId;";
                return conn.Execute(sql, new { AppId = appId, TagId = tagId }) > 0;
            }
        }

        #endregion

        #region 5. DELETE

        /// <summary>
        /// Hapus permanen Tag dari database.
        /// (ON DELETE CASCADE pada DB akan otomatis membersihkan relasi di 'app_tags').
        /// </summary>
        public bool DeleteTag(int id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM tags WHERE id = @Id;";
                return conn.Execute(sql, new { Id = id }) > 0;
            }
        }

        #endregion
    }
}