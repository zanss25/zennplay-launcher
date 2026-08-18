using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using zennplay_launcher.Models;

namespace zennplay_launcher.Services.Repositories
{
    public class SectionRepository
    {
        #region 1. READ (FETCH)

        /// <summary>
        /// Mengambil semua Section/Kategori yang ada, diurutkan berdasarkan sort_order kustom dari user.
        /// </summary>
        public IEnumerable<SectionModel> GetAllSections()
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "SELECT * FROM sections ORDER BY sort_order ASC, created_at ASC;";
                return conn.Query<SectionModel>(sql);
            }
        }

        /// <summary>
        /// Mengambil detail satu Section berdasarkan ID.
        /// </summary>
        public SectionModel? GetSectionById(Guid id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "SELECT * FROM sections WHERE id = @Id LIMIT 1;";
                return conn.QueryFirstOrDefault<SectionModel>(sql, new { Id = id.ToString() });
            }
        }

        /// <summary>
        /// Mengambil jumlah total Section yang tersisa di database.
        /// Digunakan untuk memastikan tidak boleh menghapus jika tinggal 1.
        /// </summary>
        public int GetSectionCount()
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "SELECT COUNT(*) FROM sections;";
                return conn.ExecuteScalar<int>(sql);
            }
        }

        /// <summary>
        /// Mengambil semua game yang saat ini "DITITIPKAN" (Sectionless).
        /// Yaitu game yang tidak memiliki relasi di tabel 'app_sections' dan belum dihapus.
        /// </summary>
        public IEnumerable<AppModel> GetUnassignedApps()
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"SELECT a.* FROM apps a
                               LEFT JOIN app_sections aps ON a.id = aps.app_id
                               WHERE aps.section_id IS NULL AND a.is_deleted = 0
                               ORDER BY a.title ASC;";

                return conn.Query<AppModel>(sql);
            }
        }

        /// <summary>
        /// Mengambil jumlah total game yang ada di dalam Section tertentu.
        /// </summary>
        public int GetAppCountInSection(Guid sectionId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"SELECT COUNT(a.id) 
                               FROM apps a
                               INNER JOIN app_sections aps ON a.id = aps.app_id
                               WHERE aps.section_id = @SectionId AND a.is_deleted = 0;";

                return conn.ExecuteScalar<int>(sql, new { SectionId = sectionId.ToString() });
            }
        }

        #endregion

        #region 2. CREATE & UPDATE

        /// <summary>
        /// Menambahkan Section baru. Jika SortOrder belum ditentukan, otomatis diletakkan di urutan paling akhir.
        /// </summary>
        public Guid AddSection(SectionModel section)
        {
            if (section.Id == Guid.Empty)
            {
                section.Id = Guid.NewGuid();
            }

            using (var conn = DatabaseHelper.GetOpenConnection())
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // Jika sort_order bernilai default (0), otomatis taruh di urutan paling belakang
                    if (section.SortOrder == 0)
                    {
                        string getMaxOrderSql = "SELECT COALESCE(MAX(sort_order), -1) + 1 FROM sections;";
                        section.SortOrder = conn.ExecuteScalar<int>(getMaxOrderSql, transaction: transaction);
                    }

                    string sql = @"INSERT INTO sections (id, name, desc, icon, app_type, sort_order, is_game_by_default, is_excluded_by_default)
                                   VALUES (@Id, @Name, @Desc, @Icon, @AppType, @SortOrder, @IsGameByDefault, @IsExcludedByDefault);";

                    conn.Execute(sql, new
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

                    transaction.Commit();
                    return section.Id;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Memperbarui metadata Section (termasuk urutan sort_order).
        /// </summary>
        public bool UpdateSection(SectionModel section)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"UPDATE sections SET
                               name = @Name,
                               desc = @Desc,
                               icon = @Icon,
                               app_type = @AppType,
                               sort_order = @SortOrder,
                               is_game_by_default = @IsGameByDefault,
                               is_excluded_by_default = @IsExcludedByDefault
                               WHERE id = @Id;";

                return conn.Execute(sql, new
                {
                    Id = section.Id.ToString(),
                    section.Name,
                    section.Desc,
                    section.Icon,
                    section.AppType,
                    section.SortOrder,
                    section.IsGameByDefault,
                    section.IsExcludedByDefault
                }) > 0;
            }
        }

        /// <summary>
        /// Menyimpan urutan baru seluruh Section sekaligus setelah user selesai melakukan drag-and-drop di UI.
        /// </summary>
        public bool UpdateSectionsOrder(IEnumerable<SectionModel> orderedSections)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    string sql = "UPDATE sections SET sort_order = @SortOrder WHERE id = @Id;";

                    var parameters = orderedSections.Select((sec, index) => new
                    {
                        Id = sec.Id.ToString(),
                        SortOrder = index // Nomor urut otomatis 0, 1, 2, 3...
                    });

                    conn.Execute(sql, parameters, transaction: transaction);
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        #endregion

        #region 3. ASSIGNMENT & MOVING (PENITIPAN LOGIC)

        /// <summary>
        /// Memindahkan game dari satu Section ke Section lain.
        /// </summary>
        public bool MoveAppToSection(Guid appId, Guid targetSectionId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"INSERT INTO app_sections (app_id, section_id) VALUES (@AppId, @SectionId)
                               ON CONFLICT(app_id, section_id) DO UPDATE SET section_id = @SectionId;";

                return conn.Execute(sql, new
                {
                    AppId = appId.ToString(),
                    SectionId = targetSectionId.ToString()
                }) > 0;
            }
        }

        /// <summary>
        /// Memasukkan game dari ruang "Penitipan" ke Section tujuan.
        /// </summary>
        public bool AssignUnassignedAppToSection(Guid appId, Guid targetSectionId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "INSERT OR IGNORE INTO app_sections (app_id, section_id) VALUES (@AppId, @SectionId);";
                return conn.Execute(sql, new
                {
                    AppId = appId.ToString(),
                    SectionId = targetSectionId.ToString()
                }) > 0;
            }
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
        /// Menghapus Section dengan proteksi keamanan (Aturan Min 1 Section & Pilihan Nasib Game).
        /// </summary>
        /// <param name="sectionId">ID Section yang akan dihapus</param>
        /// <param name="mode">Pilihan penanganan game di dalamnya</param>
        /// <param name="targetSectionId">ID Section tujuan (wajib diisi jika mode = MoveToOtherSection)</param>
        public bool DeleteSection(Guid sectionId, SectionDeleteMode mode, Guid? targetSectionId = null)
        {
            // PROTEKSI 1: Cek apakah section tinggal 1
            if (GetSectionCount() <= 1)
            {
                throw new InvalidOperationException("Aplikasi tidak memperbolehkan menghapus Section terakhir! Minimal harus ada 1 Section.");
            }

            using (var conn = DatabaseHelper.GetOpenConnection())
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    string secIdStr = sectionId.ToString();

                    // A. Tentukan Nasib Game di dalam Section yang akan dihapus
                    switch (mode)
                    {
                        case SectionDeleteMode.MoveToUnassigned:
                            string sqlUnassign = "DELETE FROM app_sections WHERE section_id = @SectionId;";
                            conn.Execute(sqlUnassign, new { SectionId = secIdStr }, transaction);
                            break;

                        case SectionDeleteMode.MoveToOtherSection:
                            if (!targetSectionId.HasValue || targetSectionId.Value == Guid.Empty)
                                throw new ArgumentException("Target Section ID harus diisi jika memilih memindahkan game.");

                            string targetSecIdStr = targetSectionId.Value.ToString();

                            string sqlMoveInsert = @"INSERT OR IGNORE INTO app_sections (app_id, section_id)
                                                     SELECT app_id, @TargetSectionId FROM app_sections WHERE section_id = @SectionId;";
                            conn.Execute(sqlMoveInsert, new { SectionId = secIdStr, TargetSectionId = targetSecIdStr }, transaction);

                            string sqlCleanOld = "DELETE FROM app_sections WHERE section_id = @SectionId;";
                            conn.Execute(sqlCleanOld, new { SectionId = secIdStr }, transaction);
                            break;

                        case SectionDeleteMode.SoftDeleteGames:
                            string sqlSoftDeleteGames = @"UPDATE apps SET is_deleted = 1 
                                                          WHERE id IN (SELECT app_id FROM app_sections WHERE section_id = @SectionId);";
                            conn.Execute(sqlSoftDeleteGames, new { SectionId = secIdStr }, transaction);

                            string sqlCleanRel = "DELETE FROM app_sections WHERE section_id = @SectionId;";
                            conn.Execute(sqlCleanRel, new { SectionId = secIdStr }, transaction);
                            break;
                    }

                    // B. Hapus Section itu sendiri dari tabel 'sections'
                    string sqlDeleteSection = "DELETE FROM sections WHERE id = @SectionId;";
                    int rowsAffected = conn.Execute(sqlDeleteSection, new { SectionId = secIdStr }, transaction);

                    transaction.Commit();
                    return rowsAffected > 0;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #endregion
    }
}