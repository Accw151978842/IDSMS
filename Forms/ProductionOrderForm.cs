using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    public class ProductionOrderForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSrch, txtID, txtOrderID, txtItemID, txtQty, txtAssignedTo;
        private ComboBox cboStatus;
        private DateTimePicker dtpStart, dtpEnd;
        private string selID = "";

        public ProductionOrderForm()
        {
            Text = "Production Order Management"; Size = new System.Drawing.Size(1000, 580); StartPosition = FormStartPosition.CenterParent;
            txtSrch = new TextBox { Location = new System.Drawing.Point(70, 7), Size = new System.Drawing.Size(200, 23) };
            Controls.Add(new Label { Text = "Search:", Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(55, 23) }); Controls.Add(txtSrch);
            var btnSrch = new Button { Text = "Search", Location = new System.Drawing.Point(280, 5), Size = new System.Drawing.Size(80, 27), BackColor = System.Drawing.Color.SteelBlue, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat };
            btnSrch.Click += (s, e) => Reload(txtSrch.Text); Controls.Add(btnSrch);
            dgv = new DataGridView { Location = new System.Drawing.Point(10, 42), Size = new System.Drawing.Size(600, 460), ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = System.Drawing.Color.White };
            dgv.SelectionChanged += Sel; Controls.Add(dgv);
            int px = 625, py = 42, lw = 130, fw = 200;
            Controls.Add(new Label { Text = "Production Order", Location = new System.Drawing.Point(px, py - 22), Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold) });
            txtID = new TextBox { Size = new System.Drawing.Size(fw, 23), ReadOnly = true };
            AddRow(px, ref py, lw, fw, "Production ID:", txtID);
            txtOrderID = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Sales Order ID:", txtOrderID);
            txtItemID = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Item ID:", txtItemID);
            txtQty = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Qty:", txtQty);
            dtpStart = new DateTimePicker { Size = new System.Drawing.Size(fw, 23), Format = DateTimePickerFormat.Short }; AddRow(px, ref py, lw, fw, "Start Date:", dtpStart);
            dtpEnd = new DateTimePicker { Size = new System.Drawing.Size(fw, 23), Format = DateTimePickerFormat.Short }; AddRow(px, ref py, lw, fw, "End Date:", dtpEnd);
            cboStatus = new ComboBox { Size = new System.Drawing.Size(fw, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "Planned", "InProgress", "Completed", "OnHold", "Cancelled" }); AddRow(px, ref py, lw, fw, "Status:", cboStatus);
            txtAssignedTo = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Assigned To:", txtAssignedTo);
            var btnNew = Btn("New", px, py, System.Drawing.Color.ForestGreen);
            var btnSave = Btn("Save", px + 90, py, System.Drawing.Color.RoyalBlue);
            var btnDel = Btn("Delete", px + 180, py, System.Drawing.Color.Crimson);
            btnNew.Click += (s, e) => New_(); btnSave.Click += (s, e) => Save_(); btnDel.Click += (s, e) => Del_();
            Controls.AddRange(new System.Windows.Forms.Control[] { btnNew, btnSave, btnDel }); Reload("");
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
                    string sql = string.IsNullOrEmpty(s) ? "SELECT * FROM productionorders ORDER BY productionID DESC LIMIT 300" : "SELECT * FROM productionorders WHERE productionID LIKE @s OR orderID LIKE @s LIMIT 300";
                    using (MySqlCommand cmd = new MySqlCommand(sql, cn)) { if (!string.IsNullOrEmpty(s)) cmd.Parameters.AddWithValue("@s", "%" + s + "%"); DataTable dt = new DataTable(); new MySqlDataAdapter(cmd).Fill(dt); dgv.DataSource = dt; } }
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        private void Sel(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            DataRowView row = dgv.SelectedRows[0].DataBoundItem as DataRowView; if (row == null) return;
            selID = row["productionID"].ToString();
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM productionorders WHERE productionID=@id", cn)) { cmd.Parameters.AddWithValue("@id", selID);
                        using (MySqlDataReader r = cmd.ExecuteReader()) { if (r.Read()) { txtID.Text = r["productionID"].ToString(); txtOrderID.Text = r["orderID"].ToString(); txtItemID.Text = r["itemID"].ToString(); txtQty.Text = r["qty"].ToString(); if (r["startdate"] != DBNull.Value) dtpStart.Value = Convert.ToDateTime(r["startdate"]); if (r["enddate"] != DBNull.Value) dtpEnd.Value = Convert.ToDateTime(r["enddate"]); cboStatus.Text = r["status"].ToString(); txtAssignedTo.Text = r["assignedto"].ToString(); } } } }
            }
            catch { }
        }

        private void New_() { selID = "PRD-" + DateTime.Now.ToString("yyyyMMddHHmmss"); txtID.Text = selID; txtOrderID.Text = txtItemID.Text = txtQty.Text = txtAssignedTo.Text = ""; dtpStart.Value = DateTime.Today; dtpEnd.Value = DateTime.Today.AddDays(7); cboStatus.Text = "Planned"; }

        private void Save_()
        {
            if (string.IsNullOrEmpty(txtID.Text)) { MessageBox.Show("Click New first."); return; }
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    bool ex; using (MySqlCommand chk = new MySqlCommand("SELECT COUNT(*) FROM productionorders WHERE productionID=@id", cn)) { chk.Parameters.AddWithValue("@id", txtID.Text); ex = (long)chk.ExecuteScalar() > 0; }
                    string sql = ex ? "UPDATE productionorders SET orderID=@o,itemID=@i,qty=@q,startdate=@sd,enddate=@ed,status=@ss,assignedto=@at WHERE productionID=@id"
                                   : "INSERT INTO productionorders(productionID,orderID,itemID,qty,startdate,enddate,status,assignedto)VALUES(@id,@o,@i,@q,@sd,@ed,@ss,@at)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, cn)) { cmd.Parameters.AddWithValue("@id", txtID.Text); cmd.Parameters.AddWithValue("@o", txtOrderID.Text); cmd.Parameters.AddWithValue("@i", txtItemID.Text); int q = 0; int.TryParse(txtQty.Text, out q); cmd.Parameters.AddWithValue("@q", q); cmd.Parameters.AddWithValue("@sd", dtpStart.Value.Date); cmd.Parameters.AddWithValue("@ed", dtpEnd.Value.Date); cmd.Parameters.AddWithValue("@ss", cboStatus.Text); cmd.Parameters.AddWithValue("@at", txtAssignedTo.Text); cmd.ExecuteNonQuery(); }
                    Reload(""); MessageBox.Show("Saved!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            }
            catch (Exception ex2) { MessageBox.Show("Save error: " + ex2.Message); }
        }

        private void Del_()
        {
            if (string.IsNullOrEmpty(selID)) { MessageBox.Show("Select a record."); return; }
            if (MessageBox.Show("Delete " + selID + "?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try { using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open(); using (MySqlCommand cmd = new MySqlCommand("DELETE FROM productionorders WHERE productionID=@id", cn)) { cmd.Parameters.AddWithValue("@id", selID); cmd.ExecuteNonQuery(); } selID = ""; Reload(""); } }
            catch (Exception ex) { MessageBox.Show("Delete error: " + ex.Message); }
        }
    }
}
