using System;
using System.Windows.Forms;

namespace IDSMS
{
    static class Program
    {
        public static string CurrentUserID = "";
        public static string CurrentUserName = "";
        public static string CurrentUserRole = "";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Forms.LoginForm());
        }
    }
}
