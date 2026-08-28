using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Repositories
{
    public class GenreRepository
    {
        #region 1. CREATE (INSERT)

        /// <summary>
        /// Menambahkan genre baru ke database secara asinkron.
        /// </summary>
        public async Task<Guid> AddGenreAsync(GenreModel genre)
        {
            if (genre.Id == Guid.Empty)
            {
                genre.Id = Guid.NewGuid();
            }

            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sqlGenre = @"
                INSERT INTO genres (id, name, desc) 
                VALUES (@Id, @Name, @Desc);";

            await conn.ExecuteAsync(sqlGenre, new
            {
                Id = genre.Id.ToString(),
                genre.Name,
                genre.Desc
            });

            return genre.Id;
        }

        #endregion

        #region 2. READ (FETCH & QUERY)

        /// <summary>
        /// Mengambil semua genre (diurutkan alfabetis) secara asinkron.
        /// </summary>
        public async Task<IEnumerable<GenreModel>> GetAllGenresAsync()
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "SELECT * FROM genres ORDER BY name ASC;";
            return await conn.QueryAsync<GenreModel>(sql);
        }

        /// <summary>
        /// Mengambil detail satu genre berdasarkan ID secara asinkron.
        /// </summary>
        public async Task<GenreModel?> GetGenreByIdAsync(Guid id)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "SELECT * FROM genres WHERE id = @Id LIMIT 1;";
            return await conn.QueryFirstOrDefaultAsync<GenreModel>(sql, new { Id = id.ToString() });
        }

        /// <summary>
        /// Mengambil seluruh genre yang dimiliki oleh game tertentu secara asinkron.
        /// </summary>
        public async Task<IEnumerable<GenreModel>> GetGenresByAppIdAsync(Guid appId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"
                SELECT g.* FROM genres g
                INNER JOIN app_genres ag ON g.id = ag.genre_id
                WHERE ag.app_id = @AppId
                ORDER BY g.name ASC;";

            return await conn.QueryAsync<GenreModel>(sql, new { AppId = appId.ToString() });
        }

        /// <summary>
        /// Mencari genre berdasarkan kata kunci nama secara asinkron.
        /// </summary>
        public async Task<IEnumerable<GenreModel>> GetGenresFilteredAsync(string? searchQuery = null)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            var sql = "SELECT * FROM genres";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                sql += " WHERE name LIKE @Query";
                parameters.Add("Query", $"%{searchQuery}%");
            }

            sql += " ORDER BY name ASC;";

            return await conn.QueryAsync<GenreModel>(sql, parameters);
        }

        #endregion

        #region 3. UPDATE

        /// <summary>
        /// Mengubah nama dan deskripsi genre secara asinkron.
        /// </summary>
        public async Task<bool> UpdateGenreAsync(GenreModel genre)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"
                UPDATE genres SET
                    name = @Name,
                    desc = @Desc
                WHERE id = @Id;";

            int rowsAffected = await conn.ExecuteAsync(sql, new
            {
                Id = genre.Id.ToString(),
                genre.Name,
                genre.Desc
            });

            return rowsAffected > 0;
        }

        #endregion

        #region 4. RELATIONAL MANAGEMENT (APP <-> GENRE)

        /// <summary>
        /// Menghubungkan game dengan suatu Genre secara asinkron.
        /// </summary>
        public async Task<bool> AssignGenreToAppAsync(Guid appId, Guid genreId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "INSERT OR IGNORE INTO app_genres (app_id, genre_id) VALUES (@AppId, @GenreId);";
            int rowsAffected = await conn.ExecuteAsync(sql, new { AppId = appId.ToString(), GenreId = genreId.ToString() });
            return rowsAffected > 0;
        }

        /// <summary>
        /// Menghapus hubungan genre dari game tertentu secara asinkron.
        /// </summary>
        public async Task<bool> RemoveGenreFromAppAsync(Guid appId, Guid genreId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "DELETE FROM app_genres WHERE app_id = @AppId AND genre_id = @GenreId;";
            int rowsAffected = await conn.ExecuteAsync(sql, new { AppId = appId.ToString(), GenreId = genreId.ToString() });
            return rowsAffected > 0;
        }

        #endregion

        #region 5. DELETE

        /// <summary>
        /// Hapus permanen Genre dari database secara asinkron.
        /// </summary>
        public async Task<bool> DeleteGenreAsync(Guid id)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "DELETE FROM genres WHERE id = @Id;";
            int rowsAffected = await conn.ExecuteAsync(sql, new { Id = id.ToString() });
            return rowsAffected > 0;
        }

        #endregion
    }
}