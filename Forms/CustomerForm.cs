using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    public class CustomerForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSrch, txtID, txtName, txtContact, txtEmail, txtBill, txtShip;
        private ComboBox cboType;
        private string selID = "";

        public CustomerForm()
        {
            Text = "Customer Management"; Size = new System.Drawing.Size(960, 540); StartPosition = FormStartPosition.CenterParent;
            txtSrch = new TextBox { Location = new System.Drawing.Point(70, 7), Size = new System.Drawing.Size(200, 23) };
            Controls.Add(new Label { Text = "Search:", Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(55, 23) });
            Controls.Add(txtSrch);
            var btnSrch = new Button { Text = "Search", Location = new System.Drawing.Point(280, 5), Size = new System.Drawing.Size(80, 27),
                BackColor = System.Drawing.Color.SteelBlue, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat };
            btnSrch.Click += (s, e) => Reload(txtSrch.Text); Controls.Add(btnSrch);
            dgv = new DataGridView { Location = new System.Drawing.Point(10, 42), Size = new System.Drawing.Size(580, 420),
                ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = System.Drawing.Color.White };
            dgv.SelectionChanged += Sel; Controls.Add(dgv);
            int px = 610, py = 42, lw = 130, fw = 190;
            void Row(string lbl, Control ctrl) {
                Controls.Add(new Label { Text = lbl, Location = new System.Drawing.Point(px, py), Size = new System.Drawing.Size(lw, 23) });
                ctrl.Location = new System.Drawing.Point(px + lw + 5, py); Controls.Add(ctrl); py += 35; }
            Row("Customer ID:", txtID = new TextBox { Size = new System.Drawing.Size(fw, 23), ReadOnly = true });
            Row("Name *:", txtName = new TextBox { Size = new System.Drawing.Size(fw, 23) });
            Row("Contact:", txtContact = new TextBox { Size = new System.Drawing.Size(fw, 23) });
            Row("Email:", txtEmail = new TextBox { Size = new System.Drawing.Size(fw, 23) });
            Row("Billing Address:", txtBill = new TextBox { Size = new System.Drawing.Size(fw, 23) });
            Row("Shipping Address:", txtShip = new TextBox { Size = new System.Drawing.Size(fw, 23) });
            cboType = new ComboBox { Size = new System.Drawing.Size(fw, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            cboType.Items.AddRange(new object[] { "B2B", "B2C" });
            Row("Type:", cboType);
            var btnNew  = Btn("New",    px,       py, System.Drawing.Color.ForestGreen);
            var btnSave = Btn("Save",   px + 90,  py, System.Drawing.Color.RoyalBlue);
            var btnDel  = Btn("Delete", px + 180, py, System.Drawing.Color.Crimson);
            btnNew.Click += (s,e)=>New_(); btnSave.Click += (s,e)=>Save_(); btnDel.Click += (s,e)=>Del_();
            Controls.AddRange(new Control[] { btnNew, btnSave, btnDel });
            Reload();
        }
        private Button Btn(string t, int x, int y, System.Drawing.Color c) =>
            new Button { Text=t, Location=new System.Drawing.Point(x,y), Size=new System.Drawing.Size(80,30),
                BackColor=c, ForeColor=System.Drawing.Color.White, FlatStyle=FlatStyle.Flat };
        private void Reload(string s = "") {
            try {
                using var cn = DBConnection.GetConnection(); cn.Open();
                string sql = string.IsNullOrEmpty(s)
                    ? "SELECT customerID,name,contact,email,type FROM customer ORDER BY name"
                    : "SELECT customerID,name,contact,email,type FROM customer WHERE name LIKE @s OR customerID LIKE @s ORDER BY name";
                using var cmd = new MySqlCommand(sql, cn);
                if (!string.IsNullOrEmpty(s)) cmd.Parameters.AddWithValue("@s", $"%{s}%");
                var dt = new DataTable(); new MySqlDataAdapter(cmd).Fill(dt); dgv.DataSource = dt;
            } catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
        private void Sel(object? s, EventArgs e) {
            if (dgv.SelectedRows.Count == 0) return;
            var row = dgv.SelectedRows[0].DataBoundItem as DataRowView; if (row == null) return;
            selID = row["customerID"]?.ToString() ?? "";
            try {
                using var cn = DBConnection.GetConnection(); cn.Open();
                using var cmd = new MySqlCommand("SELECT * FROM customer WHERE customerID=@id", cn);
                cmd.Parameters.AddWithValue("@id", selID);
                using var r = cmd.ExecuteReader();
                if (r.Read()) { txtID.Text=r["customerID"].ToString(); txtName.Text=r["name"].ToString();
                    txtContact.Text=r["contact"].ToString(); txtEmail.Text=r["email"].ToString();
                    txtBill.Text=r["billingAddress"].ToString(); txtShip.Text=r["shippingAddress"].ToString();
                    cboType.Text=r["type"].ToString(); }
            } catch { }
        }
        private void New_() {
            selID = "CUST" + DateTime.Now.ToString("yyMMddHHmmss"); txtID.Text = selID;
            txtName.Text=txtContact.Text=txtEmail.Text=txtBill.Text=txtShip.Text=""; cboType.SelectedIndex=-1;
        }
        private void Save_() {
            if (string.IsNullOrEmpty(txtID.Text)) { MessageBox.Show("Click New first."); return; }
            if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Name required."); return; }
            try {
                using var cn = DBConnection.GetConnection(); cn.Open();
                bool ex; using (var chk = new MySqlCommand("SELECT COUNT(*) FROM customer WHERE customerID=@id", cn))
                { chk.Parameters.AddWithValue("@id", txtID.Text); ex = (long)chk.ExecuteScalar() > 0; }
                string sql = ex
                    ? "UPDATE customer SET name=@n,contact=@c,email=@e,billingAddress=@ba,shippingAddress=@sa,type=@t WHERE customerID=@id"
                    : "INSERT INTO customer(customerID,name,contact,email,billingAddress,shippingAddress,type)VALUES(@id,@n,@c,@e,@ba,@sa,@t)";
                using var cmd = new MySqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@id", txtID.Text); cmd.Parameters.AddWithValue("@n", txtName.Text);
                cmd.Parameters.AddWithValue("@c", txtContact.Text); cmd.Parameters.AddWithValue("@e", txtEmail.Text);
                cmd.Parameters.AddWithValue("@ba", txtBill.Text); cmd.Parameters.AddWithValue("@sa", txtShip.Text);
                cmd.Parameters.AddWithValue("@t", cboType.Text); cmd.ExecuteNonQuery();
                Audit(cn, ex?"UPDATE":"CREATE", "customer", txtID.Text);
                Reload(); MessageBox.Show("Saved!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } catch (Exception ex2) { MessageBox.Show("Error: " + ex2.Message); }
        }
        private void Del_() {
            if (string.IsNullOrEmpty(selID)) { MessageBox.Show("Select a record."); return; }
            if (MessageBox.Show($"Delete {selID}?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try {
                using var cn = DBConnection.GetConnection(); cn.Open();
                using var cmd = new MySqlCommand("DELETE FROM customer WHERE customerID=@id", cn);
                cmd.Parameters.AddWithValue("@id", selID); cmd.ExecuteNonQuery();
                Audit(cn, "DELETE", "customer", selID); selID=""; Reload();
            } catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
        private static void Audit(MySqlConnection cn, string act, string mod, string rid) {
            try {
                using var cmd = new MySqlCommand(
                    "INSERT IGNORE INTO audittrail(auditID,userID,actioncode,module,recordID,details,timestamp)VALUES(@a,@u,@ac,@m,@r,@d,NOW())", cn);
                cmd.Parameters.AddWithValue("@a", "AUD"+DateTime.Now.ToString("yyyyMMddHHmmss"));
                cmd.Parameters.AddWithValue("@u", Program.CurrentUserID);
                cmd.Parameters.AddWithValue("@ac", act); cmd.Parameters.AddWithValue("@m", mod);
                cmd.Parameters.AddWithValue("@r", rid); cmd.Parameters.AddWithValue("@d", $"{act} {mod} {rid}");
                cmd.ExecuteNonQuery();
            } catch { }
        }
    }
}
