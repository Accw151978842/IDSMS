using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    public class SupplierForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSrch, txtID, txtName, txtContact, txtEmail, txtPayTerms, txtOnTime, txtDefect;
        private string selID = "";

        public SupplierForm() : this("") { }
        public SupplierForm(string filterID)
        {
            Text = "Supplier Management"; Size = new System.Drawing.Size(1000, 560); StartPosition = FormStartPosition.CenterParent;
            txtSrch = new TextBox { Location = new System.Drawing.Point(70, 7), Size = new System.Drawing.Size(200, 23) };
            Controls.Add(new Label { Text = "Search:", Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(55, 23) }); Controls.Add(txtSrch);
            var btnSrch = new Button { Text = "Search", Location = new System.Drawing.Point(280, 5), Size = new System.Drawing.Size(80, 27), BackColor = System.Drawing.Color.SteelBlue, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat };
            btnSrch.Click += (s, e) => Reload(txtSrch.Text); Controls.Add(btnSrch);
            dgv = new DataGridView { Location = new System.Drawing.Point(10, 42), Size = new System.Drawing.Size(600, 440), ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = System.Drawing.Color.White };
            dgv.SelectionChanged += Sel; Controls.Add(dgv);
            int px = 625, py = 42, lw = 140, fw = 200;
            Controls.Add(new Label { Text = "Supplier Details", Location = new System.Drawing.Point(px, py - 22), Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold) });
            txtID = new TextBox { Size = new System.Drawing.Size(fw, 23), ReadOnly = true };
            AddRow(px, ref py, lw, fw, "Supplier ID:", txtID);
            txtName = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Supplier Name *:", txtName);
            txtContact = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Contact:", txtContact);
            txtEmail = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Email:", txtEmail);
            txtPayTerms = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Payment Terms:", txtPayTerms);
            txtOnTime = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "On-Time Rate %:", txtOnTime);
            txtDefect = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Defect Rate %:", txtDefect);
            var btnNew = Btn("New", px, py, System.Drawing.Color.ForestGreen);
            var btnSave = Btn("Save", px + 90, py, System.Drawing.Color.RoyalBlue);
            var btnDel = Btn("Delete", px + 180, py, System.Drawing.Color.Crimson);
            btnNew.Click += (s, e) => New_(); btnSave.Click += (s, e) => Save_(); btnDel.Click += (s, e) => Del_();
            Controls.AddRange(new System.Windows.Forms.Control[] { btnNew, btnSave, btnDel }); Reload(filterID);
        }

        private void AddRow(int px, ref int py, int lw, int fw, string lbl, System.Windows.Forms.Control ctrl)
        { Controls.Add(new Label { Text = lbl, Location = new System.Drawing.Point(px, py), Size = new System.Drawing.Size(lw, 23) }); ctrl.Location = new System.Drawing.Point(px + lw + 5, py); Controls.Add(ctrl); py += 33; }

        private Button Btn(string t, int x, int y, System.Drawing.Color c)
        { return new Button { Text = t, Location = new System.Drawing.Point(x, y), Size = new System.Drawing.Size(80, 30), BackColor = c, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat }; }

        private void Reload(string s)
        {
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    string sql = string.IsNullOrEmpty(s) ? "SELECT supplierID,suppname,contact,email,payterms,ontimedlvrate,defectrate FROM suppliers ORDER BY suppname" : "SELECT supplierID,suppname,contact,email,payterms,ontimedlvrate,defectrate FROM suppliers WHERE suppname LIKE @s OR supplierID LIKE @s";
                    using (MySqlCommand cmd = new MySqlCommand(sql, cn)) { if (!string.IsNullOrEmpty(s)) cmd.Parameters.AddWithValue("@s", "%" + s + "%"); DataTable dt = new DataTable(); new MySqlDataAdapter(cmd).Fill(dt); dgv.DataSource = dt; } }
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        private void Sel(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            DataRowView row = dgv.SelectedRows[0].DataBoundItem as DataRowView; if (row == null) return;
            selID = row["supplierID"].ToString();
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM suppliers WHERE supplierID=@id", cn)) { cmd.Parameters.AddWithValue("@id", selID);
                        using (MySqlDataReader r = cmd.ExecuteReader()) { if (r.Read()) { txtID.Text = r["supplierID"].ToString(); txtName.Text = r["suppname"].ToString(); txtContact.Text = r["contact"].ToString(); txtEmail.Text = r["email"].ToString(); txtPayTerms.Text = r["payterms"].ToString(); txtOnTime.Text = r["ontimedlvrate"].ToString(); txtDefect.Text = r["defectrate"].ToString(); } } } }
            }
            catch { }
        }

        private void New_() { selID = "SUP-" + DateTime.Now.ToString("yyyyMMddHHmmss"); txtID.Text = selID; txtName.Text = txtContact.Text = txtEmail.Text = txtOnTime.Text = txtDefect.Text = ""; txtPayTerms.Text = "Net30"; }

        private void Save_()
        {
            if (string.IsNullOrEmpty(txtID.Text)) { MessageBox.Show("Click New first."); return; }
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Supplier name required."); return; }
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    bool ex; using (MySqlCommand chk = new MySqlCommand("SELECT COUNT(*) FROM suppliers WHERE supplierID=@id", cn)) { chk.Parameters.AddWithValue("@id", txtID.Text); ex = (long)chk.ExecuteScalar() > 0; }
                    string sql = ex ? "UPDATE suppliers SET suppname=@n,contact=@c,email=@e,payterms=@pt,ontimedlvrate=@ot,defectrate=@dr WHERE supplierID=@id"
                                   : "INSERT INTO suppliers(supplierID,suppname,contact,email,payterms,ontimedlvrate,defectrate)VALUES(@id,@n,@c,@e,@pt,@ot,@dr)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, cn)) { cmd.Parameters.AddWithValue("@id", txtID.Text); cmd.Parameters.AddWithValue("@n", txtName.Text); cmd.Parameters.AddWithValue("@c", txtContact.Text); cmd.Parameters.AddWithValue("@e", txtEmail.Text); cmd.Parameters.AddWithValue("@pt", txtPayTerms.Text); decimal ot = 0, dr = 0; decimal.TryParse(txtOnTime.Text, out ot); decimal.TryParse(txtDefect.Text, out dr); cmd.Parameters.AddWithValue("@ot", ot); cmd.Parameters.AddWithValue("@dr", dr); cmd.ExecuteNonQuery(); }
                    Reload(""); MessageBox.Show("Saved!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            }
            catch (Exception ex2) { MessageBox.Show("Save error: " + ex2.Message); }
        }

        private void Del_()
        {
            if (string.IsNullOrEmpty(selID)) { MessageBox.Show("Select a record."); return; }
            if (MessageBox.Show("Delete " + selID + "?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try { using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open(); using (MySqlCommand cmd = new MySqlCommand("DELETE FROM suppliers WHERE supplierID=@id", cn)) { cmd.Parameters.AddWithValue("@id", selID); cmd.ExecuteNonQuery(); } selID = ""; Reload(""); } }
            catch (Exception ex) { MessageBox.Show("Delete error: " + ex.Message); }
        }
    }
}
