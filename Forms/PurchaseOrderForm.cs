using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    public class PurchaseOrderForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSrch, txtID, txtSupplierID, txtTotalCost;
        private ComboBox cboStatus;
        private DateTimePicker dtpOrder, dtpExpected;
        private string selID = "";

        public PurchaseOrderForm() : this("") { }
        public PurchaseOrderForm(string filterID)
        {
            Text = "Purchase Order Management"; Size = new System.Drawing.Size(1000, 540); StartPosition = FormStartPosition.CenterParent;
            txtSrch = new TextBox { Location = new System.Drawing.Point(70, 7), Size = new System.Drawing.Size(200, 23) };
            Controls.Add(new Label { Text = "Search:", Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(55, 23) }); Controls.Add(txtSrch);
            var btnSrch = new Button { Text = "Search", Location = new System.Drawing.Point(280, 5), Size = new System.Drawing.Size(80, 27), BackColor = System.Drawing.Color.SteelBlue, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat };
            btnSrch.Click += (s, e) => Reload(txtSrch.Text); Controls.Add(btnSrch);
            dgv = new DataGridView { Location = new System.Drawing.Point(10, 42), Size = new System.Drawing.Size(600, 420), ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = System.Drawing.Color.White };
            dgv.SelectionChanged += Sel; Controls.Add(dgv);
            int px = 625, py = 42, lw = 140, fw = 200;
            Controls.Add(new Label { Text = "Purchase Order", Location = new System.Drawing.Point(px, py - 22), Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold) });
            txtID = new TextBox { Size = new System.Drawing.Size(fw, 23), ReadOnly = true };
            AddRow(px, ref py, lw, fw, "PO ID:", txtID);
            txtSupplierID = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Supplier ID:", txtSupplierID);
            dtpOrder = new DateTimePicker { Size = new System.Drawing.Size(fw, 23), Format = DateTimePickerFormat.Short }; AddRow(px, ref py, lw, fw, "Order Date:", dtpOrder);
            dtpExpected = new DateTimePicker { Size = new System.Drawing.Size(fw, 23), Format = DateTimePickerFormat.Short }; AddRow(px, ref py, lw, fw, "Expected Date:", dtpExpected);
            cboStatus = new ComboBox { Size = new System.Drawing.Size(fw, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "Draft", "Sent", "Confirmed", "Received", "Cancelled" }); AddRow(px, ref py, lw, fw, "Status:", cboStatus);
            txtTotalCost = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Total Cost:", txtTotalCost);
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
                    string sql = string.IsNullOrEmpty(s) ? "SELECT * FROM purchaseorders ORDER BY poid DESC LIMIT 300" : "SELECT * FROM purchaseorders WHERE poid LIKE @s OR supplierID LIKE @s LIMIT 300";
                    using (MySqlCommand cmd = new MySqlCommand(sql, cn)) { if (!string.IsNullOrEmpty(s)) cmd.Parameters.AddWithValue("@s", "%" + s + "%"); DataTable dt = new DataTable(); new MySqlDataAdapter(cmd).Fill(dt); dgv.DataSource = dt; } }
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        private void Sel(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            DataRowView row = dgv.SelectedRows[0].DataBoundItem as DataRowView; if (row == null) return;
            selID = row["poid"].ToString();
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM purchaseorders WHERE poid=@id", cn)) { cmd.Parameters.AddWithValue("@id", selID);
                        using (MySqlDataReader r = cmd.ExecuteReader()) { if (r.Read()) { txtID.Text = r["poid"].ToString(); txtSupplierID.Text = r["supplierID"].ToString(); if (r["orderdate"] != DBNull.Value) dtpOrder.Value = Convert.ToDateTime(r["orderdate"]); if (r["expecteddate"] != DBNull.Value) dtpExpected.Value = Convert.ToDateTime(r["expecteddate"]); cboStatus.Text = r["status"].ToString(); txtTotalCost.Text = r["totalcost"].ToString(); } } } }
            }
            catch { }
        }

        private void New_() { selID = "PO-" + DateTime.Now.ToString("yyyyMMddHHmmss"); txtID.Text = selID; txtSupplierID.Text = txtTotalCost.Text = ""; dtpOrder.Value = DateTime.Today; dtpExpected.Value = DateTime.Today.AddDays(14); cboStatus.Text = "Draft"; }

        private void Save_()
        {
            if (string.IsNullOrEmpty(txtID.Text)) { MessageBox.Show("Click New first."); return; }
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    bool ex; using (MySqlCommand chk = new MySqlCommand("SELECT COUNT(*) FROM purchaseorders WHERE poid=@id", cn)) { chk.Parameters.AddWithValue("@id", txtID.Text); ex = (long)chk.ExecuteScalar() > 0; }
                    string sql = ex ? "UPDATE purchaseorders SET supplierID=@s,orderdate=@od,expecteddate=@ed,status=@ss,totalcost=@tc WHERE poid=@id"
                                   : "INSERT INTO purchaseorders(poid,supplierID,orderdate,expecteddate,status,totalcost)VALUES(@id,@s,@od,@ed,@ss,@tc)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, cn)) { cmd.Parameters.AddWithValue("@id", txtID.Text); cmd.Parameters.AddWithValue("@s", txtSupplierID.Text); cmd.Parameters.AddWithValue("@od", dtpOrder.Value.Date); cmd.Parameters.AddWithValue("@ed", dtpExpected.Value.Date); cmd.Parameters.AddWithValue("@ss", cboStatus.Text); decimal tc = 0; decimal.TryParse(txtTotalCost.Text, out tc); cmd.Parameters.AddWithValue("@tc", tc); cmd.ExecuteNonQuery(); }
                    Reload(""); MessageBox.Show("Saved!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            }
            catch (Exception ex2) { MessageBox.Show("Save error: " + ex2.Message); }
        }

        private void Del_()
        {
            if (string.IsNullOrEmpty(selID)) { MessageBox.Show("Select a record."); return; }
            if (MessageBox.Show("Delete " + selID + "?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try { using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open(); using (MySqlCommand cmd = new MySqlCommand("DELETE FROM purchaseorders WHERE poid=@id", cn)) { cmd.Parameters.AddWithValue("@id", selID); cmd.ExecuteNonQuery(); } selID = ""; Reload(""); } }
            catch (Exception ex) { MessageBox.Show("Delete error: " + ex.Message); }
        }
    }
}
