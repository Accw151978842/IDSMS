using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    public class InvoiceForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSrch, txtID, txtCustID, txtOrderID, txtTotal;
        private ComboBox cboStatus;
        private DateTimePicker dtpIssue, dtpDue;
        private string selID = "";

        public InvoiceForm() : this("") { }
        public InvoiceForm(string filterOrderID)
        {
            Text = "Invoice Management"; Size = new System.Drawing.Size(1000, 560); StartPosition = FormStartPosition.CenterParent;
            txtSrch = new TextBox { Location = new System.Drawing.Point(70, 7), Size = new System.Drawing.Size(200, 23) };
            Controls.Add(new Label { Text = "Search:", Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(55, 23) }); Controls.Add(txtSrch);
            var btnSrch = new Button { Text = "Search", Location = new System.Drawing.Point(280, 5), Size = new System.Drawing.Size(80, 27), BackColor = System.Drawing.Color.SteelBlue, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat };
            btnSrch.Click += (s, e) => Reload(txtSrch.Text); Controls.Add(btnSrch);
            dgv = new DataGridView { Location = new System.Drawing.Point(10, 42), Size = new System.Drawing.Size(600, 440), ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = System.Drawing.Color.White };
            dgv.SelectionChanged += Sel; Controls.Add(dgv);
            int px = 625, py = 42, lw = 120, fw = 200;
            Controls.Add(new Label { Text = "Invoice", Location = new System.Drawing.Point(px, py - 22), Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold) });
            txtID = new TextBox { Size = new System.Drawing.Size(fw, 23), ReadOnly = true };
            AddRow(px, ref py, lw, fw, "Invoice ID:", txtID);
            txtOrderID = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Order ID:", txtOrderID);
            txtCustID = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Customer ID:", txtCustID);
            dtpIssue = new DateTimePicker { Size = new System.Drawing.Size(fw, 23), Format = DateTimePickerFormat.Short }; AddRow(px, ref py, lw, fw, "Issue Date:", dtpIssue);
            dtpDue = new DateTimePicker { Size = new System.Drawing.Size(fw, 23), Format = DateTimePickerFormat.Short }; AddRow(px, ref py, lw, fw, "Due Date:", dtpDue);
            cboStatus = new ComboBox { Size = new System.Drawing.Size(fw, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "Unpaid", "PartiallyPaid", "Paid", "Overdue" }); AddRow(px, ref py, lw, fw, "Status:", cboStatus);
            txtTotal = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Total:", txtTotal);
            var btnNew = Btn("New", px, py, System.Drawing.Color.ForestGreen);
            var btnSave = Btn("Save", px + 90, py, System.Drawing.Color.RoyalBlue);
            var btnDel = Btn("Delete", px + 180, py, System.Drawing.Color.Crimson);
            btnNew.Click += (s, e) => New_(filterOrderID); btnSave.Click += (s, e) => Save_(); btnDel.Click += (s, e) => Del_();
            Controls.AddRange(new System.Windows.Forms.Control[] { btnNew, btnSave, btnDel }); Reload(filterOrderID);
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
                    string sql = string.IsNullOrEmpty(s) ? "SELECT * FROM invoices ORDER BY invoiceID DESC LIMIT 300" : "SELECT * FROM invoices WHERE invoiceID LIKE @s OR orderID LIKE @s LIMIT 300";
                    using (MySqlCommand cmd = new MySqlCommand(sql, cn)) { if (!string.IsNullOrEmpty(s)) cmd.Parameters.AddWithValue("@s", "%" + s + "%"); DataTable dt = new DataTable(); new MySqlDataAdapter(cmd).Fill(dt); dgv.DataSource = dt; } }
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        private void Sel(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            DataRowView row = dgv.SelectedRows[0].DataBoundItem as DataRowView; if (row == null) return;
            selID = row["invoiceID"].ToString();
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM invoices WHERE invoiceID=@id", cn)) { cmd.Parameters.AddWithValue("@id", selID);
                        using (MySqlDataReader r = cmd.ExecuteReader()) { if (r.Read()) { txtID.Text = r["invoiceID"].ToString(); txtOrderID.Text = r["orderID"].ToString(); txtCustID.Text = r["customerID"].ToString(); if (r["issuedate"] != DBNull.Value) dtpIssue.Value = Convert.ToDateTime(r["issuedate"]); if (r["duedate"] != DBNull.Value) dtpDue.Value = Convert.ToDateTime(r["duedate"]); cboStatus.Text = r["status"].ToString(); txtTotal.Text = r["total"].ToString(); } } } }
            }
            catch { }
        }

        private void New_(string prefillOrderID)
        {
            selID = "INV-" + DateTime.Now.ToString("yyyyMMddHHmmss"); txtID.Text = selID;
            txtOrderID.Text = prefillOrderID; txtCustID.Text = "";
            dtpIssue.Value = DateTime.Today; dtpDue.Value = DateTime.Today.AddDays(30); cboStatus.Text = "Unpaid"; txtTotal.Text = "0";
        }

        private void Save_()
        {
            if (string.IsNullOrEmpty(txtID.Text)) { MessageBox.Show("Click New first."); return; }
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    bool ex; using (MySqlCommand chk = new MySqlCommand("SELECT COUNT(*) FROM invoices WHERE invoiceID=@id", cn)) { chk.Parameters.AddWithValue("@id", txtID.Text); ex = (long)chk.ExecuteScalar() > 0; }
                    string sql = ex ? "UPDATE invoices SET orderID=@o,customerID=@c,issuedate=@id2,duedate=@dd,status=@ss,total=@tt WHERE invoiceID=@id"
                                   : "INSERT INTO invoices(invoiceID,orderID,customerID,issuedate,duedate,status,total)VALUES(@id,@o,@c,@id2,@dd,@ss,@tt)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, cn)) { cmd.Parameters.AddWithValue("@id", txtID.Text); cmd.Parameters.AddWithValue("@o", txtOrderID.Text); cmd.Parameters.AddWithValue("@c", txtCustID.Text); cmd.Parameters.AddWithValue("@id2", dtpIssue.Value.Date); cmd.Parameters.AddWithValue("@dd", dtpDue.Value.Date); cmd.Parameters.AddWithValue("@ss", cboStatus.Text); decimal tt = 0; decimal.TryParse(txtTotal.Text, out tt); cmd.Parameters.AddWithValue("@tt", tt); cmd.ExecuteNonQuery(); }
                    Reload(""); MessageBox.Show("Saved!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            }
            catch (Exception ex2) { MessageBox.Show("Save error: " + ex2.Message); }
        }

        private void Del_()
        {
            if (string.IsNullOrEmpty(selID)) { MessageBox.Show("Select a record."); return; }
            if (MessageBox.Show("Delete " + selID + "?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try { using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open(); using (MySqlCommand cmd = new MySqlCommand("DELETE FROM invoices WHERE invoiceID=@id", cn)) { cmd.Parameters.AddWithValue("@id", selID); cmd.ExecuteNonQuery(); } selID = ""; Reload(""); } }
            catch (Exception ex) { MessageBox.Show("Delete error: " + ex.Message); }
        }
    }
}
