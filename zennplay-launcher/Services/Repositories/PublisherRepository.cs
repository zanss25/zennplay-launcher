using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Repositories
{
    public class PublisherRepository
    {
        #region 1. CREATE (INSERT)

        /// <summary>
        /// Menambahkan publisher baru ke database.
        /// </summary>
        public int AddPublisher(PublisherModel publisher)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sqlPublisher = @"
                    INSERT INTO publishers (name, desc) 
                    VALUES (@Name, @Desc);
                    SELECT last_insert_rowid();";

                int newPublisherId = conn.ExecuteScalar<int>(sqlPublisher, publisher);
                publisher.Id = newPublisherId;
                return newPublisherId;
            }
        }

        #endregion

        #region 2. READ (FETCH & QUERY)

        /// <summary>
        /// Mengambil semua publisher (diurutkan alfabetis).
        /// </summary>
        public IEnumerable<PublisherModel> GetAllPublishers()
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "SELECT * FROM publishers ORDER BY name ASC;";
                return conn.Query<PublisherModel>(sql);
            }
        }

        /// <summary>
        /// Mengambil detail satu publisher berdasarkan ID.
        /// </summary>
        public PublisherModel? GetPublisherById(int id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                // PERBAIKAN: Mengubah tabel 'apps' menjadi 'publishers'
                string sql = "SELECT * FROM publishers WHERE id = @Id LIMIT 1;";
                return conn.QueryFirstOrDefault<PublisherModel>(sql, new { Id = id });
            }
        }

        /// <summary>
        /// Mengambil seluruh publisher yang dimiliki oleh game tertentu.
        /// </summary>
        public IEnumerable<PublisherModel> GetPublishersByAppId(int appId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    SELECT p.* FROM publishers p
                    INNER JOIN app_publishers ap ON p.id = ap.publisher_id
                    WHERE ap.app_id = @AppId
                    ORDER BY p.name ASC;";

                return conn.Query<PublisherModel>(sql, new { AppId = appId });
            }
        }

        /// <summary>
        /// Mencari publisher berdasarkan kata kunci nama.
        /// </summary>
        public IEnumerable<PublisherModel> GetPublishersFiltered(string? searchQuery = null)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                var sql = "SELECT * FROM publishers";
                var parameters = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    sql += " WHERE name LIKE @Query";
                    parameters.Add("Query", $"%{searchQuery}%");
                }

                sql += " ORDER BY name ASC;";

                return conn.Query<PublisherModel>(sql, parameters);
            }
        }

        #endregion

        #region 3. UPDATE

        /// <summary>
        /// Mengubah nama dan deskripsi publisher.
        /// </summary>
        public bool UpdatePublisher(PublisherModel publisher)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    UPDATE publishers SET
                        name = @Name,
                        desc = @Desc
                    WHERE id = @Id;";

                return conn.Execute(sql, publisher) > 0;
            }
        }

        #endregion

        #region 4. RELATIONAL MANAGEMENT (APP <-> GENRE)

        /// <summary>
        /// Menghubungkan game dengan suatu Publisher.
        /// </summary>
        public bool AssignPublisherToApp(int appId, int publisherId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "INSERT OR IGNORE INTO app_publishers (app_id, publisher_id) VALUES (@AppId, @PublisherId);";
                return conn.Execute(sql, new { AppId = appId, PublisherId = publisherId }) > 0;
            }
        }

        /// <summary>
        /// Menghapus hubungan publisher dari game tertentu.
        /// </summary>
        public bool RemovePublisherFromApp(int appId, int publisherId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM app_publishers WHERE app_id = @AppId AND publisher_id = @PublisherId;";
                return conn.Execute(sql, new { AppId = appId, PublisherId = publisherId }) > 0;
            }
        }

        #endregion

        #region 5. DELETEGenre

        /// <summary>
        /// Hapus permanen Publisher dari database.
        /// (ON DELETE CASCADE pada DB akan otomatis membersihkan relasi di 'app_publishers').
        /// </summary>
        public bool DeletePublisher(int id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM publishers WHERE id = @Id;";
                return conn.Execute(sql, new { Id = id }) > 0;
            }
        }

        #endregion
    }
}
