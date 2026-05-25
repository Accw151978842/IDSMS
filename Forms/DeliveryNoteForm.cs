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
        private TextBox txtSrch,txtID,txt_orderID,txt_customerID,txt_shippingaddress,txt_driver,txt_signedby;
        private ComboBox cbo_deliverymethod,cbo_status;
        private DateTimePicker dtp_dndate,dtp_dispatchdate;
        private string selID="";

        public DeliveryNoteForm():this(""){ }
        public DeliveryNoteForm(string filterID)
        {
            Text="Delivery Note Management";Size=new System.Drawing.Size(1000,640);StartPosition=FormStartPosition.CenterParent;
            txtSrch=new TextBox{Location=new System.Drawing.Point(70,7),Size=new System.Drawing.Size(200,23)};
            Controls.Add(new Label{Text="Search:",Location=new System.Drawing.Point(10,10),Size=new System.Drawing.Size(55,23)});Controls.Add(txtSrch);
            var btnSrch=new Button{Text="Search",Location=new System.Drawing.Point(280,5),Size=new System.Drawing.Size(80,27),BackColor=System.Drawing.Color.SteelBlue,ForeColor=System.Drawing.Color.White,FlatStyle=FlatStyle.Flat};
            btnSrch.Click+=(s,e)=>Reload(txtSrch.Text);Controls.Add(btnSrch);
            dgv=new DataGridView{Location=new System.Drawing.Point(10,42),Size=new System.Drawing.Size(600,490),ReadOnly=true,AllowUserToAddRows=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,BackgroundColor=System.Drawing.Color.White};
            dgv.SelectionChanged+=Sel;Controls.Add(dgv);
            int px=625,py=42,lw=140,fw=200;
            Controls.Add(new Label{Text="Delivery Note",Location=new System.Drawing.Point(px,py-22),Font=new System.Drawing.Font("Segoe UI",9,System.Drawing.FontStyle.Bold)});
            txtID=new TextBox{Size=new System.Drawing.Size(fw,23),ReadOnly=true};
            void Row(string lbl,Control ctrl){Controls.Add(new Label{Text=lbl,Location=new System.Drawing.Point(px,py),Size=new System.Drawing.Size(lw,23)});ctrl.Location=new System.Drawing.Point(px+lw+5,py);Controls.Add(ctrl);py+=31;}
            Row("DN ID:",txtID);
            txt_orderID=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Order ID:",txt_orderID);
            txt_customerID=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Customer ID:",txt_customerID);
            dtp_dndate=new DateTimePicker{Size=new System.Drawing.Size(fw,23),Format=DateTimePickerFormat.Short};Row("DN Date:",dtp_dndate);
            dtp_dispatchdate=new DateTimePicker{Size=new System.Drawing.Size(fw,23),Format=DateTimePickerFormat.Short};Row("Dispatch Date:",dtp_dispatchdate);
            cbo_deliverymethod=new ComboBox{Size=new System.Drawing.Size(fw,23),DropDownStyle=ComboBoxStyle.DropDownList};
            cbo_deliverymethod.Items.AddRange(new object[]{"Courier","Truck","Self-collect"});Row("Delivery Method:",cbo_deliverymethod);
            txt_shippingaddress=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Shipping Address:",txt_shippingaddress);
            txt_driver=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Driver:",txt_driver);
            txt_signedby=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Signed By:",txt_signedby);
            cbo_status=new ComboBox{Size=new System.Drawing.Size(fw,23),DropDownStyle=ComboBoxStyle.DropDownList};
            cbo_status.Items.AddRange(new object[]{"Prepared","Dispatched","Delivered","Confirmed"});Row("Status:",cbo_status);
            var btnNew=Btn("New",px,py,System.Drawing.Color.ForestGreen);var btnSave=Btn("Save",px+90,py,System.Drawing.Color.RoyalBlue);var btnDel=Btn("Delete",px+180,py,System.Drawing.Color.Crimson);
            btnNew.Click+=(s,e)=>New_();btnSave.Click+=(s,e)=>Save_();btnDel.Click+=(s,e)=>Del_();
            Controls.AddRange(new Control[]{btnNew,btnSave,btnDel});Reload(filterID);
        }
        private Button Btn(string t,int x,int y,System.Drawing.Color c)=>new Button{Text=t,Location=new System.Drawing.Point(x,y),Size=new System.Drawing.Size(80,30),BackColor=c,ForeColor=System.Drawing.Color.White,FlatStyle=FlatStyle.Flat};
        private void Reload(string s=""){
            try{using var cn=DBConnection.GetConnection();cn.Open();
                string sql=string.IsNullOrEmpty(s)?"SELECT * FROM deliverynotes ORDER BY dnID DESC LIMIT 300":"SELECT * FROM deliverynotes WHERE dnID LIKE @s OR orderID LIKE @s LIMIT 300";
                using var cmd=new MySqlCommand(sql,cn);if(!string.IsNullOrEmpty(s))cmd.Parameters.AddWithValue("@s",$"%{s}%");
                var dt=new DataTable();new MySqlDataAdapter(cmd).Fill(dt);dgv.DataSource=dt;
            }catch(Exception ex){MessageBox.Show("Load error: "+ex.Message);}}
        private void Sel(object? s,EventArgs e){
            if(dgv.SelectedRows.Count==0)return;
            var row=dgv.SelectedRows[0].DataBoundItem as DataRowView;if(row==null)return;
            selID=row["dnID"]?.ToString()??"";
            try{using var cn=DBConnection.GetConnection();cn.Open();
                using var cmd=new MySqlCommand("SELECT * FROM deliverynotes WHERE dnID=@id",cn);
                cmd.Parameters.AddWithValue("@id",selID);
                using var r=cmd.ExecuteReader();
                if(r.Read()){txtID.Text=r["dnID"]?.ToString()??"";
                    txt_orderID.Text=r["orderID"]?.ToString()??"";
                    txt_customerID.Text=r["customerID"]?.ToString()??"";
                    if(r["dndate"]!=DBNull.Value)dtp_dndate.Value=Convert.ToDateTime(r["dndate"]);
                    if(r["dispatchdate"]!=DBNull.Value)dtp_dispatchdate.Value=Convert.ToDateTime(r["dispatchdate"]);
                    cbo_deliverymethod.Text=r["deliverymethod"]?.ToString()??"";
                    txt_shippingaddress.Text=r["shippingaddress"]?.ToString()??"";
                    txt_driver.Text=r["driver"]?.ToString()??"";
                    txt_signedby.Text=r["signedby"]?.ToString()??"";
                    cbo_status.Text=r["status"]?.ToString()??"";
                }}catch{}
        }
        private void New_(){selID="DN-"+DateTime.Now.ToString("yyyyMMddHHmmss");txtID.Text=selID;
            txt_orderID.Text=txt_customerID.Text=txt_shippingaddress.Text=txt_driver.Text=txt_signedby.Text="";
            dtp_dndate.Value=dtp_dispatchdate.Value=DateTime.Today;
            cbo_deliverymethod.Text="";cbo_status.Text="Prepared";}
        private void Save_(){
            if(string.IsNullOrEmpty(txtID.Text)){MessageBox.Show("Click New first.");return;}
            try{using var cn=DBConnection.GetConnection();cn.Open();
                bool ex;using(var chk=new MySqlCommand("SELECT COUNT(*) FROM deliverynotes WHERE dnID=@id",cn)){chk.Parameters.AddWithValue("@id",txtID.Text);ex=(long)chk.ExecuteScalar()>0;}
                string sql=ex?"UPDATE deliverynotes SET orderID=@o,customerID=@c,dndate=@dd,dispatchdate=@dp,deliverymethod=@dm,shippingaddress=@sa,driver=@dr,signedby=@sb,status=@ss WHERE dnID=@id"
                             :"INSERT INTO deliverynotes(dnID,orderID,customerID,dndate,dispatchdate,deliverymethod,shippingaddress,driver,signedby,status)VALUES(@id,@o,@c,@dd,@dp,@dm,@sa,@dr,@sb,@ss)";
                using var cmd=new MySqlCommand(sql,cn);
                cmd.Parameters.AddWithValue("@id",txtID.Text);cmd.Parameters.AddWithValue("@o",txt_orderID.Text);
                cmd.Parameters.AddWithValue("@c",txt_customerID.Text);cmd.Parameters.AddWithValue("@dd",dtp_dndate.Value.Date);
                cmd.Parameters.AddWithValue("@dp",dtp_dispatchdate.Value.Date);cmd.Parameters.AddWithValue("@dm",cbo_deliverymethod.Text);
                cmd.Parameters.AddWithValue("@sa",txt_shippingaddress.Text);cmd.Parameters.AddWithValue("@dr",txt_driver.Text);
                cmd.Parameters.AddWithValue("@sb",txt_signedby.Text);cmd.Parameters.AddWithValue("@ss",cbo_status.Text);
                cmd.ExecuteNonQuery();
                Reload();MessageBox.Show("Saved!","OK",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }catch(Exception ex2){MessageBox.Show("Save error: "+ex2.Message);}}
        private void Del_(){
            if(string.IsNullOrEmpty(selID)){MessageBox.Show("Select a record.");return;}
            if(MessageBox.Show($"Delete {selID}?","Confirm",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
            try{using var cn=DBConnection.GetConnection();cn.Open();
                using var cmd=new MySqlCommand("DELETE FROM deliverynotes WHERE dnID=@id",cn);
                cmd.Parameters.AddWithValue("@id",selID);cmd.ExecuteNonQuery();selID="";Reload();
            }catch(Exception ex){MessageBox.Show("Delete error: "+ex.Message);}}
    }
}
