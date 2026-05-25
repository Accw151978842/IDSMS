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
        private TextBox txtSrch,txtID,txt_orderID,txt_customerID,txt_billingaddress,txt_subtotal,txt_othercharges,txt_total,txt_othercomments;
        private ComboBox cbo_status;
        private DateTimePicker dtp_duedate;
        private string selID="";

        public InvoiceForm():this(""){}
        public InvoiceForm(string filterOrderID)
        {
            Text="Invoice Management";Size=new System.Drawing.Size(1000,600);StartPosition=FormStartPosition.CenterParent;
            txtSrch=new TextBox{Location=new System.Drawing.Point(70,7),Size=new System.Drawing.Size(200,23)};
            Controls.Add(new Label{Text="Search:",Location=new System.Drawing.Point(10,10),Size=new System.Drawing.Size(55,23)});Controls.Add(txtSrch);
            var btnSrch=new Button{Text="Search",Location=new System.Drawing.Point(280,5),Size=new System.Drawing.Size(80,27),BackColor=System.Drawing.Color.SteelBlue,ForeColor=System.Drawing.Color.White,FlatStyle=FlatStyle.Flat};
            btnSrch.Click+=(s,e)=>Reload(txtSrch.Text);Controls.Add(btnSrch);
            dgv=new DataGridView{Location=new System.Drawing.Point(10,42),Size=new System.Drawing.Size(600,460),ReadOnly=true,AllowUserToAddRows=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,BackgroundColor=System.Drawing.Color.White};
            dgv.SelectionChanged+=Sel;Controls.Add(dgv);
            int px=625,py=42,lw=140,fw=200;
            Controls.Add(new Label{Text="Invoice Details",Location=new System.Drawing.Point(px,py-22),Font=new System.Drawing.Font("Segoe UI",9,System.Drawing.FontStyle.Bold)});
            txtID=new TextBox{Size=new System.Drawing.Size(fw,23),ReadOnly=true};
            void Row(string lbl,Control ctrl){Controls.Add(new Label{Text=lbl,Location=new System.Drawing.Point(px,py),Size=new System.Drawing.Size(lw,23)});ctrl.Location=new System.Drawing.Point(px+lw+5,py);Controls.Add(ctrl);py+=33;}
            Row("Invoice ID:",txtID);
            txt_orderID=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Order ID:",txt_orderID);
            txt_customerID=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Customer ID:",txt_customerID);
            txt_billingaddress=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Billing Address:",txt_billingaddress);
            txt_subtotal=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Subtotal:",txt_subtotal);
            txt_othercharges=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Other Charges:",txt_othercharges);
            txt_total=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Total:",txt_total);
            dtp_duedate=new DateTimePicker{Size=new System.Drawing.Size(fw,23),Format=DateTimePickerFormat.Short};Row("Due Date:",dtp_duedate);
            cbo_status=new ComboBox{Size=new System.Drawing.Size(fw,23),DropDownStyle=ComboBoxStyle.DropDownList};
            cbo_status.Items.AddRange(new object[]{"Unpaid","Partial","Paid","Overdue"});Row("Status:",cbo_status);
            txt_othercomments=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Comments:",txt_othercomments);
            var btnNew=Btn("New",px,py,System.Drawing.Color.ForestGreen);var btnSave=Btn("Save",px+90,py,System.Drawing.Color.RoyalBlue);var btnDel=Btn("Delete",px+180,py,System.Drawing.Color.Crimson);
            btnNew.Click+=(s,e)=>New_(filterOrderID);btnSave.Click+=(s,e)=>Save_();btnDel.Click+=(s,e)=>Del_();
            Controls.AddRange(new Control[]{btnNew,btnSave,btnDel});Reload(filterOrderID);
        }
        private Button Btn(string t,int x,int y,System.Drawing.Color c)=>new Button{Text=t,Location=new System.Drawing.Point(x,y),Size=new System.Drawing.Size(80,30),BackColor=c,ForeColor=System.Drawing.Color.White,FlatStyle=FlatStyle.Flat};
        private void Reload(string s=""){
            try{using var cn=DBConnection.GetConnection();cn.Open();
                string sql=string.IsNullOrEmpty(s)?"SELECT * FROM invoices ORDER BY invoiceID DESC LIMIT 300":"SELECT * FROM invoices WHERE orderID=@s OR invoiceID LIKE @s2 LIMIT 300";
                using var cmd=new MySqlCommand(sql,cn);
                if(!string.IsNullOrEmpty(s)){cmd.Parameters.AddWithValue("@s",s);cmd.Parameters.AddWithValue("@s2",$"%{s}%");}
                var dt=new DataTable();new MySqlDataAdapter(cmd).Fill(dt);dgv.DataSource=dt;
            }catch(Exception ex){MessageBox.Show("Load error: "+ex.Message);}}
        private void Sel(object? s,EventArgs e){
            if(dgv.SelectedRows.Count==0)return;
            var row=dgv.SelectedRows[0].DataBoundItem as DataRowView;if(row==null)return;
            selID=row["invoiceID"]?.ToString()??"";
            try{using var cn=DBConnection.GetConnection();cn.Open();
                using var cmd=new MySqlCommand("SELECT * FROM invoices WHERE invoiceID=@id",cn);
                cmd.Parameters.AddWithValue("@id",selID);
                using var r=cmd.ExecuteReader();
                if(r.Read()){txtID.Text=r["invoiceID"]?.ToString()??"";
                    txt_orderID.Text=r["orderID"]?.ToString()??"";
                    txt_customerID.Text=r["customerID"]?.ToString()??"";
                    txt_billingaddress.Text=r["billingaddress"]?.ToString()??"";
                    txt_subtotal.Text=r["subtotal"]?.ToString()??"";
                    txt_othercharges.Text=r["othercharges"]?.ToString()??"";
                    txt_total.Text=r["total"]?.ToString()??"";
                    if(r["duedate"]!=DBNull.Value)dtp_duedate.Value=Convert.ToDateTime(r["duedate"]);
                    cbo_status.Text=r["status"]?.ToString()??"";
                    txt_othercomments.Text=r["othercomments"]?.ToString()??"";
                }}catch{}
        }
        private void New_(string oid=""){selID="INV-"+DateTime.Now.ToString("yyyyMMddHHmmss");txtID.Text=selID;
            txt_orderID.Text=oid;txt_customerID.Text=txt_billingaddress.Text=txt_subtotal.Text=txt_othercharges.Text=txt_total.Text=txt_othercomments.Text="";
            dtp_duedate.Value=DateTime.Today.AddDays(30);cbo_status.Text="Unpaid";}
        private void Save_(){
            if(string.IsNullOrEmpty(txtID.Text)){MessageBox.Show("Click New first.");return;}
            try{using var cn=DBConnection.GetConnection();cn.Open();
                bool ex;using(var chk=new MySqlCommand("SELECT COUNT(*) FROM invoices WHERE invoiceID=@id",cn)){chk.Parameters.AddWithValue("@id",txtID.Text);ex=(long)chk.ExecuteScalar()>0;}
                string sql=ex?"UPDATE invoices SET orderID=@o,customerID=@c,billingaddress=@ba,subtotal=@st,othercharges=@oc,total=@tot,duedate=@dd,status=@s,othercomments=@cm WHERE invoiceID=@id"
                             :"INSERT INTO invoices(invoiceID,orderID,customerID,billingaddress,subtotal,othercharges,total,duedate,status,othercomments)VALUES(@id,@o,@c,@ba,@st,@oc,@tot,@dd,@s,@cm)";
                using var cmd=new MySqlCommand(sql,cn);
                cmd.Parameters.AddWithValue("@id",txtID.Text);cmd.Parameters.AddWithValue("@o",txt_orderID.Text);
                cmd.Parameters.AddWithValue("@c",txt_customerID.Text);cmd.Parameters.AddWithValue("@ba",txt_billingaddress.Text);
                decimal st=0,oc=0;decimal.TryParse(txt_subtotal.Text,out st);decimal.TryParse(txt_othercharges.Text,out oc);
                decimal tot=0;decimal.TryParse(txt_total.Text,out tot);
                cmd.Parameters.AddWithValue("@st",st);cmd.Parameters.AddWithValue("@oc",oc);cmd.Parameters.AddWithValue("@tot",tot==0?st+oc:tot);
                cmd.Parameters.AddWithValue("@dd",dtp_duedate.Value.Date);cmd.Parameters.AddWithValue("@s",cbo_status.Text);
                cmd.Parameters.AddWithValue("@cm",txt_othercomments.Text);cmd.ExecuteNonQuery();
                Reload();MessageBox.Show("Saved!","OK",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }catch(Exception ex2){MessageBox.Show("Save error: "+ex2.Message);}}
        private void Del_(){
            if(string.IsNullOrEmpty(selID)){MessageBox.Show("Select a record.");return;}
            if(MessageBox.Show($"Delete {selID}?","Confirm",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
            try{using var cn=DBConnection.GetConnection();cn.Open();
                using var cmd=new MySqlCommand("DELETE FROM invoices WHERE invoiceID=@id",cn);
                cmd.Parameters.AddWithValue("@id",selID);cmd.ExecuteNonQuery();selID="";Reload();
            }catch(Exception ex){MessageBox.Show("Delete error: "+ex.Message);}}
    }
}
