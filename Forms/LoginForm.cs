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
        private Label lblMsg;
        private int failCount = 0;

        public LoginForm()
        {
            Text = "IDSMS Login";
            Size = new System.Drawing.Size(360, 260);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var pnl = new Panel { Dock = DockStyle.Fill, BackColor = System.Drawing.Color.FromArgb(30, 30, 60) };
            Controls.Add(pnl);

            var lblTitle = new Label
            {
                Text = "IDSMS",
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 18, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(120, 20),
                Size = new System.Drawing.Size(120, 40)
            };
            pnl.Controls.Add(lblTitle);

            pnl.Controls.Add(Lbl("User ID:", 60, 75));
            txtUser = new TextBox { Location = new System.Drawing.Point(140, 72), Size = new System.Drawing.Size(160, 23), Text = "USR0000001" };
            pnl.Controls.Add(txtUser);

            pnl.Controls.Add(Lbl("Password:", 60, 110));
            txtPwd = new TextBox { Location = new System.Drawing.Point(140, 107), Size = new System.Drawing.Size(160, 23), UseSystemPasswordChar = true, Text = "Admin@1234" };
            pnl.Controls.Add(txtPwd);

            btnLogin = new Button
            {
                Text = "Login",
                Location = new System.Drawing.Point(120, 145),
                Size = new System.Drawing.Size(110, 33),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.Click += BtnLogin_Click;
            pnl.Controls.Add(btnLogin);

            lblMsg = new Label { ForeColor = System.Drawing.Color.Red, Location = new System.Drawing.Point(60, 188), Size = new System.Drawing.Size(240, 23) };
            pnl.Controls.Add(lblMsg);

            txtPwd.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) btnLogin.PerformClick(); };
        }

        private Label Lbl(string t, int x, int y)
        {
            return new Label { Text = t, ForeColor = System.Drawing.Color.White, Location = new System.Drawing.Point(x, y), Size = new System.Drawing.Size(75, 23) };
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (failCount >= 3)
            {
                MessageBox.Show("Account locked after 3 failed attempts.", "Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string uid = txtUser.Text.Trim();
            string pwd = txtPwd.Text;
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection())
                {
                    cn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(
                        "SELECT userID,empname,role FROM users WHERE userID=@u AND password=@p LIMIT 1", cn))
                    {
                        cmd.Parameters.AddWithValue("@u", uid);
                        cmd.Parameters.AddWithValue("@p", pwd);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                Program.CurrentUserID   = r["userID"].ToString();
                                Program.CurrentUserName = r["empname"].ToString();
                                Program.CurrentUserRole = r["role"].ToString();
                                new MainMenuForm().Show();
                                Hide();
                            }
                            else
                            {
                                failCount++;
                                lblMsg.Text = "Invalid credentials. Attempt " + failCount + "/3";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login error: " + ex.Message);
            }
        }
    }
}
