using System;
using MySql.Data.MySqlClient;

namespace IDSMS.Database
{
    public static class DBConnection
    {
        // TODO: Change Pwd=yourpassword to your actual MySQL password
        private static readonly string connectionString =
            "Server=localhost;Database=idsms_db;Uid=root;Pwd=yourpassword;CharSet=utf8mb4;";

        public static MySqlConnection GetConnection() => new MySqlConnection(connectionString);

        public static bool TestConnection()
        {
            try { using var c = GetConnection(); c.Open(); return true; }
            catch { return false; }
        }
    }
}
