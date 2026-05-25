using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    public class ComplaintForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSrch, txtID, txtCustID, txtOrderID, txtSerialNo, txtDesc, txtHandledBy, txtResolution;
        private ComboBox cboCategory, cboPriority, cboStatus;
        private string selID = "";

        public ComplaintForm() : this("") { }
        public ComplaintForm(string filterID)
        {
            Text = "Complaint Management"; Size = new System.Drawing.Size(1000, 660); StartPosition = FormStartPosition.CenterParent;
            txtSrch = new TextBox { Location = new System.Drawing.Point(70, 7), Size = new System.Drawing.Size(200, 23) };
            Controls.Add(new Label { Text = "Search:", Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(55, 23) }); Controls.Add(txtSrch);
            var btnSrch = new Button { Text = "Search", Location = new System.Drawing.Point(280, 5), Size = new System.Drawing.Size(80, 27), BackColor = System.Drawing.Color.SteelBlue, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat };
            btnSrch.Click += (s, e) => Reload(txtSrch.Text); Controls.Add(btnSrch);
            dgv = new DataGridView { Location = new System.Drawing.Point(10, 42), Size = new System.Drawing.Size(600, 510), ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = System.Drawing.Color.White };
            dgv.SelectionChanged += Sel; Controls.Add(dgv);
            int px = 625, py = 42, lw = 140, fw = 200;
            Controls.Add(new Label { Text = "Complaint", Location = new System.Drawing.Point(px, py - 22), Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold) });
            txtID = new TextBox { Size = new System.Drawing.Size(fw, 23), ReadOnly = true };
            AddRow(px, ref py, lw, fw, "Complaint ID:", txtID);
            txtCustID = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Customer ID:", txtCustID);
            txtOrderID = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Order ID:", txtOrderID);
            txtSerialNo = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Serial No:", txtSerialNo);
            cboCategory = new ComboBox { Size = new System.Drawing.Size(fw, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cboCategory.Items.AddRange(new object[] { "Damage", "WrongItem", "LateDelivery", "QualityIssue", "Other" }); AddRow(px, ref py, lw, fw, "Category:", cboCategory);
            txtDesc = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Description:", txtDesc);
            cboPriority = new ComboBox { Size = new System.Drawing.Size(fw, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cboPriority.Items.AddRange(new object[] { "Low", "Medium", "High", "Critical" }); AddRow(px, ref py, lw, fw, "Priority:", cboPriority);
            cboStatus = new ComboBox { Size = new System.Drawing.Size(fw, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "Open", "InProgress", "Resolved", "Closed" }); AddRow(px, ref py, lw, fw, "Status:", cboStatus);
            txtHandledBy = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Handled By:", txtHandledBy);
            txtResolution = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Resolution:", txtResolution);
            var btnNew = Btn("New", px, py, System.Drawing.Color.ForestGreen);
            var btnSave = Btn("Save", px + 90, py, System.Drawing.Color.RoyalBlue);
            var btnDel = Btn("Delete", px + 180, py, System.Drawing.Color.Crimson);
            btnNew.Click += (s, e) => New_(); btnSave.Click += (s, e) => Save_(); btnDel.Click += (s, e) => Del_();
            Controls.AddRange(new System.Windows.Forms.Control[] { btnNew, btnSave, btnDel }); Reload(filterID);
        }

        private void AddRow(int px, ref int py, int lw, int fw, string lbl, System.Windows.Forms.Control ctrl)
        { Controls.Add(new Label { Text = lbl, Location = new System.Drawing.Point(px, py), Size = new System.Drawing.Size(lw, 23) }); ctrl.Location = new System.Drawing.Point(px + lw + 5, py); Controls.Add(ctrl); py += 31; }

        private Button Btn(string t, int x, int y, System.Drawing.Color c)
        { return new Button { Text = t, Location = new System.Drawing.Point(x, y), Size = new System.Drawing.Size(80, 30), BackColor = c, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat }; }

        private void Reload(string s)
        {
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    string sql = string.IsNullOrEmpty(s) ? "SELECT * FROM complaints ORDER BY complaintID DESC LIMIT 300" : "SELECT * FROM complaints WHERE complaintID LIKE @s OR customerID LIKE @s LIMIT 300";
                    using (MySqlCommand cmd = new MySqlCommand(sql, cn)) { if (!string.IsNullOrEmpty(s)) cmd.Parameters.AddWithValue("@s", "%" + s + "%"); DataTable dt = new DataTable(); new MySqlDataAdapter(cmd).Fill(dt); dgv.DataSource = dt; } }
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        private void Sel(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            DataRowView row = dgv.SelectedRows[0].DataBoundItem as DataRowView; if (row == null) return;
            selID = row["complaintID"].ToString();
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM complaints WHERE complaintID=@id", cn)) { cmd.Parameters.AddWithValue("@id", selID);
                        using (MySqlDataReader r = cmd.ExecuteReader()) { if (r.Read()) { txtID.Text = r["complaintID"].ToString(); txtCustID.Text = r["customerID"].ToString(); txtOrderID.Text = r["orderID"].ToString(); txtSerialNo.Text = r["serialNo"].ToString(); cboCategory.Text = r["category"].ToString(); txtDesc.Text = r["description"].ToString(); cboPriority.Text = r["priority"].ToString(); cboStatus.Text = r["status"].ToString(); txtHandledBy.Text = r["handledby"].ToString(); txtResolution.Text = r["resolution"].ToString(); } } } }
            }
            catch { }
        }

        private void New_() { selID = "CMP-" + DateTime.Now.ToString("yyyyMMddHHmmss"); txtID.Text = selID; txtCustID.Text = txtOrderID.Text = txtSerialNo.Text = txtDesc.Text = txtHandledBy.Text = txtResolution.Text = ""; cboCategory.Text = "Other"; cboPriority.Text = "Medium"; cboStatus.Text = "Open"; }

        private void Save_()
        {
            if (string.IsNullOrEmpty(txtID.Text)) { MessageBox.Show("Click New first."); return; }
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    bool ex; using (MySqlCommand chk = new MySqlCommand("SELECT COUNT(*) FROM complaints WHERE complaintID=@id", cn)) { chk.Parameters.AddWithValue("@id", txtID.Text); ex = (long)chk.ExecuteScalar() > 0; }
                    string sql = ex ? "UPDATE complaints SET customerID=@c,orderID=@o,serialNo=@sn,category=@ca,description=@de,priority=@pr,status=@ss,handledby=@hb,resolution=@re WHERE complaintID=@id"
                                   : "INSERT INTO complaints(complaintID,customerID,orderID,serialNo,category,description,priority,status,handledby,resolution)VALUES(@id,@c,@o,@sn,@ca,@de,@pr,@ss,@hb,@re)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, cn)) { cmd.Parameters.AddWithValue("@id", txtID.Text); cmd.Parameters.AddWithValue("@c", txtCustID.Text); cmd.Parameters.AddWithValue("@o", txtOrderID.Text); cmd.Parameters.AddWithValue("@sn", txtSerialNo.Text); cmd.Parameters.AddWithValue("@ca", cboCategory.Text); cmd.Parameters.AddWithValue("@de", txtDesc.Text); cmd.Parameters.AddWithValue("@pr", cboPriority.Text); cmd.Parameters.AddWithValue("@ss", cboStatus.Text); cmd.Parameters.AddWithValue("@hb", txtHandledBy.Text); cmd.Parameters.AddWithValue("@re", txtResolution.Text); cmd.ExecuteNonQuery(); }
                    Reload(""); MessageBox.Show("Saved!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            }
            catch (Exception ex2) { MessageBox.Show("Save error: " + ex2.Message); }
        }

        private void Del_()
        {
            if (string.IsNullOrEmpty(selID)) { MessageBox.Show("Select a record."); return; }
            if (MessageBox.Show("Delete " + selID + "?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try { using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open(); using (MySqlCommand cmd = new MySqlCommand("DELETE FROM complaints WHERE complaintID=@id", cn)) { cmd.Parameters.AddWithValue("@id", selID); cmd.ExecuteNonQuery(); } selID = ""; Reload(""); } }
            catch (Exception ex) { MessageBox.Show("Delete error: " + ex.Message); }
        }
    }
}
