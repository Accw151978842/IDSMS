using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace IDSMS.Database
{
    public static class DBConnection
    {
        private const string ConnStr =
            "Server=localhost;Database=idsms_db;Uid=root;Pwd=yourpassword;CharSet=utf8mb4;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnStr);
        }

        public static bool TestConnection()
        {
            try
            {
                using (MySqlConnection cn = GetConnection())
                {
                    cn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("DB connection failed:\n" + ex.Message, "DB Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
