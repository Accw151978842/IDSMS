using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    public class InventoryForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSrch, txtID, txtItemID, txtQty, txtLocation, txtMinStock;
        private ComboBox cboStatus;
        private string selID = "";

        public InventoryForm() : this("") { }
        public InventoryForm(string filterID)
        {
            Text = "Inventory Management"; Size = new System.Drawing.Size(1000, 560); StartPosition = FormStartPosition.CenterParent;
            txtSrch = new TextBox { Location = new System.Drawing.Point(70, 7), Size = new System.Drawing.Size(200, 23) };
            Controls.Add(new Label { Text = "Search:", Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(55, 23) }); Controls.Add(txtSrch);
            var btnSrch = new Button { Text = "Search", Location = new System.Drawing.Point(280, 5), Size = new System.Drawing.Size(80, 27), BackColor = System.Drawing.Color.SteelBlue, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat };
            btnSrch.Click += (s, e) => Reload(txtSrch.Text); Controls.Add(btnSrch);
            dgv = new DataGridView { Location = new System.Drawing.Point(10, 42), Size = new System.Drawing.Size(600, 440), ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = System.Drawing.Color.White };
            dgv.SelectionChanged += Sel; Controls.Add(dgv);
            int px = 625, py = 42, lw = 140, fw = 200;
            Controls.Add(new Label { Text = "Inventory Record", Location = new System.Drawing.Point(px, py - 22), Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold) });
            txtID = new TextBox { Size = new System.Drawing.Size(fw, 23), ReadOnly = true };
            AddRow(px, ref py, lw, fw, "Inventory ID:", txtID);
            txtItemID = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Item ID:", txtItemID);
            txtQty = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Quantity:", txtQty);
            txtLocation = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Location:", txtLocation);
            txtMinStock = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Min Stock Level:", txtMinStock);
            cboStatus = new ComboBox { Size = new System.Drawing.Size(fw, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "InStock", "LowStock", "OutOfStock" }); AddRow(px, ref py, lw, fw, "Status:", cboStatus);
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
                    string sql = string.IsNullOrEmpty(s)
                        ? "SELECT inv.inventoryID,i.itemID,i.description,inv.qty,inv.location,inv.minstocklv,inv.status FROM inventory inv LEFT JOIN items i ON inv.itemID=i.itemID ORDER BY inv.status,inv.qty LIMIT 300"
                        : "SELECT inv.inventoryID,i.itemID,i.description,inv.qty,inv.location,inv.minstocklv,inv.status FROM inventory inv LEFT JOIN items i ON inv.itemID=i.itemID WHERE inv.inventoryID LIKE @s OR inv.itemID LIKE @s LIMIT 300";
                    using (MySqlCommand cmd = new MySqlCommand(sql, cn)) { if (!string.IsNullOrEmpty(s)) cmd.Parameters.AddWithValue("@s", "%" + s + "%"); DataTable dt = new DataTable(); new MySqlDataAdapter(cmd).Fill(dt); dgv.DataSource = dt; } }
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        private void Sel(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            DataRowView row = dgv.SelectedRows[0].DataBoundItem as DataRowView; if (row == null) return;
            selID = row["inventoryID"].ToString();
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM inventory WHERE inventoryID=@id", cn)) { cmd.Parameters.AddWithValue("@id", selID);
                        using (MySqlDataReader r = cmd.ExecuteReader()) { if (r.Read()) { txtID.Text = r["inventoryID"].ToString(); txtItemID.Text = r["itemID"].ToString(); txtQty.Text = r["qty"].ToString(); txtLocation.Text = r["location"].ToString(); txtMinStock.Text = r["minstocklv"].ToString(); cboStatus.Text = r["status"].ToString(); } } } }
            }
            catch { }
        }

        private void New_() { selID = "INV-" + DateTime.Now.ToString("yyyyMMddHHmmss"); txtID.Text = selID; txtItemID.Text = txtQty.Text = txtLocation.Text = txtMinStock.Text = ""; cboStatus.Text = "InStock"; }

        private void Save_()
        {
            if (string.IsNullOrEmpty(txtID.Text)) { MessageBox.Show("Click New first."); return; }
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    bool ex; using (MySqlCommand chk = new MySqlCommand("SELECT COUNT(*) FROM inventory WHERE inventoryID=@id", cn)) { chk.Parameters.AddWithValue("@id", txtID.Text); ex = (long)chk.ExecuteScalar() > 0; }
                    string sql = ex ? "UPDATE inventory SET itemID=@i,qty=@q,location=@l,minstocklv=@ms,status=@ss WHERE inventoryID=@id"
                                   : "INSERT INTO inventory(inventoryID,itemID,qty,location,minstocklv,status)VALUES(@id,@i,@q,@l,@ms,@ss)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, cn)) { cmd.Parameters.AddWithValue("@id", txtID.Text); cmd.Parameters.AddWithValue("@i", txtItemID.Text); int q = 0; int.TryParse(txtQty.Text, out q); cmd.Parameters.AddWithValue("@q", q); cmd.Parameters.AddWithValue("@l", txtLocation.Text); int ms = 0; int.TryParse(txtMinStock.Text, out ms); cmd.Parameters.AddWithValue("@ms", ms); cmd.Parameters.AddWithValue("@ss", cboStatus.Text); cmd.ExecuteNonQuery(); }
                    Reload(""); MessageBox.Show("Saved!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            }
            catch (Exception ex2) { MessageBox.Show("Save error: " + ex2.Message); }
        }

        private void Del_()
        {
            if (string.IsNullOrEmpty(selID)) { MessageBox.Show("Select a record."); return; }
            if (MessageBox.Show("Delete " + selID + "?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try { using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open(); using (MySqlCommand cmd = new MySqlCommand("DELETE FROM inventory WHERE inventoryID=@id", cn)) { cmd.Parameters.AddWithValue("@id", selID); cmd.ExecuteNonQuery(); } selID = ""; Reload(""); } }
            catch (Exception ex) { MessageBox.Show("Delete error: " + ex.Message); }
        }
    }
}
