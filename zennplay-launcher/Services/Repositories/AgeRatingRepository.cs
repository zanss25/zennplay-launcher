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

        public Guid AddAgeRating(AgeRatingModel ageRating)
        {
            if (ageRating.Id == Guid.Empty)
            {
                ageRating.Id = Guid.NewGuid();
            }

            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sqlAgeRating = @"
                        INSERT INTO age_ratings (id, name, desc) 
                        VALUES (@Id, @Name, @Desc);";

                // 🔑 Gunakan anonymous object agar Guid tersimpan lowercase konsisten
                conn.Execute(sqlAgeRating, new
                {
                    Id = ageRating.Id.ToString(),
                    ageRating.Name,
                    ageRating.Desc
                });

                return ageRating.Id;
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
        public AgeRatingModel? GetAgeRatingById(Guid id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "SELECT * FROM age_ratings WHERE id = @Id LIMIT 1;";
                return conn.QueryFirstOrDefault<AgeRatingModel>(sql, new { Id = id.ToString() });
            }
        }

        /// <summary>
        /// Mengambil seluruh ageRating yang dimiliki oleh game tertentu.
        /// </summary>
        public IEnumerable<AgeRatingModel> GetAgeRatingsByAppId(Guid appId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    SELECT r.* FROM age_ratings r
                    INNER JOIN app_age_ratings ar ON r.id = ar.age_rating_id
                    WHERE ar.app_id = @AppId
                    ORDER BY r.name ASC;";

                return conn.Query<AgeRatingModel>(sql, new { AppId = appId.ToString() });
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

        public bool UpdateAgeRating(AgeRatingModel ageRating)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                        UPDATE age_ratings SET
                            name = @Name,
                            desc = @Desc
                        WHERE id = @Id;";

                // 🔑 Gunakan anonymous object di sini juga
                return conn.Execute(sql, new
                {
                    Id = ageRating.Id.ToString(),
                    ageRating.Name,
                    ageRating.Desc
                }) > 0;
            }
        }

        #endregion

        #region 4. RELATIONAL MANAGEMENT (APP <-> AGE RATING)

        /// <summary>
        /// Menghubungkan game dengan suatu AgeRating.
        /// </summary>
        public bool AssignAgeRatingToApp(Guid appId, Guid ageRatingId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "INSERT OR IGNORE INTO app_age_ratings (app_id, age_rating_id) VALUES (@AppId, @AgeRatingId);";
                return conn.Execute(sql, new { AppId = appId.ToString(), AgeRatingId = ageRatingId.ToString() }) > 0;
            }
        }

        /// <summary>
        /// Menghapus hubungan ageRating dari game tertentu.
        /// </summary>
        public bool RemoveAgeRatingFromApp(Guid appId, Guid ageRatingId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM app_age_ratings WHERE app_id = @AppId AND age_rating_id = @AgeRatingId;";
                return conn.Execute(sql, new { AppId = appId.ToString(), AgeRatingId = ageRatingId.ToString() }) > 0;
            }
        }

        #endregion

        #region 5. DELETE

        /// <summary>
        /// Hapus permanen AgeRating dari database.
        /// </summary>
        public bool DeleteAgeRating(Guid id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM age_ratings WHERE id = @Id;";
                return conn.Execute(sql, new { Id = id.ToString() }) > 0;
            }
        }

        #endregion
    }
}