using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtUser, txtPwd;
        private Button btnLogin;
        private Label lblStatus;

        public LoginForm()
        {
            Text = "IDSMS Login"; Size = new System.Drawing.Size(380, 260);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var lblTitle = new Label { Text = "Integrated Demand & Supply Mgt System", Location = new System.Drawing.Point(15, 18), Size = new System.Drawing.Size(330, 22), Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold), TextAlign = System.Drawing.ContentAlignment.MiddleCenter };
            var lblU = new Label { Text = "User ID:", Location = new System.Drawing.Point(40, 58), Size = new System.Drawing.Size(70, 23) };
            txtUser = new TextBox { Location = new System.Drawing.Point(118, 56), Size = new System.Drawing.Size(200, 23) };
            var lblP = new Label { Text = "Password:", Location = new System.Drawing.Point(40, 90), Size = new System.Drawing.Size(70, 23) };
            txtPwd = new TextBox { Location = new System.Drawing.Point(118, 88), Size = new System.Drawing.Size(200, 23), UseSystemPasswordChar = true };
            btnLogin = new Button { Text = "Login", Location = new System.Drawing.Point(118, 122), Size = new System.Drawing.Size(200, 33), BackColor = System.Drawing.Color.RoyalBlue, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat };
            btnLogin.Click += DoLogin;
            txtPwd.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) DoLogin(null, null); };
            lblStatus = new Label { Location = new System.Drawing.Point(15, 165), Size = new System.Drawing.Size(330, 22), ForeColor = System.Drawing.Color.Red, TextAlign = System.Drawing.ContentAlignment.MiddleCenter };
            Controls.AddRange(new System.Windows.Forms.Control[] { lblTitle, lblU, txtUser, lblP, txtPwd, btnLogin, lblStatus });
        }

        private void DoLogin(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPwd.Text))
            { lblStatus.Text = "Enter User ID and Password."; return; }
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT userID,empname,role,failcount,locked FROM users WHERE userID=@u AND password=@p", cn))
                    {
                        cmd.Parameters.AddWithValue("@u", txtUser.Text.Trim());
                        cmd.Parameters.AddWithValue("@p", txtPwd.Text);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                if (Convert.ToInt32(r["locked"]) == 1) { lblStatus.Text = "Account locked. Contact Admin."; return; }
                                Program.CurrentUserID = r["userID"].ToString();
                                Program.CurrentUserName = r["empname"].ToString();
                                Program.CurrentUserRole = r["role"].ToString();
                            }
                            else { lblStatus.Text = "Invalid credentials. " + IncrFail(txtUser.Text.Trim()); return; }
                        }
                    }
                    using (MySqlCommand reset = new MySqlCommand("UPDATE users SET failcount=0 WHERE userID=@u", cn))
                    { reset.Parameters.AddWithValue("@u", Program.CurrentUserID); reset.ExecuteNonQuery(); }
                }
                Hide(); new MainMenuForm().ShowDialog(); Application.Exit();
            }
            catch (Exception ex) { lblStatus.Text = "DB Error: " + ex.Message; }
        }

        private string IncrFail(string uid)
        {
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("UPDATE users SET failcount=failcount+1,locked=IF(failcount>=2,1,0) WHERE userID=@u", cn))
                    { cmd.Parameters.AddWithValue("@u", uid); cmd.ExecuteNonQuery(); } }
            }
            catch { }
            return "(account may be locked after 3 failures)";
        }
    }
}
