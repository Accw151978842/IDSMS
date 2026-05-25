using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    public class ProductionOrderForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSrch,txtID,txt_orderID,txt_itemID,txt_serialNo;
        private ComboBox cbo_step,cbo_priority,cbo_status;
        private DateTimePicker dtp_plannedstart,dtp_plannedend,dtp_eta;
        private string selID="";

        public ProductionOrderForm():this(""){ }
        public ProductionOrderForm(string filterID)
        {
            Text="Production Order Management";Size=new System.Drawing.Size(1000,620);StartPosition=FormStartPosition.CenterParent;
            txtSrch=new TextBox{Location=new System.Drawing.Point(70,7),Size=new System.Drawing.Size(200,23)};
            Controls.Add(new Label{Text="Search:",Location=new System.Drawing.Point(10,10),Size=new System.Drawing.Size(55,23)});Controls.Add(txtSrch);
            var btnSrch=new Button{Text="Search",Location=new System.Drawing.Point(280,5),Size=new System.Drawing.Size(80,27),BackColor=System.Drawing.Color.SteelBlue,ForeColor=System.Drawing.Color.White,FlatStyle=FlatStyle.Flat};
            btnSrch.Click+=(s,e)=>Reload(txtSrch.Text);Controls.Add(btnSrch);
            dgv=new DataGridView{Location=new System.Drawing.Point(10,42),Size=new System.Drawing.Size(600,470),ReadOnly=true,AllowUserToAddRows=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,BackgroundColor=System.Drawing.Color.White};
            dgv.SelectionChanged+=Sel;Controls.Add(dgv);
            int px=625,py=42,lw=140,fw=200;
            Controls.Add(new Label{Text="Production Order",Location=new System.Drawing.Point(px,py-22),Font=new System.Drawing.Font("Segoe UI",9,System.Drawing.FontStyle.Bold)});
            txtID=new TextBox{Size=new System.Drawing.Size(fw,23),ReadOnly=true};
            void Row(string lbl,Control ctrl){Controls.Add(new Label{Text=lbl,Location=new System.Drawing.Point(px,py),Size=new System.Drawing.Size(lw,23)});ctrl.Location=new System.Drawing.Point(px+lw+5,py);Controls.Add(ctrl);py+=31;}
            Row("Prod ID:",txtID);
            txt_orderID=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Order ID:",txt_orderID);
            txt_itemID=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Item ID:",txt_itemID);
            txt_serialNo=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Serial No:",txt_serialNo);
            cbo_step=new ComboBox{Size=new System.Drawing.Size(fw,23),DropDownStyle=ComboBoxStyle.DropDownList};
            cbo_step.Items.AddRange(new object[]{"Cutting","Assembly","Finishing","QC","Packing"});Row("Current Step:",cbo_step);
            dtp_plannedstart=new DateTimePicker{Size=new System.Drawing.Size(fw,23),Format=DateTimePickerFormat.Short};Row("Planned Start:",dtp_plannedstart);
            dtp_plannedend=new DateTimePicker{Size=new System.Drawing.Size(fw,23),Format=DateTimePickerFormat.Short};Row("Planned End:",dtp_plannedend);
            cbo_priority=new ComboBox{Size=new System.Drawing.Size(fw,23),DropDownStyle=ComboBoxStyle.DropDownList};
            cbo_priority.Items.AddRange(new object[]{"Low","Medium","High","Urgent"});Row("Priority:",cbo_priority);
            dtp_eta=new DateTimePicker{Size=new System.Drawing.Size(fw,23),Format=DateTimePickerFormat.Short};Row("ETA:",dtp_eta);
            cbo_status=new ComboBox{Size=new System.Drawing.Size(fw,23),DropDownStyle=ComboBoxStyle.DropDownList};
            cbo_status.Items.AddRange(new object[]{"Scheduled","InProgress","Completed","OnHold","Cancelled"});Row("Status:",cbo_status);
            var btnNew=Btn("New",px,py,System.Drawing.Color.ForestGreen);var btnSave=Btn("Save",px+90,py,System.Drawing.Color.RoyalBlue);var btnDel=Btn("Delete",px+180,py,System.Drawing.Color.Crimson);
            btnNew.Click+=(s,e)=>New_();btnSave.Click+=(s,e)=>Save_();btnDel.Click+=(s,e)=>Del_();
            Controls.AddRange(new Control[]{btnNew,btnSave,btnDel});Reload(filterID);
        }
        private Button Btn(string t,int x,int y,System.Drawing.Color c)=>new Button{Text=t,Location=new System.Drawing.Point(x,y),Size=new System.Drawing.Size(80,30),BackColor=c,ForeColor=System.Drawing.Color.White,FlatStyle=FlatStyle.Flat};
        private void Reload(string s=""){
            try{using var cn=DBConnection.GetConnection();cn.Open();
                string sql=string.IsNullOrEmpty(s)?"SELECT * FROM production ORDER BY prodID DESC LIMIT 300":"SELECT * FROM production WHERE prodID LIKE @s OR orderID LIKE @s LIMIT 300";
                using var cmd=new MySqlCommand(sql,cn);if(!string.IsNullOrEmpty(s))cmd.Parameters.AddWithValue("@s",$"%{s}%");
                var dt=new DataTable();new MySqlDataAdapter(cmd).Fill(dt);dgv.DataSource=dt;
            }catch(Exception ex){MessageBox.Show("Load error: "+ex.Message);}}
        private void Sel(object? s,EventArgs e){
            if(dgv.SelectedRows.Count==0)return;
            var row=dgv.SelectedRows[0].DataBoundItem as DataRowView;if(row==null)return;
            selID=row["prodID"]?.ToString()??"";
            try{using var cn=DBConnection.GetConnection();cn.Open();
                using var cmd=new MySqlCommand("SELECT * FROM production WHERE prodID=@id",cn);
                cmd.Parameters.AddWithValue("@id",selID);
                using var r=cmd.ExecuteReader();
                if(r.Read()){txtID.Text=r["prodID"]?.ToString()??"";
                    txt_orderID.Text=r["orderID"]?.ToString()??"";
                    txt_itemID.Text=r["itemID"]?.ToString()??"";
                    txt_serialNo.Text=r["serialNo"]?.ToString()??"";
                    cbo_step.Text=r["step"]?.ToString()??"";
                    if(r["plannedstart"]!=DBNull.Value)dtp_plannedstart.Value=Convert.ToDateTime(r["plannedstart"]);
                    if(r["plannedend"]!=DBNull.Value)dtp_plannedend.Value=Convert.ToDateTime(r["plannedend"]);
                    cbo_priority.Text=r["priority"]?.ToString()??"";
                    if(r["eta"]!=DBNull.Value)dtp_eta.Value=Convert.ToDateTime(r["eta"]);
                    cbo_status.Text=r["status"]?.ToString()??"";
                }}catch{}
        }
        private void New_(){selID="PRD-"+DateTime.Now.ToString("yyyyMMddHHmmss");txtID.Text=selID;
            txt_orderID.Text=txt_itemID.Text=txt_serialNo.Text="";
            dtp_plannedstart.Value=dtp_plannedend.Value=dtp_eta.Value=DateTime.Today;
            cbo_step.Text="Cutting";cbo_priority.Text="Medium";cbo_status.Text="Scheduled";}
        private void Save_(){
            if(string.IsNullOrEmpty(txtID.Text)){MessageBox.Show("Click New first.");return;}
            try{using var cn=DBConnection.GetConnection();cn.Open();
                bool ex;using(var chk=new MySqlCommand("SELECT COUNT(*) FROM production WHERE prodID=@id",cn)){chk.Parameters.AddWithValue("@id",txtID.Text);ex=(long)chk.ExecuteScalar()>0;}
                string sql=ex?"UPDATE production SET orderID=@o,itemID=@i,serialNo=@sn,step=@st,plannedstart=@ps,plannedend=@pe,priority=@pr,eta=@et,status=@ss WHERE prodID=@id"
                             :"INSERT INTO production(prodID,orderID,itemID,serialNo,step,plannedstart,plannedend,priority,eta,status)VALUES(@id,@o,@i,@sn,@st,@ps,@pe,@pr,@et,@ss)";
                using var cmd=new MySqlCommand(sql,cn);
                cmd.Parameters.AddWithValue("@id",txtID.Text);cmd.Parameters.AddWithValue("@o",txt_orderID.Text);
                cmd.Parameters.AddWithValue("@i",txt_itemID.Text);cmd.Parameters.AddWithValue("@sn",txt_serialNo.Text);
                cmd.Parameters.AddWithValue("@st",cbo_step.Text);cmd.Parameters.AddWithValue("@ps",dtp_plannedstart.Value.Date);
                cmd.Parameters.AddWithValue("@pe",dtp_plannedend.Value.Date);cmd.Parameters.AddWithValue("@pr",cbo_priority.Text);
                cmd.Parameters.AddWithValue("@et",dtp_eta.Value.Date);cmd.Parameters.AddWithValue("@ss",cbo_status.Text);
                cmd.ExecuteNonQuery();
                Reload();MessageBox.Show("Saved!","OK",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }catch(Exception ex2){MessageBox.Show("Save error: "+ex2.Message);}}
        private void Del_(){
            if(string.IsNullOrEmpty(selID)){MessageBox.Show("Select a record.");return;}
            if(MessageBox.Show($"Delete {selID}?","Confirm",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
            try{using var cn=DBConnection.GetConnection();cn.Open();
                using var cmd=new MySqlCommand("DELETE FROM production WHERE prodID=@id",cn);
                cmd.Parameters.AddWithValue("@id",selID);cmd.ExecuteNonQuery();selID="";Reload();
            }catch(Exception ex){MessageBox.Show("Delete error: "+ex.Message);}}
    }
}
