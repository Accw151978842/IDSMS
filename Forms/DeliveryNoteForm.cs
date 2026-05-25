using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    public class DeliveryNoteForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSrch, txtID, txtOrderID, txtCustID, txtAddress, txtDriver, txtSignedBy;
        private ComboBox cboDM, cboStatus;
        private DateTimePicker dtpDN, dtpDispatch;
        private string selID = "";

        public DeliveryNoteForm() : this("") { }
        public DeliveryNoteForm(string filterID)
        {
            Text = "Delivery Note Management"; Size = new System.Drawing.Size(1000, 640); StartPosition = FormStartPosition.CenterParent;
            txtSrch = new TextBox { Location = new System.Drawing.Point(70, 7), Size = new System.Drawing.Size(200, 23) };
            Controls.Add(new Label { Text = "Search:", Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(55, 23) }); Controls.Add(txtSrch);
            var btnSrch = new Button { Text = "Search", Location = new System.Drawing.Point(280, 5), Size = new System.Drawing.Size(80, 27), BackColor = System.Drawing.Color.SteelBlue, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat };
            btnSrch.Click += (s, e) => Reload(txtSrch.Text); Controls.Add(btnSrch);
            dgv = new DataGridView { Location = new System.Drawing.Point(10, 42), Size = new System.Drawing.Size(600, 490), ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = System.Drawing.Color.White };
            dgv.SelectionChanged += Sel; Controls.Add(dgv);
            int px = 625, py = 42, lw = 140, fw = 200;
            Controls.Add(new Label { Text = "Delivery Note", Location = new System.Drawing.Point(px, py - 22), Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold) });
            txtID = new TextBox { Size = new System.Drawing.Size(fw, 23), ReadOnly = true };
            AddRow(px, ref py, lw, fw, "DN ID:", txtID);
            txtOrderID = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Order ID:", txtOrderID);
            txtCustID = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Customer ID:", txtCustID);
            dtpDN = new DateTimePicker { Size = new System.Drawing.Size(fw, 23), Format = DateTimePickerFormat.Short }; AddRow(px, ref py, lw, fw, "DN Date:", dtpDN);
            dtpDispatch = new DateTimePicker { Size = new System.Drawing.Size(fw, 23), Format = DateTimePickerFormat.Short }; AddRow(px, ref py, lw, fw, "Dispatch Date:", dtpDispatch);
            cboDM = new ComboBox { Size = new System.Drawing.Size(fw, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cboDM.Items.AddRange(new object[] { "Courier", "Truck", "Self-collect" }); AddRow(px, ref py, lw, fw, "Delivery Method:", cboDM);
            txtAddress = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Shipping Address:", txtAddress);
            txtDriver = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Driver:", txtDriver);
            txtSignedBy = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Signed By:", txtSignedBy);
            cboStatus = new ComboBox { Size = new System.Drawing.Size(fw, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "Prepared", "Dispatched", "Delivered", "Confirmed" }); AddRow(px, ref py, lw, fw, "Status:", cboStatus);
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
                    string sql = string.IsNullOrEmpty(s) ? "SELECT * FROM deliverynotes ORDER BY dnID DESC LIMIT 300" : "SELECT * FROM deliverynotes WHERE dnID LIKE @s OR orderID LIKE @s LIMIT 300";
                    using (MySqlCommand cmd = new MySqlCommand(sql, cn)) { if (!string.IsNullOrEmpty(s)) cmd.Parameters.AddWithValue("@s", "%" + s + "%"); DataTable dt = new DataTable(); new MySqlDataAdapter(cmd).Fill(dt); dgv.DataSource = dt; } }
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        private void Sel(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            DataRowView row = dgv.SelectedRows[0].DataBoundItem as DataRowView; if (row == null) return;
            selID = row["dnID"].ToString();
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM deliverynotes WHERE dnID=@id", cn)) { cmd.Parameters.AddWithValue("@id", selID);
                        using (MySqlDataReader r = cmd.ExecuteReader()) { if (r.Read()) { txtID.Text = r["dnID"].ToString(); txtOrderID.Text = r["orderID"].ToString(); txtCustID.Text = r["customerID"].ToString(); if (r["dndate"] != DBNull.Value) dtpDN.Value = Convert.ToDateTime(r["dndate"]); if (r["dispatchdate"] != DBNull.Value) dtpDispatch.Value = Convert.ToDateTime(r["dispatchdate"]); cboDM.Text = r["deliverymethod"].ToString(); txtAddress.Text = r["shippingaddress"].ToString(); txtDriver.Text = r["driver"].ToString(); txtSignedBy.Text = r["signedby"].ToString(); cboStatus.Text = r["status"].ToString(); } } } }
            }
            catch { }
        }

        private void New_() { selID = "DN-" + DateTime.Now.ToString("yyyyMMddHHmmss"); txtID.Text = selID; txtOrderID.Text = txtCustID.Text = txtAddress.Text = txtDriver.Text = txtSignedBy.Text = ""; dtpDN.Value = dtpDispatch.Value = DateTime.Today; cboDM.SelectedIndex = -1; cboStatus.Text = "Prepared"; }

        private void Save_()
        {
            if (string.IsNullOrEmpty(txtID.Text)) { MessageBox.Show("Click New first."); return; }
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    bool ex; using (MySqlCommand chk = new MySqlCommand("SELECT COUNT(*) FROM deliverynotes WHERE dnID=@id", cn)) { chk.Parameters.AddWithValue("@id", txtID.Text); ex = (long)chk.ExecuteScalar() > 0; }
                    string sql = ex ? "UPDATE deliverynotes SET orderID=@o,customerID=@c,dndate=@dd,dispatchdate=@dp,deliverymethod=@dm,shippingaddress=@sa,driver=@dr,signedby=@sb,status=@ss WHERE dnID=@id"
                                   : "INSERT INTO deliverynotes(dnID,orderID,customerID,dndate,dispatchdate,deliverymethod,shippingaddress,driver,signedby,status)VALUES(@id,@o,@c,@dd,@dp,@dm,@sa,@dr,@sb,@ss)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, cn)) { cmd.Parameters.AddWithValue("@id", txtID.Text); cmd.Parameters.AddWithValue("@o", txtOrderID.Text); cmd.Parameters.AddWithValue("@c", txtCustID.Text); cmd.Parameters.AddWithValue("@dd", dtpDN.Value.Date); cmd.Parameters.AddWithValue("@dp", dtpDispatch.Value.Date); cmd.Parameters.AddWithValue("@dm", cboDM.Text); cmd.Parameters.AddWithValue("@sa", txtAddress.Text); cmd.Parameters.AddWithValue("@dr", txtDriver.Text); cmd.Parameters.AddWithValue("@sb", txtSignedBy.Text); cmd.Parameters.AddWithValue("@ss", cboStatus.Text); cmd.ExecuteNonQuery(); }
                    Reload(""); MessageBox.Show("Saved!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            }
            catch (Exception ex2) { MessageBox.Show("Save error: " + ex2.Message); }
        }

        private void Del_()
        {
            if (string.IsNullOrEmpty(selID)) { MessageBox.Show("Select a record."); return; }
            if (MessageBox.Show("Delete " + selID + "?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try { using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open(); using (MySqlCommand cmd = new MySqlCommand("DELETE FROM deliverynotes WHERE dnID=@id", cn)) { cmd.Parameters.AddWithValue("@id", selID); cmd.ExecuteNonQuery(); } selID = ""; Reload(""); } }
            catch (Exception ex) { MessageBox.Show("Delete error: " + ex.Message); }
        }
    }
}
