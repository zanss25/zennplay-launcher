using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Repositories
{
    public class DeveloperRepository
    {
        #region 1. CREATE (INSERT)

        /// <summary>
        /// Menambahkan developer baru ke database.
        /// </summary>
        public Guid AddDeveloper(DeveloperModel developer)
        {
            // Pastikan entitas memiliki ID sebelum disimpan
            if (developer.Id == Guid.Empty)
            {
                developer.Id = Guid.NewGuid();
            }

            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sqlDeveloper = @"
                    INSERT INTO developers (id, name, desc) 
                    VALUES (@Id, @Name, @Desc);";

                conn.Execute(sqlDeveloper, new
                {
                    Id = developer.Id.ToString(),
                    developer.Name,
                    developer.Desc
                });

                return developer.Id;
            }
        }

        #endregion

        #region 2. READ (FETCH & QUERY)

        /// <summary>
        /// Mengambil semua developer (diurutkan alfabetis).
        /// </summary>
        public IEnumerable<DeveloperModel> GetAllDevelopers()
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "SELECT * FROM developers ORDER BY name ASC;";
                return conn.Query<DeveloperModel>(sql);
            }
        }

        /// <summary>
        /// Mengambil detail satu developer berdasarkan ID.
        /// </summary>
        public DeveloperModel? GetDeveloperById(Guid id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "SELECT * FROM developers WHERE id = @Id LIMIT 1;";
                return conn.QueryFirstOrDefault<DeveloperModel>(sql, new { Id = id.ToString() });
            }
        }

        /// <summary>
        /// Mengambil seluruh developer yang dimiliki oleh game tertentu.
        /// </summary>
        public IEnumerable<DeveloperModel> GetDevelopersByAppId(Guid appId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    SELECT d.* FROM developers d
                    INNER JOIN app_developers ad ON d.id = ad.developer_id
                    WHERE ad.app_id = @AppId
                    ORDER BY d.name ASC;";

                return conn.Query<DeveloperModel>(sql, new { AppId = appId.ToString() });
            }
        }

        /// <summary>
        /// Mencari developer berdasarkan kata kunci nama.
        /// </summary>
        public IEnumerable<DeveloperModel> GetDevelopersFiltered(string? searchQuery = null)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                var sql = "SELECT * FROM developers";
                var parameters = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    sql += " WHERE name LIKE @Query";
                    parameters.Add("Query", $"%{searchQuery}%");
                }

                sql += " ORDER BY name ASC;";

                return conn.Query<DeveloperModel>(sql, parameters);
            }
        }

        #endregion

        #region 3. UPDATE

        /// <summary>
        /// Mengubah nama dan deskripsi developer.
        /// </summary>
        public bool UpdateDeveloper(DeveloperModel developer)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    UPDATE developers SET
                        name = @Name,
                        desc = @Desc
                    WHERE id = @Id;";

                return conn.Execute(sql, new
                {
                    Id = developer.Id.ToString(),
                    developer.Name,
                    developer.Desc
                }) > 0;
            }
        }

        #endregion

        #region 4. RELATIONAL MANAGEMENT (APP <-> DEVELOPER)

        /// <summary>
        /// Menghubungkan game dengan suatu Developer.
        /// </summary>
        public bool AssignDeveloperToApp(Guid appId, Guid developerId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "INSERT OR IGNORE INTO app_developers (app_id, developer_id) VALUES (@AppId, @DeveloperId);";
                return conn.Execute(sql, new { AppId = appId.ToString(), DeveloperId = developerId.ToString() }) > 0;
            }
        }

        /// <summary>
        /// Menghapus hubungan developer dari game tertentu.
        /// </summary>
        public bool RemoveDeveloperFromApp(Guid appId, Guid developerId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM app_developers WHERE app_id = @AppId AND developer_id = @DeveloperId;";
                return conn.Execute(sql, new { AppId = appId.ToString(), DeveloperId = developerId.ToString() }) > 0;
            }
        }

        #endregion

        #region 5. DELETE

        /// <summary>
        /// Hapus permanen Developer dari database.
        /// (ON DELETE CASCADE pada DB akan otomatis membersihkan relasi di 'app_developers').
        /// </summary>
        public bool DeleteDeveloper(Guid id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM developers WHERE id = @Id;";
                return conn.Execute(sql, new { Id = id.ToString() }) > 0;
            }
        }

        #endregion
    }
}