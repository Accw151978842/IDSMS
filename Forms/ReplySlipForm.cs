using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    // Reply Slip: confirmation/sign-off on a Delivery Note
    public class ReplySlipForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSrch, txtID, txt_orderID, txt_customerID, txt_signedby;
        private ComboBox cbo_status;
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
            void Row(string lbl, Control ctrl) { Controls.Add(new Label { Text = lbl, Location = new System.Drawing.Point(px, py), Size = new System.Drawing.Size(lw, 23) }); ctrl.Location = new System.Drawing.Point(px + lw + 5, py); Controls.Add(ctrl); py += 35; }
            Row("DN ID:", txtID);
            txt_orderID = new TextBox { Size = new System.Drawing.Size(fw, 23) }; Row("Order ID:", txt_orderID);
            txt_customerID = new TextBox { Size = new System.Drawing.Size(fw, 23) }; Row("Customer ID:", txt_customerID);
            txt_signedby = new TextBox { Size = new System.Drawing.Size(fw, 23) }; Row("Signed By:", txt_signedby);
            cbo_status = new ComboBox { Size = new System.Drawing.Size(fw, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cbo_status.Items.AddRange(new object[] { "Prepared", "Dispatched", "Delivered", "Confirmed" }); Row("Status:", cbo_status);
            var btnSave = new Button { Text = "Confirm Delivery", Location = new System.Drawing.Point(px, py), Size = new System.Drawing.Size(150, 33), BackColor = System.Drawing.Color.SeaGreen, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat };
            btnSave.Click += Save_; Controls.Add(btnSave);
            Reload(filterID);
        }
        private void Reload(string s = "") {
            try { using var cn = DBConnection.GetConnection(); cn.Open();
                string sql = string.IsNullOrEmpty(s) ? "SELECT dnID,orderID,customerID,driver,signedby,status FROM deliverynotes ORDER BY dnID DESC LIMIT 300" : "SELECT dnID,orderID,customerID,driver,signedby,status FROM deliverynotes WHERE dnID LIKE @s OR orderID LIKE @s LIMIT 300";
                using var cmd = new MySqlCommand(sql, cn); if (!string.IsNullOrEmpty(s)) cmd.Parameters.AddWithValue("@s", $"%{s}%");
                var dt = new DataTable(); new MySqlDataAdapter(cmd).Fill(dt); dgv.DataSource = dt;
            } catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); } }
        private void Sel(object? s, EventArgs e) {
            if (dgv.SelectedRows.Count == 0) return;
            var row = dgv.SelectedRows[0].DataBoundItem as DataRowView; if (row == null) return;
            selID = row["dnID"]?.ToString() ?? "";
            txtID.Text = selID; txt_orderID.Text = row["orderID"]?.ToString() ?? "";
            txt_customerID.Text = row["customerID"]?.ToString() ?? "";
            txt_signedby.Text = row["signedby"]?.ToString() ?? ""; cbo_status.Text = row["status"]?.ToString() ?? ""; }
        private void Save_(object? s, EventArgs e) {
            if (string.IsNullOrEmpty(selID)) { MessageBox.Show("Select a delivery note first."); return; }
            if (string.IsNullOrWhiteSpace(txt_signedby.Text)) { MessageBox.Show("Signed By is required."); return; }
            try { using var cn = DBConnection.GetConnection(); cn.Open();
                using var cmd = new MySqlCommand("UPDATE deliverynotes SET signedby=@sb,status=@ss WHERE dnID=@id", cn);
                cmd.Parameters.AddWithValue("@sb", txt_signedby.Text); cmd.Parameters.AddWithValue("@ss", cbo_status.Text); cmd.Parameters.AddWithValue("@id", selID);
                cmd.ExecuteNonQuery(); Reload(); MessageBox.Show("Delivery confirmed!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } catch (Exception ex) { MessageBox.Show("Save error: " + ex.Message); } }
    }
}
