using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Repositories
{
    public class AgeRatingRepository
    {
        #region 1. CREATE (INSERT)

        /// <summary>
        /// Menambahkan ageRating baru ke database.
        /// </summary>
        public int AddAgeRating(AgeRatingModel ageRating)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sqlAgeRating = @"
                    INSERT INTO age_ratings (name, desc) 
                    VALUES (@Name, @Desc);
                    SELECT last_insert_rowid();";

                int newAgeRatingId = conn.ExecuteScalar<int>(sqlAgeRating, ageRating);
                ageRating.Id = newAgeRatingId;
                return newAgeRatingId;
            }
        }

        #endregion

        #region 2. READ (FETCH & QUERY)

        /// <summary>
        /// Mengambil semua ageRating (diurutkan alfabetis).
        /// </summary>
        public IEnumerable<AgeRatingModel> GetAllAgeRatings()
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "SELECT * FROM age_ratings ORDER BY name ASC;";
                return conn.Query<AgeRatingModel>(sql);
            }
        }

        /// <summary>
        /// Mengambil detail satu ageRating berdasarkan ID.
        /// </summary>
        public AgeRatingModel? GetAgeRatingById(int id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                // PERBAIKAN: Mengubah tabel 'apps' menjadi 'ageRatings'
                string sql = "SELECT * FROM age_ratings WHERE id = @Id LIMIT 1;";
                return conn.QueryFirstOrDefault<AgeRatingModel>(sql, new { Id = id });
            }
        }

        /// <summary>
        /// Mengambil seluruh ageRating yang dimiliki oleh game tertentu.
        /// </summary>
        public IEnumerable<AgeRatingModel> GetAgeRatingsByAppId(int appId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    SELECT r.* FROM age_ratings r
                    INNER JOIN app_age_ratings ar ON r.id = ar.age_rating_id
                    WHERE ar.app_id = @AppId
                    ORDER BY r.name ASC;";

                return conn.Query<AgeRatingModel>(sql, new { AppId = appId });
            }
        }

        /// <summary>
        /// Mencari ageRating berdasarkan kata kunci nama.
        /// </summary>
        public IEnumerable<AgeRatingModel> GetAgeRatingsFiltered(string? searchQuery = null)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                var sql = "SELECT * FROM age_ratings";
                var parameters = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    sql += " WHERE name LIKE @Query";
                    parameters.Add("Query", $"%{searchQuery}%");
                }

                sql += " ORDER BY name ASC;";

                return conn.Query<AgeRatingModel>(sql, parameters);
            }
        }

        #endregion

        #region 3. UPDATE

        /// <summary>
        /// Mengubah nama dan deskripsi ageRating.
        /// </summary>
        public bool UpdateAgeRating(AgeRatingModel ageRating)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    UPDATE age_ratings SET
                        name = @Name,
                        desc = @Desc
                    WHERE id = @Id;";

                return conn.Execute(sql, ageRating) > 0;
            }
        }

        #endregion

        #region 4. RELATIONAL MANAGEMENT (APP <-> GENRE)

        /// <summary>
        /// Menghubungkan game dengan suatu AgeRating.
        /// </summary>
        public bool AssignAgeRatingToApp(int appId, int ageRatingId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "INSERT OR IGNORE INTO app_age_ratings (app_id, age_rating_id) VALUES (@AppId, @AgeRatingId);";
                return conn.Execute(sql, new { AppId = appId, AgeRatingId = ageRatingId }) > 0;
            }
        }

        /// <summary>
        /// Menghapus hubungan ageRating dari game tertentu.
        /// </summary>
        public bool RemoveAgeRatingFromApp(int appId, int ageRatingId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM app_age_ratings WHERE app_id = @AppId AND age_rating_id = @AgeRatingId;";
                return conn.Execute(sql, new { AppId = appId, AgeRatingId = ageRatingId }) > 0;
            }
        }

        #endregion

        #region 5. DELETE

        /// <summary>
        /// Hapus permanen AgeRating dari database.
        /// (ON DELETE CASCADE pada DB akan otomatis membersihkan relasi di 'app_ageRatings').
        /// </summary>
        public bool DeleteAgeRating(int id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM age_ratings WHERE id = @Id;";
                return conn.Execute(sql, new { Id = id }) > 0;
            }
        }

        #endregion
    }
}
