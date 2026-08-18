using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Repositories
{
    public class GenreRepository
    {
        #region 1. CREATE (INSERT)

        /// <summary>
        /// Menambahkan genre baru ke database.
        /// </summary>
        public Guid AddGenre(GenreModel genre)
        {
            // Pastikan entitas memiliki ID sebelum disimpan
            if (genre.Id == Guid.Empty)
            {
                genre.Id = Guid.NewGuid();
            }

            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sqlGenre = @"
                    INSERT INTO genres (id, name, desc) 
                    VALUES (@Id, @Name, @Desc);";

                conn.Execute(sqlGenre, new
                {
                    Id = genre.Id.ToString(),
                    genre.Name,
                    genre.Desc
                });

                return genre.Id;
            }
        }

        #endregion

        #region 2. READ (FETCH & QUERY)

        /// <summary>
        /// Mengambil semua genre (diurutkan alfabetis).
        /// </summary>
        public IEnumerable<GenreModel> GetAllGenres()
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "SELECT * FROM genres ORDER BY name ASC;";
                return conn.Query<GenreModel>(sql);
            }
        }

        /// <summary>
        /// Mengambil detail satu genre berdasarkan ID.
        /// </summary>
        public GenreModel? GetGenreById(Guid id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "SELECT * FROM genres WHERE id = @Id LIMIT 1;";
                return conn.QueryFirstOrDefault<GenreModel>(sql, new { Id = id.ToString() });
            }
        }

        /// <summary>
        /// Mengambil seluruh genre yang dimiliki oleh game tertentu.
        /// </summary>
        public IEnumerable<GenreModel> GetGenresByAppId(Guid appId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    SELECT g.* FROM genres g
                    INNER JOIN app_genres ag ON g.id = ag.genre_id
                    WHERE ag.app_id = @AppId
                    ORDER BY g.name ASC;";

                return conn.Query<GenreModel>(sql, new { AppId = appId.ToString() });
            }
        }

        /// <summary>
        /// Mencari genre berdasarkan kata kunci nama.
        /// </summary>
        public IEnumerable<GenreModel> GetGenresFiltered(string? searchQuery = null)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                var sql = "SELECT * FROM genres";
                var parameters = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    sql += " WHERE name LIKE @Query";
                    parameters.Add("Query", $"%{searchQuery}%");
                }

                sql += " ORDER BY name ASC;";

                return conn.Query<GenreModel>(sql, parameters);
            }
        }

        #endregion

        #region 3. UPDATE

        /// <summary>
        /// Mengubah nama dan deskripsi genre.
        /// </summary>
        public bool UpdateGenre(GenreModel genre)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"
                    UPDATE genres SET
                        name = @Name,
                        desc = @Desc
                    WHERE id = @Id;";

                return conn.Execute(sql, new
                {
                    Id = genre.Id.ToString(),
                    genre.Name,
                    genre.Desc
                }) > 0;
            }
        }

        #endregion

        #region 4. RELATIONAL MANAGEMENT (APP <-> GENRE)

        /// <summary>
        /// Menghubungkan game dengan suatu Genre.
        /// </summary>
        public bool AssignGenreToApp(Guid appId, Guid genreId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "INSERT OR IGNORE INTO app_genres (app_id, genre_id) VALUES (@AppId, @GenreId);";
                return conn.Execute(sql, new { AppId = appId.ToString(), GenreId = genreId.ToString() }) > 0;
            }
        }

        /// <summary>
        /// Menghapus hubungan genre dari game tertentu.
        /// </summary>
        public bool RemoveGenreFromApp(Guid appId, Guid genreId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM app_genres WHERE app_id = @AppId AND genre_id = @GenreId;";
                return conn.Execute(sql, new { AppId = appId.ToString(), GenreId = genreId.ToString() }) > 0;
            }
        }

        #endregion

        #region 5. DELETE

        /// <summary>
        /// Hapus permanen Genre dari database.
        /// (ON DELETE CASCADE pada DB akan otomatis membersihkan relasi di 'app_genres').
        /// </summary>
        public bool DeleteGenre(Guid id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "DELETE FROM genres WHERE id = @Id;";
                return conn.Execute(sql, new { Id = id.ToString() }) > 0;
            }
        }

        #endregion
    }
}