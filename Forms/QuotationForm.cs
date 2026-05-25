using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    public class QuotationForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSrch, txtID, txtCustID, txtTotalAmt, txtNotes;
        private ComboBox cboStatus;
        private DateTimePicker dtpDate, dtpExpiry;
        private string selID = "";

        public QuotationForm()
        {
            Text = "Quotation Management"; Size = new System.Drawing.Size(1000, 560); StartPosition = FormStartPosition.CenterParent;
            txtSrch = new TextBox { Location = new System.Drawing.Point(70, 7), Size = new System.Drawing.Size(200, 23) };
            Controls.Add(new Label { Text = "Search:", Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(55, 23) }); Controls.Add(txtSrch);
            var btnSrch = new Button { Text = "Search", Location = new System.Drawing.Point(280, 5), Size = new System.Drawing.Size(80, 27), BackColor = System.Drawing.Color.SteelBlue, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat };
            btnSrch.Click += (s, e) => Reload(txtSrch.Text); Controls.Add(btnSrch);
            dgv = new DataGridView { Location = new System.Drawing.Point(10, 42), Size = new System.Drawing.Size(600, 440), ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = System.Drawing.Color.White };
            dgv.SelectionChanged += Sel; Controls.Add(dgv);
            int px = 625, py = 42, lw = 120, fw = 200;
            Controls.Add(new Label { Text = "Quotation", Location = new System.Drawing.Point(px, py - 22), Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold) });
            txtID = new TextBox { Size = new System.Drawing.Size(fw, 23), ReadOnly = true };
            AddRow(px, ref py, lw, fw, "Quotation ID:", txtID);
            txtCustID = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Customer ID:", txtCustID);
            dtpDate = new DateTimePicker { Size = new System.Drawing.Size(fw, 23), Format = DateTimePickerFormat.Short }; AddRow(px, ref py, lw, fw, "Date:", dtpDate);
            dtpExpiry = new DateTimePicker { Size = new System.Drawing.Size(fw, 23), Format = DateTimePickerFormat.Short }; AddRow(px, ref py, lw, fw, "Expiry Date:", dtpExpiry);
            cboStatus = new ComboBox { Size = new System.Drawing.Size(fw, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "Draft", "Sent", "Accepted", "Rejected", "Expired" }); AddRow(px, ref py, lw, fw, "Status:", cboStatus);
            txtTotalAmt = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Total Amount:", txtTotalAmt);
            txtNotes = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Notes:", txtNotes);
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
                    string sql = string.IsNullOrEmpty(s) ? "SELECT * FROM quotations ORDER BY quotationdate DESC LIMIT 300" : "SELECT * FROM quotations WHERE quotationID LIKE @s OR customerID LIKE @s LIMIT 300";
                    using (MySqlCommand cmd = new MySqlCommand(sql, cn)) { if (!string.IsNullOrEmpty(s)) cmd.Parameters.AddWithValue("@s", "%" + s + "%"); DataTable dt = new DataTable(); new MySqlDataAdapter(cmd).Fill(dt); dgv.DataSource = dt; } }
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        private void Sel(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            DataRowView row = dgv.SelectedRows[0].DataBoundItem as DataRowView; if (row == null) return;
            selID = row["quotationID"].ToString();
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM quotations WHERE quotationID=@id", cn)) { cmd.Parameters.AddWithValue("@id", selID);
                        using (MySqlDataReader r = cmd.ExecuteReader()) { if (r.Read()) { txtID.Text = r["quotationID"].ToString(); txtCustID.Text = r["customerID"].ToString(); if (r["quotationdate"] != DBNull.Value) dtpDate.Value = Convert.ToDateTime(r["quotationdate"]); if (r["expirydate"] != DBNull.Value) dtpExpiry.Value = Convert.ToDateTime(r["expirydate"]); cboStatus.Text = r["status"].ToString(); txtTotalAmt.Text = r["totalamt"].ToString(); txtNotes.Text = r["notes"].ToString(); } } } }
            }
            catch { }
        }

        private void New_() { selID = "QT-" + DateTime.Now.ToString("yyyyMMddHHmmss"); txtID.Text = selID; txtCustID.Text = txtTotalAmt.Text = txtNotes.Text = ""; dtpDate.Value = DateTime.Today; dtpExpiry.Value = DateTime.Today.AddDays(30); cboStatus.Text = "Draft"; }

        private void Save_()
        {
            if (string.IsNullOrEmpty(txtID.Text)) { MessageBox.Show("Click New first."); return; }
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    bool ex; using (MySqlCommand chk = new MySqlCommand("SELECT COUNT(*) FROM quotations WHERE quotationID=@id", cn)) { chk.Parameters.AddWithValue("@id", txtID.Text); ex = (long)chk.ExecuteScalar() > 0; }
                    string sql = ex ? "UPDATE quotations SET customerID=@c,quotationdate=@qd,expirydate=@ed,status=@ss,totalamt=@ta,notes=@no WHERE quotationID=@id"
                                   : "INSERT INTO quotations(quotationID,customerID,quotationdate,expirydate,status,totalamt,notes)VALUES(@id,@c,@qd,@ed,@ss,@ta,@no)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, cn)) { cmd.Parameters.AddWithValue("@id", txtID.Text); cmd.Parameters.AddWithValue("@c", txtCustID.Text); cmd.Parameters.AddWithValue("@qd", dtpDate.Value.Date); cmd.Parameters.AddWithValue("@ed", dtpExpiry.Value.Date); cmd.Parameters.AddWithValue("@ss", cboStatus.Text); decimal ta = 0; decimal.TryParse(txtTotalAmt.Text, out ta); cmd.Parameters.AddWithValue("@ta", ta); cmd.Parameters.AddWithValue("@no", txtNotes.Text); cmd.ExecuteNonQuery(); }
                    Reload(""); MessageBox.Show("Saved!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            }
            catch (Exception ex2) { MessageBox.Show("Save error: " + ex2.Message); }
        }

        private void Del_()
        {
            if (string.IsNullOrEmpty(selID)) { MessageBox.Show("Select a record."); return; }
            if (MessageBox.Show("Delete " + selID + "?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try { using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open(); using (MySqlCommand cmd = new MySqlCommand("DELETE FROM quotations WHERE quotationID=@id", cn)) { cmd.Parameters.AddWithValue("@id", selID); cmd.ExecuteNonQuery(); } selID = ""; Reload(""); } }
            catch (Exception ex) { MessageBox.Show("Delete error: " + ex.Message); }
        }
    }
}
