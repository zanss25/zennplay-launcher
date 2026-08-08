using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zennplay_launcher.Services
{
    public class DatabaseServices
    {
        private void ExecuteNonQuery(SqliteConnection conn, string sql)
        {
            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public bool IsTableExists(string tableName)
        {
            using (var connection = DatabaseHelper.GetOpenConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name=$name";
                command.Parameters.AddWithValue("$name", tableName);

                var result = Convert.ToInt32(command.ExecuteScalar());

                return result > 0;
            }
        }

        // Fungsi untuk query SQLite (Tanpa Return)
        public void ExecuteQuery(string conn, string query, Dictionary<string, object> parameters = null)
        {
            using (var connection = new SqliteConnection(conn))
            {
                connection.Open();
                using (var command = new SqliteCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }
                    command.ExecuteNonQuery();
                }
            }
        }

        // Fungsi untuk query SQLite dengan Type dan Return
        public T ExecuteScalar<T>(string conn, string query, Dictionary<string, object> parameters = null)
        {
            using (var connection = new SqliteConnection(conn))
            {
                try
                {
                    connection.Open();
                    var command = new SqliteCommand(query, connection);

                    // Tambahkan parameter jika ada
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        // Mengubah object hasil database ke tipe T yang diminta
                        return (T)Convert.ChangeType(result, typeof(T));
                    }

                    return default(T);
                }
                catch (SqliteException ex) when (ex.SqliteErrorCode == 1)
                {
                    return default(T);
                }
            }
        }

    }
}
