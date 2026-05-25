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
        private TextBox txtSrch, txtID, txt_customerID, txt_paymentmethod, txt_versionno;
        private ComboBox cbo_status, cbo_deliverymethod;
        private DateTimePicker dtp_issuedate, dtp_validuntil, dtp_estimateddate;
        private string selID = "";

        public QuotationForm() : this("") { }
        public QuotationForm(string filterID)
        {
            Text = "Quotation Management"; Size = new System.Drawing.Size(1000, 600); StartPosition = FormStartPosition.CenterParent;
            txtSrch = new TextBox { Location = new System.Drawing.Point(70, 7), Size = new System.Drawing.Size(200, 23) };
            Controls.Add(new Label { Text = "Search:", Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(55, 23) }); Controls.Add(txtSrch);
            var btnSrch = new Button { Text = "Search", Location = new System.Drawing.Point(280, 5), Size = new System.Drawing.Size(80, 27),
                BackColor = System.Drawing.Color.SteelBlue, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat };
            btnSrch.Click += (s, e) => Reload(txtSrch.Text); Controls.Add(btnSrch);
            dgv = new DataGridView { Location = new System.Drawing.Point(10, 42), Size = new System.Drawing.Size(600, 460),
                ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = System.Drawing.Color.White };
            dgv.SelectionChanged += Sel; Controls.Add(dgv);
            int px = 625, py = 42, lw = 140, fw = 200;
            Controls.Add(new Label { Text = "Quotation Details", Location = new System.Drawing.Point(px, py-22),
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold) });
            txtID = new TextBox { Size = new System.Drawing.Size(fw, 23), ReadOnly = true };
            void Row(string lbl, Control ctrl) {
                Controls.Add(new Label { Text = lbl, Location = new System.Drawing.Point(px, py), Size = new System.Drawing.Size(lw, 23) });
                ctrl.Location = new System.Drawing.Point(px + lw + 5, py); Controls.Add(ctrl); py += 33; }
            Row("Quotation ID:", txtID);
            txt_customerID = new TextBox { Size = new System.Drawing.Size(fw,23) }; Row("Customer ID:", txt_customerID);
            dtp_issuedate = new DateTimePicker { Size = new System.Drawing.Size(fw,23), Format=DateTimePickerFormat.Short }; Row("Issue Date:", dtp_issuedate);
            dtp_validuntil = new DateTimePicker { Size = new System.Drawing.Size(fw,23), Format=DateTimePickerFormat.Short }; Row("Valid Until:", dtp_validuntil);
            cbo_status = new ComboBox { Size = new System.Drawing.Size(fw,23), DropDownStyle=ComboBoxStyle.DropDownList };
            cbo_status.Items.AddRange(new object[]{"Draft","Sent","Accepted","Rejected","Expired"}); Row("Status:", cbo_status);
            txt_paymentmethod = new TextBox { Size = new System.Drawing.Size(fw,23) }; Row("Payment Method:", txt_paymentmethod);
            cbo_deliverymethod = new ComboBox { Size = new System.Drawing.Size(fw,23), DropDownStyle=ComboBoxStyle.DropDownList };
            cbo_deliverymethod.Items.AddRange(new object[]{"Courier","Truck","Self-collect"}); Row("Delivery Method:", cbo_deliverymethod);
            dtp_estimateddate = new DateTimePicker { Size = new System.Drawing.Size(fw,23), Format=DateTimePickerFormat.Short }; Row("Est. Delivery:", dtp_estimateddate);
            txt_versionno = new TextBox { Size = new System.Drawing.Size(fw,23) }; Row("Version No:", txt_versionno);
            var btnNew=Btn("New",px,py,System.Drawing.Color.ForestGreen); var btnSave=Btn("Save",px+90,py,System.Drawing.Color.RoyalBlue); var btnDel=Btn("Delete",px+180,py,System.Drawing.Color.Crimson);
            btnNew.Click+=(s,e)=>New_(); btnSave.Click+=(s,e)=>Save_(); btnDel.Click+=(s,e)=>Del_();
            Controls.AddRange(new Control[]{btnNew,btnSave,btnDel}); Reload(filterID);
        }
        private Button Btn(string t,int x,int y,System.Drawing.Color c)=>new Button{Text=t,Location=new System.Drawing.Point(x,y),Size=new System.Drawing.Size(80,30),BackColor=c,ForeColor=System.Drawing.Color.White,FlatStyle=FlatStyle.Flat};
        private void Reload(string s=""){
            try{using var cn=DBConnection.GetConnection();cn.Open();
                string sql=string.IsNullOrEmpty(s)?"SELECT * FROM quotations ORDER BY quotationID DESC LIMIT 300":"SELECT * FROM quotations WHERE quotationID LIKE @s LIMIT 300";
                using var cmd=new MySqlCommand(sql,cn); if(!string.IsNullOrEmpty(s))cmd.Parameters.AddWithValue("@s",$"%{s}%");
                var dt=new DataTable();new MySqlDataAdapter(cmd).Fill(dt);dgv.DataSource=dt;
            }catch(Exception ex){MessageBox.Show("Load error: "+ex.Message);}}
        private void Sel(object? s,EventArgs e){
            if(dgv.SelectedRows.Count==0)return;
            var row=dgv.SelectedRows[0].DataBoundItem as DataRowView;if(row==null)return;
            selID=row["quotationID"]?.ToString()??"";
            try{using var cn=DBConnection.GetConnection();cn.Open();
                using var cmd=new MySqlCommand("SELECT * FROM quotations WHERE quotationID=@id",cn);
                cmd.Parameters.AddWithValue("@id",selID);
                using var r=cmd.ExecuteReader();
                if(r.Read()){txtID.Text=r["quotationID"]?.ToString()??"";
                    txt_customerID.Text=r["customerID"]?.ToString()??"";
                    if(r["issuedate"]!=DBNull.Value)dtp_issuedate.Value=Convert.ToDateTime(r["issuedate"]);
                    if(r["validuntil"]!=DBNull.Value)dtp_validuntil.Value=Convert.ToDateTime(r["validuntil"]);
                    cbo_status.Text=r["status"]?.ToString()??"";
                    txt_paymentmethod.Text=r["paymentmethod"]?.ToString()??"";
                    cbo_deliverymethod.Text=r["deliverymethod"]?.ToString()??"";
                    if(r["estimateddate"]!=DBNull.Value)dtp_estimateddate.Value=Convert.ToDateTime(r["estimateddate"]);
                    txt_versionno.Text=r["versionno"]?.ToString()??"";
                }}catch{}
        }
        private void New_(){selID="QT-"+DateTime.Now.ToString("yyyyMMddHHmmss");txtID.Text=selID;
            txt_customerID.Text=txt_paymentmethod.Text=txt_versionno.Text="";
            dtp_issuedate.Value=dtp_validuntil.Value=dtp_estimateddate.Value=DateTime.Today;
            cbo_status.Text="Draft";cbo_deliverymethod.Text="";}
        private void Save_(){
            if(string.IsNullOrEmpty(txtID.Text)){MessageBox.Show("Click New first.");return;}
            try{using var cn=DBConnection.GetConnection();cn.Open();
                bool ex;using(var chk=new MySqlCommand("SELECT COUNT(*) FROM quotations WHERE quotationID=@id",cn)){chk.Parameters.AddWithValue("@id",txtID.Text);ex=(long)chk.ExecuteScalar()>0;}
                string sql=ex?"UPDATE quotations SET customerID=@c,issuedate=@id2,validuntil=@vu,status=@s,paymentmethod=@pm,deliverymethod=@dm,estimateddate=@ed,versionno=@vn WHERE quotationID=@id"
                             :"INSERT INTO quotations(quotationID,customerID,issuedate,validuntil,status,paymentmethod,deliverymethod,estimateddate,versionno)VALUES(@id,@c,@id2,@vu,@s,@pm,@dm,@ed,@vn)";
                using var cmd=new MySqlCommand(sql,cn);
                cmd.Parameters.AddWithValue("@id",txtID.Text);cmd.Parameters.AddWithValue("@c",txt_customerID.Text);
                cmd.Parameters.AddWithValue("@id2",dtp_issuedate.Value.Date);cmd.Parameters.AddWithValue("@vu",dtp_validuntil.Value.Date);
                cmd.Parameters.AddWithValue("@s",cbo_status.Text);cmd.Parameters.AddWithValue("@pm",txt_paymentmethod.Text);
                cmd.Parameters.AddWithValue("@dm",cbo_deliverymethod.Text);cmd.Parameters.AddWithValue("@ed",dtp_estimateddate.Value.Date);
                int vn;int.TryParse(txt_versionno.Text,out vn);cmd.Parameters.AddWithValue("@vn",vn==0?1:vn);
                cmd.ExecuteNonQuery();
                Reload();MessageBox.Show("Saved!","OK",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }catch(Exception ex2){MessageBox.Show("Save error: "+ex2.Message);}}
        private void Del_(){
            if(string.IsNullOrEmpty(selID)){MessageBox.Show("Select a record.");return;}
            if(MessageBox.Show($"Delete {selID}?","Confirm",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
            try{using var cn=DBConnection.GetConnection();cn.Open();
                using var cmd=new MySqlCommand("DELETE FROM quotations WHERE quotationID=@id",cn);
                cmd.Parameters.AddWithValue("@id",selID);cmd.ExecuteNonQuery();selID="";Reload();
            }catch(Exception ex){MessageBox.Show("Delete error: "+ex.Message);}}
    }
}
