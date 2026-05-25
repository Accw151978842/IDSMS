using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtUser, txtPass;
        private Label lblErr;
        private int fails = 0;

        public LoginForm()
        {
            Text = "IDSMS Login"; Size = new System.Drawing.Size(420, 300);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;

            var title = new Label { Text = "IDSMS - Premium Living Furniture",
                Font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(20, 18), Size = new System.Drawing.Size(370, 30),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter };
            var lblU = new Label { Text = "Username:", Location = new System.Drawing.Point(60, 70), Size = new System.Drawing.Size(85, 23) };
            txtUser = new TextBox { Location = new System.Drawing.Point(155, 67), Size = new System.Drawing.Size(180, 23) };
            var lblP = new Label { Text = "Password:", Location = new System.Drawing.Point(60, 105), Size = new System.Drawing.Size(85, 23) };
            txtPass = new TextBox { Location = new System.Drawing.Point(155, 102), Size = new System.Drawing.Size(180, 23), UseSystemPasswordChar = true };
            lblErr = new Label { Text = "", ForeColor = System.Drawing.Color.Red,
                Location = new System.Drawing.Point(60, 132), Size = new System.Drawing.Size(300, 23) };
            var btnLogin = new Button { Text = "Login", Location = new System.Drawing.Point(155, 160),
                Size = new System.Drawing.Size(100, 33), FlatStyle = FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215), ForeColor = System.Drawing.Color.White };
            btnLogin.Click += Login_Click; AcceptButton = btnLogin;
            Controls.AddRange(new Control[] { title, lblU, txtUser, lblP, txtPass, lblErr, btnLogin });
        }

        private void Login_Click(object? s, EventArgs e)
        {
            if (fails >= 3) { lblErr.Text = "Account locked. Contact admin."; return; }
            string uid = txtUser.Text.Trim(), pwd = txtPass.Text;
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(pwd)) { lblErr.Text = "Enter username and password."; return; }
            try
            {
                using var conn = DBConnection.GetConnection(); conn.Open();
                using var cmd = new MySqlCommand("SELECT userID,empname,role,password FROM users WHERE userID=@u", conn);
                cmd.Parameters.AddWithValue("@u", uid);
                using var r = cmd.ExecuteReader();
                if (r.Read() && pwd == r.GetString("password"))
                {
                    Program.CurrentUserID   = r.GetString("userID");
                    Program.CurrentUserName = r.GetString("empname");
                    Program.CurrentUserRole = r.GetString("role");
                    Hide(); new MainMenuForm().ShowDialog(); Close(); return;
                }
                fails++; lblErr.Text = $"Invalid credentials. Attempt {fails}/3.";
            }
            catch (Exception ex) { MessageBox.Show("DB Error: " + ex.Message); }
        }
    }
}
