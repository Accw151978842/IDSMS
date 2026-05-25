using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    public class UserManagementForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSrch, txtID, txtName, txtEmail, txtPwd;
        private ComboBox cboRole;
        private string selID = "";

        public UserManagementForm()
        {
            if (Program.CurrentUserRole != "Admin")
            {
                MessageBox.Show("Admin only.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Load += (s, e) => Close(); return;
            }
            Text = "User Management (Admin Only)"; Size = new System.Drawing.Size(960, 560); StartPosition = FormStartPosition.CenterParent;
            txtSrch = new TextBox { Location = new System.Drawing.Point(70, 7), Size = new System.Drawing.Size(200, 23) };
            Controls.Add(new Label { Text = "Search:", Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(55, 23) }); Controls.Add(txtSrch);
            var btnSrch = new Button { Text = "Search", Location = new System.Drawing.Point(280, 5), Size = new System.Drawing.Size(80, 27), BackColor = System.Drawing.Color.SteelBlue, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat };
            btnSrch.Click += (s, e) => Reload(txtSrch.Text); Controls.Add(btnSrch);
            dgv = new DataGridView { Location = new System.Drawing.Point(10, 42), Size = new System.Drawing.Size(580, 430), ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = System.Drawing.Color.White };
            dgv.SelectionChanged += Sel; Controls.Add(dgv);
            int px = 610, py = 42, lw = 100, fw = 200;
            Controls.Add(new Label { Text = "User Details", Location = new System.Drawing.Point(px, py - 22), Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold) });
            txtID = new TextBox { Size = new System.Drawing.Size(fw, 23), ReadOnly = true }; AddRow(px, ref py, lw, fw, "User ID:", txtID);
            txtName = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Name *:", txtName);
            txtEmail = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Email:", txtEmail);
            cboRole = new ComboBox { Size = new System.Drawing.Size(fw, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cboRole.Items.AddRange(new object[] { "Admin", "Sales", "Production", "Inventory", "Logistics", "Finance" }); AddRow(px, ref py, lw, fw, "Role *:", cboRole);
            txtPwd = new TextBox { Size = new System.Drawing.Size(fw, 23), UseSystemPasswordChar = true }; AddRow(px, ref py, lw, fw, "Password:", txtPwd);
            Controls.Add(new Label { Text = "(blank = keep current)", Location = new System.Drawing.Point(px, py), Size = new System.Drawing.Size(220, 18), ForeColor = System.Drawing.Color.Gray, Font = new System.Drawing.Font("Segoe UI", 8) }); py += 28;
            var btnNew = Btn("New", px, py, System.Drawing.Color.ForestGreen);
            var btnSave = Btn("Save", px + 90, py, System.Drawing.Color.RoyalBlue);
            var btnDel = Btn("Delete", px + 180, py, System.Drawing.Color.Crimson);
            btnNew.Click += (s, e) => New_(); btnSave.Click += (s, e) => Save_(); btnDel.Click += (s, e) => Del_();
            Controls.AddRange(new System.Windows.Forms.Control[] { btnNew, btnSave, btnDel });
            Reload("");
        }

        private void AddRow(int px, ref int py, int lw, int fw, string lbl, System.Windows.Forms.Control ctrl)
        { Controls.Add(new Label { Text = lbl, Location = new System.Drawing.Point(px, py), Size = new System.Drawing.Size(lw, 23) }); ctrl.Location = new System.Drawing.Point(px + lw + 5, py); Controls.Add(ctrl); py += 35; }

        private Button Btn(string t, int x, int y, System.Drawing.Color c)
        { return new Button { Text = t, Location = new System.Drawing.Point(x, y), Size = new System.Drawing.Size(80, 30), BackColor = c, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat }; }

        private void Reload(string s)
        {
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    string sql = string.IsNullOrEmpty(s) ? "SELECT userID,empname,role,email FROM users ORDER BY role,empname" : "SELECT userID,empname,role,email FROM users WHERE empname LIKE @s OR userID LIKE @s OR role LIKE @s";
                    using (MySqlCommand cmd = new MySqlCommand(sql, cn)) { if (!string.IsNullOrEmpty(s)) cmd.Parameters.AddWithValue("@s", "%" + s + "%"); DataTable dt = new DataTable(); new MySqlDataAdapter(cmd).Fill(dt); dgv.DataSource = dt; } }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void Sel(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            DataRowView row = dgv.SelectedRows[0].DataBoundItem as DataRowView; if (row == null) return;
            selID = row["userID"].ToString(); txtID.Text = selID; txtName.Text = row["empname"].ToString(); cboRole.Text = row["role"].ToString(); txtEmail.Text = row["email"].ToString(); txtPwd.Text = "";
        }

        private void New_() { selID = "USR" + DateTime.Now.ToString("yyMMddHHmmss"); txtID.Text = selID; txtName.Text = txtEmail.Text = txtPwd.Text = ""; cboRole.SelectedIndex = -1; }

        private void Save_()
        {
            if (string.IsNullOrEmpty(txtID.Text)) { MessageBox.Show("Click New first."); return; }
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(cboRole.Text)) { MessageBox.Show("Name and Role required."); return; }
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    bool ex; using (MySqlCommand chk = new MySqlCommand("SELECT COUNT(*) FROM users WHERE userID=@id", cn)) { chk.Parameters.AddWithValue("@id", txtID.Text); ex = (long)chk.ExecuteScalar() > 0; }
                    bool chgPwd = !string.IsNullOrEmpty(txtPwd.Text);
                    string sql;
                    if (ex) sql = chgPwd ? "UPDATE users SET empname=@n,role=@r,email=@e,password=@p WHERE userID=@id" : "UPDATE users SET empname=@n,role=@r,email=@e WHERE userID=@id";
                    else { if (!chgPwd) { MessageBox.Show("Password required for new user."); return; } sql = "INSERT INTO users(userID,empname,role,email,password)VALUES(@id,@n,@r,@e,@p)"; }
                    using (MySqlCommand cmd = new MySqlCommand(sql, cn)) { cmd.Parameters.AddWithValue("@id", txtID.Text); cmd.Parameters.AddWithValue("@n", txtName.Text); cmd.Parameters.AddWithValue("@r", cboRole.Text); cmd.Parameters.AddWithValue("@e", txtEmail.Text); if (chgPwd) cmd.Parameters.AddWithValue("@p", txtPwd.Text); cmd.ExecuteNonQuery(); }
                    Reload(""); MessageBox.Show("Saved!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            }
            catch (Exception ex2) { MessageBox.Show("Error: " + ex2.Message); }
        }

        private void Del_()
        {
            if (string.IsNullOrEmpty(selID)) { MessageBox.Show("Select a user."); return; }
            if (selID == Program.CurrentUserID) { MessageBox.Show("Cannot delete your own account."); return; }
            if (MessageBox.Show("Delete " + selID + "?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try { using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open(); using (MySqlCommand cmd = new MySqlCommand("DELETE FROM users WHERE userID=@id", cn)) { cmd.Parameters.AddWithValue("@id", selID); cmd.ExecuteNonQuery(); } selID = ""; Reload(""); } }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
    }
}
