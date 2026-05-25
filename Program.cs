using System;
using System.Windows.Forms;
using IDSMS.Database;
using IDSMS.Forms;

namespace IDSMS
{
    internal static class Program
    {
        // Global session variables
        public static string CurrentUserID   { get; set; } = string.Empty;
        public static string CurrentUserRole { get; set; } = string.Empty;
        public static string CurrentUserName { get; set; } = string.Empty;

        [STAThread]
        static void Main()
        {
            // .NET Framework standard startup
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
