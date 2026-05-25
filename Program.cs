using System;
using System.Windows.Forms;
using IDSMS.Database;
using IDSMS.Forms;

namespace IDSMS
{
    internal static class Program
    {
        public static string CurrentUserID   = string.Empty;
        public static string CurrentUserRole = string.Empty;
        public static string CurrentUserName = string.Empty;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!DBConnection.TestConnection())
            {
                MessageBox.Show(
                    "Cannot connect to database.\nCheck Database/DBConnection.cs settings.",
                    "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new LoginForm());
        }
    }
}
