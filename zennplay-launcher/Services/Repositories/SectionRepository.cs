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
        /// Mengambil semua Section/Kategori yang ada.
        /// </summary>
        public IEnumerable<SectionModel> GetAllSections()
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "SELECT * FROM sections ORDER BY id ASC;";
                return conn.Query<SectionModel>(sql);
            }
        }

        /// <summary>
        /// Mengambil detail satu Section berdasarkan ID.
        /// </summary>
        public SectionModel? GetSectionById(int id)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "SELECT * FROM sections WHERE id = @Id LIMIT 1;";
                return conn.QueryFirstOrDefault<SectionModel>(sql, new { Id = id });
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
        public int GetAppCountInSection(int sectionId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"SELECT COUNT(a.id) 
                               FROM apps a
                               INNER JOIN app_sections aps ON a.id = aps.app_id
                               WHERE aps.section_id = @SectionId AND a.is_deleted = 0;";

                return conn.ExecuteScalar<int>(sql, new { SectionId = sectionId });
            }
        }

        #endregion

        #region 2. CREATE & UPDATE

        /// <summary>
        /// Menambahkan Section baru.
        /// </summary>
        public int AddSection(SectionModel section)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"INSERT INTO sections (name, desc, icon, is_game_by_default, is_excluded_by_default)
                               VALUES (@Name, @Desc, @Icon, @IsGameByDefault, @IsExcludedByDefault);
                               SELECT last_insert_rowid();";

                return conn.ExecuteScalar<int>(sql, section);
            }
        }

        /// <summary>
        /// Memperbarui nama, ikon, atau deskripsi Section.
        /// </summary>
        public bool UpdateSection(SectionModel section)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = @"UPDATE sections SET
                               name = @Name,
                               desc = @Desc,
                               icon = @Icon,
                               is_game_by_default = @IsGameByDefault,
                               is_excluded_by_default = @IsExcludedByDefault
                               WHERE id = @Id;";

                return conn.Execute(sql, section) > 0;
            }
        }

        #endregion

        #region 3. ASSIGNMENT & MOVING (PENITIPAN LOGIC)

        /// <summary>
        /// Memindahkan game dari satu Section ke Section lain.
        /// </summary>
        public bool MoveAppToSection(int appId, int targetSectionId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                // Gunakan INSERT OR REPLACE agar jika game belum punya section (dititipkan), dia akan dibuatkan baris baru.
                string sql = @"INSERT INTO app_sections (app_id, section_id) VALUES (@AppId, @SectionId)
                               ON CONFLICT(app_id, section_id) DO UPDATE SET section_id = @SectionId;";

                return conn.Execute(sql, new { AppId = appId, SectionId = targetSectionId }) > 0;
            }
        }

        /// <summary>
        /// Memasukkan game dari ruang "Penitipan" ke Section tujuan.
        /// </summary>
        public bool AssignUnassignedAppToSection(int appId, int targetSectionId)
        {
            using (var conn = DatabaseHelper.GetOpenConnection())
            {
                string sql = "INSERT OR IGNORE INTO app_sections (app_id, section_id) VALUES (@AppId, @SectionId);";
                return conn.Execute(sql, new { AppId = appId, SectionId = targetSectionId }) > 0;
            }
        }

        #endregion

        #region 4. DELETE SECTION (WITH SAFEGUARDS)

        public enum SectionDeleteMode
        {
            MoveToUnassigned, // Titipkan semua game (Hapus relasi di app_sections)
            MoveToOtherSection, // Pindahkan semua game ke Section target
            SoftDeleteGames // Hapus soft-delete seluruh game di section ini
        }

        /// <summary>
        /// Menghapus Section dengan proteksi keamanan (Aturan Min 1 Section & Pilihan Nasib Game).
        /// </summary>
        /// <param name="sectionId">ID Section yang akan dihapus</param>
        /// <param name="mode">Pilihan penanganan game di dalamnya</param>
        /// <param name="targetSectionId">ID Section tujuan (wajib diisi jika mode = MoveToOtherSection)</param>
        public bool DeleteSection(int sectionId, SectionDeleteMode mode, int? targetSectionId = null)
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
                    // A. Tentukan Nasib Game di dalam Section yang akan dihapus
                    switch (mode)
                    {
                        case SectionDeleteMode.MoveToUnassigned:
                            // Hapus SEMUA keterikatan game pada section_id ini
                            string sqlUnassign = "DELETE FROM app_sections WHERE section_id = @SectionId;";
                            conn.Execute(sqlUnassign, new { SectionId = sectionId }, transaction);
                            break;

                        case SectionDeleteMode.MoveToOtherSection:
                            if (!targetSectionId.HasValue)
                                throw new ArgumentException("Target Section ID harus diisi jika memilih memindahkan game.");

                            // Pindahkan game ke section tujuan (Gunakan INSERT OR IGNORE jika game sudah ada di section tujuan, lalu hapus relasi lama)
                            string sqlMoveInsert = @"INSERT OR IGNORE INTO app_sections (app_id, section_id)
                                                     SELECT app_id, @TargetSectionId FROM app_sections WHERE section_id = @SectionId;";
                            conn.Execute(sqlMoveInsert, new { SectionId = sectionId, TargetSectionId = targetSectionId.Value }, transaction);

                            // Bersihkan relasi dari section lama yang dihapus
                            string sqlCleanOld = "DELETE FROM app_sections WHERE section_id = @SectionId;";
                            conn.Execute(sqlCleanOld, new { SectionId = sectionId }, transaction);
                            break;

                        case SectionDeleteMode.SoftDeleteGames:
                            // Soft delete seluruh game yang HANYA ada di section ini
                            string sqlSoftDeleteGames = @"UPDATE apps SET is_deleted = 1 
                                                          WHERE id IN (SELECT app_id FROM app_sections WHERE section_id = @SectionId);";
                            conn.Execute(sqlSoftDeleteGames, new { SectionId = sectionId }, transaction);

                            // Hapus relasinya
                            string sqlCleanRel = "DELETE FROM app_sections WHERE section_id = @SectionId;";
                            conn.Execute(sqlCleanRel, new { SectionId = sectionId }, transaction);
                            break;
                    }

                    // B. Hapus Section itu sendiri dari tabel 'sections'
                    string sqlDeleteSection = "DELETE FROM sections WHERE id = @SectionId;";
                    int rowsAffected = conn.Execute(sqlDeleteSection, new { SectionId = sectionId }, transaction);

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