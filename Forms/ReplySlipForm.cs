using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    public class ReplySlipForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSrch, txtID, txtOrderID, txtCustID, txtSignedBy;
        private ComboBox cboStatus;
        private string selID = "";

        public ReplySlipForm() : this("") { }
        public ReplySlipForm(string filterID)
        {
            Text = "Reply Slip / Delivery Confirmation"; Size = new System.Drawing.Size(1000, 520); StartPosition = FormStartPosition.CenterParent;
            txtSrch = new TextBox { Location = new System.Drawing.Point(70, 7), Size = new System.Drawing.Size(200, 23) };
            Controls.Add(new Label { Text = "Search:", Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(55, 23) }); Controls.Add(txtSrch);
            var btnSrch = new Button { Text = "Search", Location = new System.Drawing.Point(280, 5), Size = new System.Drawing.Size(80, 27), BackColor = System.Drawing.Color.SteelBlue, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat };
            btnSrch.Click += (s, e) => Reload(txtSrch.Text); Controls.Add(btnSrch);
            dgv = new DataGridView { Location = new System.Drawing.Point(10, 42), Size = new System.Drawing.Size(600, 400), ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = System.Drawing.Color.White };
            dgv.SelectionChanged += Sel; Controls.Add(dgv);
            int px = 625, py = 42, lw = 140, fw = 200;
            Controls.Add(new Label { Text = "Reply Slip (DN Confirm)", Location = new System.Drawing.Point(px, py - 22), Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold) });
            txtID = new TextBox { Size = new System.Drawing.Size(fw, 23), ReadOnly = true };
            AddRow(px, ref py, lw, fw, "DN ID:", txtID);
            txtOrderID = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Order ID:", txtOrderID);
            txtCustID = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Customer ID:", txtCustID);
            txtSignedBy = new TextBox { Size = new System.Drawing.Size(fw, 23) }; AddRow(px, ref py, lw, fw, "Signed By:", txtSignedBy);
            cboStatus = new ComboBox { Size = new System.Drawing.Size(fw, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new object[] { "Prepared", "Dispatched", "Delivered", "Confirmed" }); AddRow(px, ref py, lw, fw, "Status:", cboStatus);
            var btnSave = new Button { Text = "Confirm Delivery", Location = new System.Drawing.Point(px, py), Size = new System.Drawing.Size(150, 33), BackColor = System.Drawing.Color.SeaGreen, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.Click += Save_; Controls.Add(btnSave);
            Reload(filterID);
        }

        private void AddRow(int px, ref int py, int lw, int fw, string lbl, System.Windows.Forms.Control ctrl)
        { Controls.Add(new Label { Text = lbl, Location = new System.Drawing.Point(px, py), Size = new System.Drawing.Size(lw, 23) }); ctrl.Location = new System.Drawing.Point(px + lw + 5, py); Controls.Add(ctrl); py += 35; }

        private void Reload(string s)
        {
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    string sql = string.IsNullOrEmpty(s) ? "SELECT dnID,orderID,customerID,driver,signedby,status FROM deliverynotes ORDER BY dnID DESC LIMIT 300" : "SELECT dnID,orderID,customerID,driver,signedby,status FROM deliverynotes WHERE dnID LIKE @s OR orderID LIKE @s LIMIT 300";
                    using (MySqlCommand cmd = new MySqlCommand(sql, cn)) { if (!string.IsNullOrEmpty(s)) cmd.Parameters.AddWithValue("@s", "%" + s + "%"); DataTable dt = new DataTable(); new MySqlDataAdapter(cmd).Fill(dt); dgv.DataSource = dt; } }
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        private void Sel(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            DataRowView row = dgv.SelectedRows[0].DataBoundItem as DataRowView; if (row == null) return;
            selID = row["dnID"].ToString();
            txtID.Text = selID; txtOrderID.Text = row["orderID"].ToString();
            txtCustID.Text = row["customerID"].ToString(); txtSignedBy.Text = row["signedby"].ToString(); cboStatus.Text = row["status"].ToString();
        }

        private void Save_(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selID)) { MessageBox.Show("Select a delivery note first."); return; }
            if (string.IsNullOrWhiteSpace(txtSignedBy.Text)) { MessageBox.Show("Signed By is required."); return; }
            try
            {
                using (MySqlConnection cn = DBConnection.GetConnection()) { cn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("UPDATE deliverynotes SET signedby=@sb,status=@ss WHERE dnID=@id", cn)) { cmd.Parameters.AddWithValue("@sb", txtSignedBy.Text); cmd.Parameters.AddWithValue("@ss", cboStatus.Text); cmd.Parameters.AddWithValue("@id", selID); cmd.ExecuteNonQuery(); }
                    Reload(""); MessageBox.Show("Delivery confirmed!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            }
            catch (Exception ex) { MessageBox.Show("Save error: " + ex.Message); }
        }
    }
}
