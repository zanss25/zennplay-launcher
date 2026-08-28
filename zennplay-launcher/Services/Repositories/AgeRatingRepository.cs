using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Repositories
{
    public class AgeRatingRepository
    {
        #region 1. CREATE (INSERT)

        /// <summary>
        /// Menambahkan age rating baru ke database secara asinkron.
        /// </summary>
        public async Task<Guid> AddAgeRatingAsync(AgeRatingModel ageRating)
        {
            if (ageRating.Id == Guid.Empty)
            {
                ageRating.Id = Guid.NewGuid();
            }

            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sqlAgeRating = @"
                INSERT INTO age_ratings (id, name, desc) 
                VALUES (@Id, @Name, @Desc);";

            await conn.ExecuteAsync(sqlAgeRating, new
            {
                Id = ageRating.Id.ToString(),
                ageRating.Name,
                ageRating.Desc
            });

            return ageRating.Id;
        }

        #endregion

        #region 2. READ (FETCH & QUERY)

        /// <summary>
        /// Mengambil semua age rating (diurutkan alfabetis) secara asinkron.
        /// </summary>
        public async Task<IEnumerable<AgeRatingModel>> GetAllAgeRatingsAsync()
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "SELECT * FROM age_ratings ORDER BY name ASC;";
            return await conn.QueryAsync<AgeRatingModel>(sql);
        }

        /// <summary>
        /// Mengambil detail satu age rating berdasarkan ID secara asinkron.
        /// </summary>
        public async Task<AgeRatingModel?> GetAgeRatingByIdAsync(Guid id)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "SELECT * FROM age_ratings WHERE id = @Id LIMIT 1;";
            return await conn.QueryFirstOrDefaultAsync<AgeRatingModel>(sql, new { Id = id.ToString() });
        }

        /// <summary>
        /// Mengambil seluruh age rating yang dimiliki oleh game tertentu secara asinkron.
        /// </summary>
        public async Task<IEnumerable<AgeRatingModel>> GetAgeRatingsByAppIdAsync(Guid appId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"
                SELECT r.* FROM age_ratings r
                INNER JOIN app_age_ratings ar ON r.id = ar.age_rating_id
                WHERE ar.app_id = @AppId
                ORDER BY r.name ASC;";

            return await conn.QueryAsync<AgeRatingModel>(sql, new { AppId = appId.ToString() });
        }

        /// <summary>
        /// Mencari age rating berdasarkan kata kunci nama secara asinkron.
        /// </summary>
        public async Task<IEnumerable<AgeRatingModel>> GetAgeRatingsFilteredAsync(string? searchQuery = null)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            var sql = "SELECT * FROM age_ratings";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                sql += " WHERE name LIKE @Query";
                parameters.Add("Query", $"%{searchQuery}%");
            }

            sql += " ORDER BY name ASC;";

            return await conn.QueryAsync<AgeRatingModel>(sql, parameters);
        }

        #endregion

        #region 3. UPDATE

        /// <summary>
        /// Mengubah nama dan deskripsi age rating secara asinkron.
        /// </summary>
        public async Task<bool> UpdateAgeRatingAsync(AgeRatingModel ageRating)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"
                UPDATE age_ratings SET
                    name = @Name,
                    desc = @Desc
                WHERE id = @Id;";

            int rowsAffected = await conn.ExecuteAsync(sql, new
            {
                Id = ageRating.Id.ToString(),
                ageRating.Name,
                ageRating.Desc
            });

            return rowsAffected > 0;
        }

        #endregion

        #region 4. RELATIONAL MANAGEMENT (APP <-> AGE RATING)

        /// <summary>
        /// Menghubungkan game dengan suatu AgeRating secara asinkron.
        /// </summary>
        public async Task<bool> AssignAgeRatingToAppAsync(Guid appId, Guid ageRatingId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "INSERT OR IGNORE INTO app_age_ratings (app_id, age_rating_id) VALUES (@AppId, @AgeRatingId);";
            int rowsAffected = await conn.ExecuteAsync(sql, new { AppId = appId.ToString(), AgeRatingId = ageRatingId.ToString() });
            return rowsAffected > 0;
        }

        /// <summary>
        /// Menghapus hubungan age rating dari game tertentu secara asinkron.
        /// </summary>
        public async Task<bool> RemoveAgeRatingFromAppAsync(Guid appId, Guid ageRatingId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "DELETE FROM app_age_ratings WHERE app_id = @AppId AND age_rating_id = @AgeRatingId;";
            int rowsAffected = await conn.ExecuteAsync(sql, new { AppId = appId.ToString(), AgeRatingId = ageRatingId.ToString() });
            return rowsAffected > 0;
        }

        #endregion

        #region 5. DELETE

        /// <summary>
        /// Hapus permanen AgeRating dari database secara asinkron.
        /// </summary>
        public async Task<bool> DeleteAgeRatingAsync(Guid id)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "DELETE FROM age_ratings WHERE id = @Id;";
            int rowsAffected = await conn.ExecuteAsync(sql, new { Id = id.ToString() });
            return rowsAffected > 0;
        }

        #endregion
    }
}