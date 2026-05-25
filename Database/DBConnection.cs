using MySql.Data.MySqlClient;

namespace IDSMS.Database
{
    public static class DBConnection
    {
        private static readonly string ConnStr =
            "Server=localhost;Database=idsms_db;Uid=root;Pwd=root;CharSet=utf8mb4;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnStr);
        }
    }
}
