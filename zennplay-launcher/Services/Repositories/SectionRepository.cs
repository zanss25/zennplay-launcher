using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Repositories
{
    public class SectionRepository
    {
        #region 1. READ (FETCH)

        /// <summary>
        /// Mengambil semua Section/Kategori yang ada secara asinkron, diurutkan berdasarkan sort_order kustom dari user.
        /// </summary>
        public async Task<IEnumerable<SectionModel>> GetAllSectionsAsync()
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "SELECT * FROM sections ORDER BY sort_order ASC, created_at ASC;";
            return await conn.QueryAsync<SectionModel>(sql);
        }

        /// <summary>
        /// Mengambil detail satu Section berdasarkan ID secara asinkron.
        /// </summary>
        public async Task<SectionModel?> GetSectionByIdAsync(Guid id)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "SELECT * FROM sections WHERE id = @Id LIMIT 1;";
            return await conn.QueryFirstOrDefaultAsync<SectionModel>(sql, new { Id = id.ToString() });
        }

        /// <summary>
        /// Mengambil jumlah total Section yang tersisa di database secara asinkron.
        /// </summary>
        public async Task<int> GetSectionCountAsync()
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "SELECT COUNT(*) FROM sections;";
            return await conn.ExecuteScalarAsync<int>(sql);
        }

        /// <summary>
        /// Mengambil semua game yang saat ini "DITITIPKAN" (Sectionless) secara asinkron.
        /// </summary>
        public async Task<IEnumerable<AppModel>> GetUnassignedAppsAsync()
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"SELECT a.* FROM apps a
                           LEFT JOIN app_sections aps ON a.id = aps.app_id
                           WHERE aps.section_id IS NULL AND a.is_deleted = 0
                           ORDER BY a.title ASC;";

            return await conn.QueryAsync<AppModel>(sql);
        }

        /// <summary>
        /// Mengambil jumlah total game yang ada di dalam Section tertentu secara asinkron.
        /// </summary>
        public async Task<int> GetAppCountInSectionAsync(Guid sectionId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"SELECT COUNT(a.id) 
                           FROM apps a
                           INNER JOIN app_sections aps ON a.id = aps.app_id
                           WHERE aps.section_id = @SectionId AND a.is_deleted = 0;";

            return await conn.ExecuteScalarAsync<int>(sql, new { SectionId = sectionId.ToString() });
        }

        #endregion

        #region 2. CREATE & UPDATE

        /// <summary>
        /// Menambahkan Section baru secara asinkron.
        /// </summary>
        public async Task<Guid> AddSectionAsync(SectionModel section)
        {
            if (section.Id == Guid.Empty)
            {
                section.Id = Guid.NewGuid();
            }

            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Jika sort_order bernilai default (0), otomatis taruh di urutan paling belakang
                if (section.SortOrder == 0)
                {
                    string getMaxOrderSql = "SELECT COALESCE(MAX(sort_order), -1) + 1 FROM sections;";
                    section.SortOrder = await conn.ExecuteScalarAsync<int>(getMaxOrderSql, transaction: transaction);
                }

                string sql = @"INSERT INTO sections (id, name, desc, icon, app_type, sort_order, is_game_by_default, is_excluded_by_default)
                               VALUES (@Id, @Name, @Desc, @Icon, @AppType, @SortOrder, @IsGameByDefault, @IsExcludedByDefault);";

                await conn.ExecuteAsync(sql, new
                {
                    Id = section.Id.ToString(),
                    section.Name,
                    section.Desc,
                    section.Icon,
                    section.AppType,
                    section.SortOrder,
                    section.IsGameByDefault,
                    section.IsExcludedByDefault
                }, transaction: transaction);

                await transaction.CommitAsync();
                return section.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Memperbarui metadata Section secara asinkron.
        /// </summary>
        public async Task<bool> UpdateSectionAsync(SectionModel section)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"UPDATE sections SET
                           name = @Name,
                           desc = @Desc,
                           icon = @Icon,
                           app_type = @AppType,
                           sort_order = @SortOrder,
                           is_game_by_default = @IsGameByDefault,
                           is_excluded_by_default = @IsExcludedByDefault
                           WHERE id = @Id;";

            int rowsAffected = await conn.ExecuteAsync(sql, new
            {
                Id = section.Id.ToString(),
                section.Name,
                section.Desc,
                section.Icon,
                section.AppType,
                section.SortOrder,
                section.IsGameByDefault,
                section.IsExcludedByDefault
            });

            return rowsAffected > 0;
        }

        /// <summary>
        /// Menyimpan urutan baru seluruh Section sekaligus setelah drag-and-drop di UI secara asinkron.
        /// </summary>
        public async Task<bool> UpdateSectionsOrderAsync(IEnumerable<SectionModel> orderedSections)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string sql = "UPDATE sections SET sort_order = @SortOrder WHERE id = @Id;";

                var parameters = orderedSections.Select((sec, index) => new
                {
                    Id = sec.Id.ToString(),
                    SortOrder = index
                });

                await conn.ExecuteAsync(sql, parameters, transaction: transaction);
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        #endregion

        #region 3. ASSIGNMENT & MOVING (PENITIPAN LOGIC)

        /// <summary>
        /// Memindahkan game dari satu Section ke Section lain secara asinkron.
        /// </summary>
        public async Task<bool> MoveAppToSectionAsync(Guid appId, Guid targetSectionId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = @"INSERT INTO app_sections (app_id, section_id) VALUES (@AppId, @SectionId)
                           ON CONFLICT(app_id, section_id) DO UPDATE SET section_id = @SectionId;";

            int rowsAffected = await conn.ExecuteAsync(sql, new
            {
                AppId = appId.ToString(),
                SectionId = targetSectionId.ToString()
            });

            return rowsAffected > 0;
        }

        /// <summary>
        /// Memasukkan game dari ruang "Penitipan" ke Section tujuan secara asinkron.
        /// </summary>
        public async Task<bool> AssignUnassignedAppToSectionAsync(Guid appId, Guid targetSectionId)
        {
            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            string sql = "INSERT OR IGNORE INTO app_sections (app_id, section_id) VALUES (@AppId, @SectionId);";
            int rowsAffected = await conn.ExecuteAsync(sql, new
            {
                AppId = appId.ToString(),
                SectionId = targetSectionId.ToString()
            });

            return rowsAffected > 0;
        }

        #endregion

        #region 4. DELETE SECTION (WITH SAFEGUARDS)

        public enum SectionDeleteMode
        {
            MoveToUnassigned,   // Titipkan semua game (Hapus relasi di app_sections)
            MoveToOtherSection, // Pindahkan semua game ke Section target
            SoftDeleteGames     // Hapus soft-delete seluruh game di section ini
        }

        /// <summary>
        /// Menghapus Section dengan proteksi keamanan secara asinkron.
        /// </summary>
        public async Task<bool> DeleteSectionAsync(Guid sectionId, SectionDeleteMode mode, Guid? targetSectionId = null)
        {
            // PROTEKSI 1: Cek apakah section tinggal 1
            if (await GetSectionCountAsync() <= 1)
            {
                throw new InvalidOperationException("Aplikasi tidak memperbolehkan menghapus Section terakhir! Minimal harus ada 1 Section.");
            }

            await using var conn = (DbConnection)DatabaseHelper.GetOpenConnection();
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                string secIdStr = sectionId.ToString();

                // A. Tentukan Nasib Game di dalam Section yang akan dihapus
                switch (mode)
                {
                    case SectionDeleteMode.MoveToUnassigned:
                        string sqlUnassign = "DELETE FROM app_sections WHERE section_id = @SectionId;";
                        await conn.ExecuteAsync(sqlUnassign, new { SectionId = secIdStr }, transaction);
                        break;

                    case SectionDeleteMode.MoveToOtherSection:
                        if (!targetSectionId.HasValue || targetSectionId.Value == Guid.Empty)
                            throw new ArgumentException("Target Section ID harus diisi jika memilih memindahkan game.");

                        string targetSecIdStr = targetSectionId.Value.ToString();

                        string sqlMoveInsert = @"INSERT OR IGNORE INTO app_sections (app_id, section_id)
                                                 SELECT app_id, @TargetSectionId FROM app_sections WHERE section_id = @SectionId;";
                        await conn.ExecuteAsync(sqlMoveInsert, new { SectionId = secIdStr, TargetSectionId = targetSecIdStr }, transaction);

                        string sqlCleanOld = "DELETE FROM app_sections WHERE section_id = @SectionId;";
                        await conn.ExecuteAsync(sqlCleanOld, new { SectionId = secIdStr }, transaction);
                        break;

                    case SectionDeleteMode.SoftDeleteGames:
                        string sqlSoftDeleteGames = @"UPDATE apps SET is_deleted = 1 
                                                     WHERE id IN (SELECT app_id FROM app_sections WHERE section_id = @SectionId);";
                        await conn.ExecuteAsync(sqlSoftDeleteGames, new { SectionId = secIdStr }, transaction);

                        string sqlCleanRel = "DELETE FROM app_sections WHERE section_id = @SectionId;";
                        await conn.ExecuteAsync(sqlCleanRel, new { SectionId = secIdStr }, transaction);
                        break;
                }

                // B. Hapus Section itu sendiri dari tabel 'sections'
                string sqlDeleteSection = "DELETE FROM sections WHERE id = @SectionId;";
                int rowsAffected = await conn.ExecuteAsync(sqlDeleteSection, new { SectionId = secIdStr }, transaction);

                await transaction.CommitAsync();
                return rowsAffected > 0;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        #endregion
    }
}