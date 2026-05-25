using System;
using System.Windows.Forms;
using IDSMS.Database;
using IDSMS.Forms;

namespace IDSMS
{
    internal static class Program
    {
        public static string CurrentUserID   { get; set; } = "";
        public static string CurrentUserRole { get; set; } = "";
        public static string CurrentUserName { get; set; } = "";

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            if (!DBConnection.TestConnection())
            {
                MessageBox.Show("Cannot connect to database.\nCheck Database/DBConnection.cs settings.",
                    "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Application.Run(new LoginForm());
        }
    }
}
