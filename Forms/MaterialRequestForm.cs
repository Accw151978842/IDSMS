using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    public class MaterialRequestForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSrch,txtID,txt_prodID,txt_deliverylocation;
        private ComboBox cbo_urgency,cbo_priority,cbo_status;
        private DateTimePicker dtp_requireddate;
        private string selID="";

        public MaterialRequestForm():this(""){ }
        public MaterialRequestForm(string filterID)
        {
            Text="Material Request Management";Size=new System.Drawing.Size(1000,580);StartPosition=FormStartPosition.CenterParent;
            txtSrch=new TextBox{Location=new System.Drawing.Point(70,7),Size=new System.Drawing.Size(200,23)};
            Controls.Add(new Label{Text="Search:",Location=new System.Drawing.Point(10,10),Size=new System.Drawing.Size(55,23)});Controls.Add(txtSrch);
            var btnSrch=new Button{Text="Search",Location=new System.Drawing.Point(280,5),Size=new System.Drawing.Size(80,27),BackColor=System.Drawing.Color.SteelBlue,ForeColor=System.Drawing.Color.White,FlatStyle=FlatStyle.Flat};
            btnSrch.Click+=(s,e)=>Reload(txtSrch.Text);Controls.Add(btnSrch);
            dgv=new DataGridView{Location=new System.Drawing.Point(10,42),Size=new System.Drawing.Size(600,440),ReadOnly=true,AllowUserToAddRows=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,BackgroundColor=System.Drawing.Color.White};
            dgv.SelectionChanged+=Sel;Controls.Add(dgv);
            int px=625,py=42,lw=140,fw=200;
            Controls.Add(new Label{Text="Material Request",Location=new System.Drawing.Point(px,py-22),Font=new System.Drawing.Font("Segoe UI",9,System.Drawing.FontStyle.Bold)});
            txtID=new TextBox{Size=new System.Drawing.Size(fw,23),ReadOnly=true};
            void Row(string lbl,Control ctrl){Controls.Add(new Label{Text=lbl,Location=new System.Drawing.Point(px,py),Size=new System.Drawing.Size(lw,23)});ctrl.Location=new System.Drawing.Point(px+lw+5,py);Controls.Add(ctrl);py+=33;}
            Row("MR ID:",txtID);
            txt_prodID=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Production Order:",txt_prodID);
            dtp_requireddate=new DateTimePicker{Size=new System.Drawing.Size(fw,23),Format=DateTimePickerFormat.Short};Row("Required Date:",dtp_requireddate);
            txt_deliverylocation=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Delivery Location:",txt_deliverylocation);
            cbo_urgency=new ComboBox{Size=new System.Drawing.Size(fw,23),DropDownStyle=ComboBoxStyle.DropDownList};
            cbo_urgency.Items.AddRange(new object[]{"Normal","Urgent","Critical"});Row("Urgency:",cbo_urgency);
            cbo_priority=new ComboBox{Size=new System.Drawing.Size(fw,23),DropDownStyle=ComboBoxStyle.DropDownList};
            cbo_priority.Items.AddRange(new object[]{"Low","Medium","High"});Row("Priority:",cbo_priority);
            cbo_status=new ComboBox{Size=new System.Drawing.Size(fw,23),DropDownStyle=ComboBoxStyle.DropDownList};
            cbo_status.Items.AddRange(new object[]{"Pending","Approved","Issued","Rejected"});Row("Status:",cbo_status);
            var btnNew=Btn("New",px,py,System.Drawing.Color.ForestGreen);var btnSave=Btn("Save",px+90,py,System.Drawing.Color.RoyalBlue);var btnDel=Btn("Delete",px+180,py,System.Drawing.Color.Crimson);
            btnNew.Click+=(s,e)=>New_();btnSave.Click+=(s,e)=>Save_();btnDel.Click+=(s,e)=>Del_();
            Controls.AddRange(new Control[]{btnNew,btnSave,btnDel});Reload(filterID);
        }
        private Button Btn(string t,int x,int y,System.Drawing.Color c)=>new Button{Text=t,Location=new System.Drawing.Point(x,y),Size=new System.Drawing.Size(80,30),BackColor=c,ForeColor=System.Drawing.Color.White,FlatStyle=FlatStyle.Flat};
        private void Reload(string s=""){
            try{using var cn=DBConnection.GetConnection();cn.Open();
                string sql=string.IsNullOrEmpty(s)?"SELECT * FROM materialrequests ORDER BY mrid DESC LIMIT 300":"SELECT * FROM materialrequests WHERE mrid LIKE @s OR prodID LIKE @s LIMIT 300";
                using var cmd=new MySqlCommand(sql,cn);if(!string.IsNullOrEmpty(s))cmd.Parameters.AddWithValue("@s",$"%{s}%");
                var dt=new DataTable();new MySqlDataAdapter(cmd).Fill(dt);dgv.DataSource=dt;
            }catch(Exception ex){MessageBox.Show("Load error: "+ex.Message);}}
        private void Sel(object? s,EventArgs e){
            if(dgv.SelectedRows.Count==0)return;
            var row=dgv.SelectedRows[0].DataBoundItem as DataRowView;if(row==null)return;
            selID=row["mrid"]?.ToString()??"";
            try{using var cn=DBConnection.GetConnection();cn.Open();
                using var cmd=new MySqlCommand("SELECT * FROM materialrequests WHERE mrid=@id",cn);
                cmd.Parameters.AddWithValue("@id",selID);
                using var r=cmd.ExecuteReader();
                if(r.Read()){txtID.Text=r["mrid"]?.ToString()??"";
                    txt_prodID.Text=r["prodID"]?.ToString()??"";
                    if(r["requireddate"]!=DBNull.Value)dtp_requireddate.Value=Convert.ToDateTime(r["requireddate"]);
                    txt_deliverylocation.Text=r["deliverylocation"]?.ToString()??"";
                    cbo_urgency.Text=r["urgency"]?.ToString()??"";
                    cbo_priority.Text=r["priority"]?.ToString()??"";
                    cbo_status.Text=r["status"]?.ToString()??"";
                }}catch{}
        }
        private void New_(){selID="MR-"+DateTime.Now.ToString("yyyyMMddHHmmss");txtID.Text=selID;
            txt_prodID.Text=txt_deliverylocation.Text="";dtp_requireddate.Value=DateTime.Today;
            cbo_urgency.Text="Normal";cbo_priority.Text="Medium";cbo_status.Text="Pending";}
        private void Save_(){
            if(string.IsNullOrEmpty(txtID.Text)){MessageBox.Show("Click New first.");return;}
            try{using var cn=DBConnection.GetConnection();cn.Open();
                bool ex;using(var chk=new MySqlCommand("SELECT COUNT(*) FROM materialrequests WHERE mrid=@id",cn)){chk.Parameters.AddWithValue("@id",txtID.Text);ex=(long)chk.ExecuteScalar()>0;}
                string sql=ex?"UPDATE materialrequests SET prodID=@p,requireddate=@rd,deliverylocation=@dl,urgency=@ug,priority=@pr,status=@ss WHERE mrid=@id"
                             :"INSERT INTO materialrequests(mrid,prodID,requireddate,deliverylocation,urgency,priority,status)VALUES(@id,@p,@rd,@dl,@ug,@pr,@ss)";
                using var cmd=new MySqlCommand(sql,cn);
                cmd.Parameters.AddWithValue("@id",txtID.Text);cmd.Parameters.AddWithValue("@p",txt_prodID.Text);
                cmd.Parameters.AddWithValue("@rd",dtp_requireddate.Value.Date);cmd.Parameters.AddWithValue("@dl",txt_deliverylocation.Text);
                cmd.Parameters.AddWithValue("@ug",cbo_urgency.Text);cmd.Parameters.AddWithValue("@pr",cbo_priority.Text);
                cmd.Parameters.AddWithValue("@ss",cbo_status.Text);cmd.ExecuteNonQuery();
                Reload();MessageBox.Show("Saved!","OK",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }catch(Exception ex2){MessageBox.Show("Save error: "+ex2.Message);}}
        private void Del_(){
            if(string.IsNullOrEmpty(selID)){MessageBox.Show("Select a record.");return;}
            if(MessageBox.Show($"Delete {selID}?","Confirm",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
            try{using var cn=DBConnection.GetConnection();cn.Open();
                using var cmd=new MySqlCommand("DELETE FROM materialrequests WHERE mrid=@id",cn);
                cmd.Parameters.AddWithValue("@id",selID);cmd.ExecuteNonQuery();selID="";Reload();
            }catch(Exception ex){MessageBox.Show("Delete error: "+ex.Message);}}
    }
}
