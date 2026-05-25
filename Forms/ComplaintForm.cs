using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    public class ComplaintForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSrch,txtID,txt_customerID,txt_orderID,txt_serialNo,txt_description,txt_handledby,txt_resolution;
        private ComboBox cbo_category,cbo_priority,cbo_status;
        private string selID="";

        public ComplaintForm():this(""){ }
        public ComplaintForm(string filterID)
        {
            Text="Complaint Management";Size=new System.Drawing.Size(1000,660);StartPosition=FormStartPosition.CenterParent;
            txtSrch=new TextBox{Location=new System.Drawing.Point(70,7),Size=new System.Drawing.Size(200,23)};
            Controls.Add(new Label{Text="Search:",Location=new System.Drawing.Point(10,10),Size=new System.Drawing.Size(55,23)});Controls.Add(txtSrch);
            var btnSrch=new Button{Text="Search",Location=new System.Drawing.Point(280,5),Size=new System.Drawing.Size(80,27),BackColor=System.Drawing.Color.SteelBlue,ForeColor=System.Drawing.Color.White,FlatStyle=FlatStyle.Flat};
            btnSrch.Click+=(s,e)=>Reload(txtSrch.Text);Controls.Add(btnSrch);
            dgv=new DataGridView{Location=new System.Drawing.Point(10,42),Size=new System.Drawing.Size(600,510),ReadOnly=true,AllowUserToAddRows=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,BackgroundColor=System.Drawing.Color.White};
            dgv.SelectionChanged+=Sel;Controls.Add(dgv);
            int px=625,py=42,lw=140,fw=200;
            Controls.Add(new Label{Text="Complaint",Location=new System.Drawing.Point(px,py-22),Font=new System.Drawing.Font("Segoe UI",9,System.Drawing.FontStyle.Bold)});
            txtID=new TextBox{Size=new System.Drawing.Size(fw,23),ReadOnly=true};
            void Row(string lbl,Control ctrl){Controls.Add(new Label{Text=lbl,Location=new System.Drawing.Point(px,py),Size=new System.Drawing.Size(lw,23)});ctrl.Location=new System.Drawing.Point(px+lw+5,py);Controls.Add(ctrl);py+=31;}
            Row("Complaint ID:",txtID);
            txt_customerID=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Customer ID:",txt_customerID);
            txt_orderID=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Order ID:",txt_orderID);
            txt_serialNo=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Serial No:",txt_serialNo);
            cbo_category=new ComboBox{Size=new System.Drawing.Size(fw,23),DropDownStyle=ComboBoxStyle.DropDownList};
            cbo_category.Items.AddRange(new object[]{"Damage","WrongItem","LateDelivery","QualityIssue","Other"});Row("Category:",cbo_category);
            txt_description=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Description:",txt_description);
            cbo_priority=new ComboBox{Size=new System.Drawing.Size(fw,23),DropDownStyle=ComboBoxStyle.DropDownList};
            cbo_priority.Items.AddRange(new object[]{"Low","Medium","High","Critical"});Row("Priority:",cbo_priority);
            cbo_status=new ComboBox{Size=new System.Drawing.Size(fw,23),DropDownStyle=ComboBoxStyle.DropDownList};
            cbo_status.Items.AddRange(new object[]{"Open","InProgress","Resolved","Closed"});Row("Status:",cbo_status);
            txt_handledby=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Handled By:",txt_handledby);
            txt_resolution=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Resolution:",txt_resolution);
            var btnNew=Btn("New",px,py,System.Drawing.Color.ForestGreen);var btnSave=Btn("Save",px+90,py,System.Drawing.Color.RoyalBlue);var btnDel=Btn("Delete",px+180,py,System.Drawing.Color.Crimson);
            btnNew.Click+=(s,e)=>New_();btnSave.Click+=(s,e)=>Save_();btnDel.Click+=(s,e)=>Del_();
            Controls.AddRange(new Control[]{btnNew,btnSave,btnDel});Reload(filterID);
        }
        private Button Btn(string t,int x,int y,System.Drawing.Color c)=>new Button{Text=t,Location=new System.Drawing.Point(x,y),Size=new System.Drawing.Size(80,30),BackColor=c,ForeColor=System.Drawing.Color.White,FlatStyle=FlatStyle.Flat};
        private void Reload(string s=""){
            try{using var cn=DBConnection.GetConnection();cn.Open();
                string sql=string.IsNullOrEmpty(s)?"SELECT * FROM complaints ORDER BY complaintID DESC LIMIT 300":"SELECT * FROM complaints WHERE complaintID LIKE @s OR customerID LIKE @s LIMIT 300";
                using var cmd=new MySqlCommand(sql,cn);if(!string.IsNullOrEmpty(s))cmd.Parameters.AddWithValue("@s",$"%{s}%");
                var dt=new DataTable();new MySqlDataAdapter(cmd).Fill(dt);dgv.DataSource=dt;
            }catch(Exception ex){MessageBox.Show("Load error: "+ex.Message);}}
        private void Sel(object? s,EventArgs e){
            if(dgv.SelectedRows.Count==0)return;
            var row=dgv.SelectedRows[0].DataBoundItem as DataRowView;if(row==null)return;
            selID=row["complaintID"]?.ToString()??"";
            try{using var cn=DBConnection.GetConnection();cn.Open();
                using var cmd=new MySqlCommand("SELECT * FROM complaints WHERE complaintID=@id",cn);
                cmd.Parameters.AddWithValue("@id",selID);
                using var r=cmd.ExecuteReader();
                if(r.Read()){txtID.Text=r["complaintID"]?.ToString()??"";
                    txt_customerID.Text=r["customerID"]?.ToString()??"";
                    txt_orderID.Text=r["orderID"]?.ToString()??"";
                    txt_serialNo.Text=r["serialNo"]?.ToString()??"";
                    cbo_category.Text=r["category"]?.ToString()??"";
                    txt_description.Text=r["description"]?.ToString()??"";
                    cbo_priority.Text=r["priority"]?.ToString()??"";
                    cbo_status.Text=r["status"]?.ToString()??"";
                    txt_handledby.Text=r["handledby"]?.ToString()??"";
                    txt_resolution.Text=r["resolution"]?.ToString()??"";
                }}catch{}
        }
        private void New_(){selID="CMP-"+DateTime.Now.ToString("yyyyMMddHHmmss");txtID.Text=selID;
            txt_customerID.Text=txt_orderID.Text=txt_serialNo.Text=txt_description.Text=txt_handledby.Text=txt_resolution.Text="";
            cbo_category.Text="Other";cbo_priority.Text="Medium";cbo_status.Text="Open";}
        private void Save_(){
            if(string.IsNullOrEmpty(txtID.Text)){MessageBox.Show("Click New first.");return;}
            try{using var cn=DBConnection.GetConnection();cn.Open();
                bool ex;using(var chk=new MySqlCommand("SELECT COUNT(*) FROM complaints WHERE complaintID=@id",cn)){chk.Parameters.AddWithValue("@id",txtID.Text);ex=(long)chk.ExecuteScalar()>0;}
                string sql=ex?"UPDATE complaints SET customerID=@c,orderID=@o,serialNo=@sn,category=@ca,description=@de,priority=@pr,status=@ss,handledby=@hb,resolution=@re WHERE complaintID=@id"
                             :"INSERT INTO complaints(complaintID,customerID,orderID,serialNo,category,description,priority,status,handledby,resolution)VALUES(@id,@c,@o,@sn,@ca,@de,@pr,@ss,@hb,@re)";
                using var cmd=new MySqlCommand(sql,cn);
                cmd.Parameters.AddWithValue("@id",txtID.Text);cmd.Parameters.AddWithValue("@c",txt_customerID.Text);
                cmd.Parameters.AddWithValue("@o",txt_orderID.Text);cmd.Parameters.AddWithValue("@sn",txt_serialNo.Text);
                cmd.Parameters.AddWithValue("@ca",cbo_category.Text);cmd.Parameters.AddWithValue("@de",txt_description.Text);
                cmd.Parameters.AddWithValue("@pr",cbo_priority.Text);cmd.Parameters.AddWithValue("@ss",cbo_status.Text);
                cmd.Parameters.AddWithValue("@hb",txt_handledby.Text);cmd.Parameters.AddWithValue("@re",txt_resolution.Text);
                cmd.ExecuteNonQuery();
                Reload();MessageBox.Show("Saved!","OK",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }catch(Exception ex2){MessageBox.Show("Save error: "+ex2.Message);}}
        private void Del_(){
            if(string.IsNullOrEmpty(selID)){MessageBox.Show("Select a record.");return;}
            if(MessageBox.Show($"Delete {selID}?","Confirm",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
            try{using var cn=DBConnection.GetConnection();cn.Open();
                using var cmd=new MySqlCommand("DELETE FROM complaints WHERE complaintID=@id",cn);
                cmd.Parameters.AddWithValue("@id",selID);cmd.ExecuteNonQuery();selID="";Reload();
            }catch(Exception ex){MessageBox.Show("Delete error: "+ex.Message);}}
    }
}
