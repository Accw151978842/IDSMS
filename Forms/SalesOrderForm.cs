using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    public class SalesOrderForm : Form
    {
        private DataGridView dgvO, dgvL;
        private TextBox txtSrch,txtOID,txtCID,txtSAddr;
        private ComboBox cboStatus,cboDM;
        private DateTimePicker dtpOD,dtpDD;
        private string selID="";

        public SalesOrderForm()
        {
            Text="Sales Order Management";Size=new System.Drawing.Size(1100,680);StartPosition=FormStartPosition.CenterParent;
            txtSrch=new TextBox{Location=new System.Drawing.Point(70,7),Size=new System.Drawing.Size(200,23)};
            Controls.Add(new Label{Text="Search:",Location=new System.Drawing.Point(10,10),Size=new System.Drawing.Size(55,23)});Controls.Add(txtSrch);
            var btnSrch=new Button{Text="Search",Location=new System.Drawing.Point(280,5),Size=new System.Drawing.Size(80,27),BackColor=System.Drawing.Color.SteelBlue,ForeColor=System.Drawing.Color.White,FlatStyle=FlatStyle.Flat};
            btnSrch.Click+=(s,e)=>Reload(txtSrch.Text);Controls.Add(btnSrch);
            dgvO=DGV(10,42,650,230);dgvO.SelectionChanged+=SelO;Controls.Add(dgvO);
            Controls.Add(new Label{Text="Order Lines:",Location=new System.Drawing.Point(10,280),Font=new System.Drawing.Font("Segoe UI",9,System.Drawing.FontStyle.Bold)});
            dgvL=DGV(10,300,650,260);Controls.Add(dgvL);
            int px=675,py=42,lw=130,fw=200;
            Controls.Add(new Label{Text="Order Details",Location=new System.Drawing.Point(px,py-22),Font=new System.Drawing.Font("Segoe UI",9,System.Drawing.FontStyle.Bold)});
            void Row(string lbl,Control ctrl){Controls.Add(new Label{Text=lbl,Location=new System.Drawing.Point(px,py),Size=new System.Drawing.Size(lw,23)});ctrl.Location=new System.Drawing.Point(px+lw+5,py);Controls.Add(ctrl);py+=33;}
            txtOID=new TextBox{Size=new System.Drawing.Size(fw,23),ReadOnly=true};Row("Order ID:",txtOID);
            txtCID=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Customer ID *:",txtCID);
            dtpOD=new DateTimePicker{Size=new System.Drawing.Size(fw,23),Format=DateTimePickerFormat.Short};Row("Order Date:",dtpOD);
            cboStatus=new ComboBox{Size=new System.Drawing.Size(fw,23),DropDownStyle=ComboBoxStyle.DropDownList};
            cboStatus.Items.AddRange(new object[]{"Created","Confirmed","InProduction","Ready","Dispatched","Delivered","Closed"});Row("Status:",cboStatus);
            cboDM=new ComboBox{Size=new System.Drawing.Size(fw,23),DropDownStyle=ComboBoxStyle.DropDownList};
            cboDM.Items.AddRange(new object[]{"Courier","Self-collect","Truck"});Row("Delivery Method:",cboDM);
            dtpDD=new DateTimePicker{Size=new System.Drawing.Size(fw,23),Format=DateTimePickerFormat.Short};Row("Dispatch Date:",dtpDD);
            txtSAddr=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Shipping Address:",txtSAddr);
            py+=7;
            var btnNew=Btn("New",px,py,System.Drawing.Color.ForestGreen);
            var btnSave=Btn("Save",px+90,py,System.Drawing.Color.RoyalBlue);
            var btnDel=Btn("Delete",px+180,py,System.Drawing.Color.Crimson);
            var btnInv=Btn("Invoice",px+270,py,System.Drawing.Color.DarkOrange);
            btnNew.Click+=(s,e)=>New_();btnSave.Click+=(s,e)=>Save_();
            btnDel.Click+=(s,e)=>Del_();btnInv.Click+=(s,e)=>Inv_();
            Controls.AddRange(new Control[]{btnNew,btnSave,btnDel,btnInv});
            Reload();
        }
        private Button Btn(string t,int x,int y,System.Drawing.Color c)=>new Button{Text=t,Location=new System.Drawing.Point(x,y),Size=new System.Drawing.Size(80,30),BackColor=c,ForeColor=System.Drawing.Color.White,FlatStyle=FlatStyle.Flat};
        private DataGridView DGV(int x,int y,int w,int h)=>new DataGridView{Location=new System.Drawing.Point(x,y),Size=new System.Drawing.Size(w,h),ReadOnly=true,AllowUserToAddRows=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,BackgroundColor=System.Drawing.Color.White};
        private void Reload(string s=""){
            try{using var cn=DBConnection.GetConnection();cn.Open();
                string sql=@"SELECT so.orderID,c.name AS Customer,so.orderdate,so.status,so.deliverymethod,so.dispatchdate FROM salesorders so LEFT JOIN customer c ON so.customerID=c.customerID";
                if(!string.IsNullOrEmpty(s))sql+=" WHERE so.orderID LIKE @s OR c.name LIKE @s";
                sql+=" ORDER BY so.orderdate DESC LIMIT 300";
                using var cmd=new MySqlCommand(sql,cn);
                if(!string.IsNullOrEmpty(s))cmd.Parameters.AddWithValue("@s",$"%{s}%");
                var dt=new DataTable();new MySqlDataAdapter(cmd).Fill(dt);dgvO.DataSource=dt;
            }catch(Exception ex){MessageBox.Show("Error: "+ex.Message);}}
        private void LoadLines(string oid){
            try{using var cn=DBConnection.GetConnection();cn.Open();
                using var cmd=new MySqlCommand(@"SELECT sol.lineID,i.itemID,i.description,sol.qty,sol.price,sol.discount,ROUND(sol.qty*sol.price*(1-sol.discount/100),2) AS LineTotal FROM salesorderlines sol JOIN items i ON sol.itemID=i.itemID WHERE sol.orderID=@id",cn);
                cmd.Parameters.AddWithValue("@id",oid);
                var dt=new DataTable();new MySqlDataAdapter(cmd).Fill(dt);dgvL.DataSource=dt;
            }catch{}}
        private void SelO(object? s,EventArgs e){
            if(dgvO.SelectedRows.Count==0)return;
            var row=dgvO.SelectedRows[0].DataBoundItem as DataRowView;if(row==null)return;
            selID=row["orderID"]?.ToString()??"";
            LoadLines(selID);
            try{using var cn=DBConnection.GetConnection();cn.Open();
                using var cmd=new MySqlCommand("SELECT * FROM salesorders WHERE orderID=@id",cn);
                cmd.Parameters.AddWithValue("@id",selID);
                using var r=cmd.ExecuteReader();
                if(r.Read()){txtOID.Text=r["orderID"].ToString();txtCID.Text=r["customerID"].ToString();
                    cboStatus.Text=r["status"].ToString();cboDM.Text=r["deliverymethod"].ToString();
                    txtSAddr.Text=r["shippingaddress"].ToString();
                    if(r["orderdate"]!=DBNull.Value)dtpOD.Value=Convert.ToDateTime(r["orderdate"]);
                    if(r["dispatchdate"]!=DBNull.Value)dtpDD.Value=Convert.ToDateTime(r["dispatchdate"]);
                }}catch{}
        }
        private void New_(){selID="SO-"+DateTime.Now.ToString("yyyyMMddHHmmss");txtOID.Text=selID;
            txtCID.Text=txtSAddr.Text="";dtpOD.Value=DateTime.Today;cboStatus.Text="Created";
            cboDM.SelectedIndex=-1;dgvL.DataSource=null;}
        private void Save_(){
            if(string.IsNullOrEmpty(txtOID.Text)){MessageBox.Show("Click New first.");return;}
            if(string.IsNullOrWhiteSpace(txtCID.Text)){MessageBox.Show("Customer ID required.");return;}
            try{using var cn=DBConnection.GetConnection();cn.Open();
                bool ex;using(var chk=new MySqlCommand("SELECT COUNT(*) FROM salesorders WHERE orderID=@id",cn)){chk.Parameters.AddWithValue("@id",txtOID.Text);ex=(long)chk.ExecuteScalar()>0;}
                string sql=ex?"UPDATE salesorders SET customerID=@c,orderdate=@od,status=@s,deliverymethod=@dm,dispatchdate=@dd,shippingaddress=@sa WHERE orderID=@id"
                             :"INSERT INTO salesorders(orderID,customerID,orderdate,status,deliverymethod,dispatchdate,shippingaddress)VALUES(@id,@c,@od,@s,@dm,@dd,@sa)";
                using var cmd=new MySqlCommand(sql,cn);
                cmd.Parameters.AddWithValue("@id",txtOID.Text);cmd.Parameters.AddWithValue("@c",txtCID.Text);
                cmd.Parameters.AddWithValue("@od",dtpOD.Value.Date);cmd.Parameters.AddWithValue("@s",cboStatus.Text);
                cmd.Parameters.AddWithValue("@dm",cboDM.Text);cmd.Parameters.AddWithValue("@dd",dtpDD.Value.Date);
                cmd.Parameters.AddWithValue("@sa",txtSAddr.Text);cmd.ExecuteNonQuery();
                Reload();MessageBox.Show("Saved!","OK",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }catch(Exception ex2){MessageBox.Show("Error: "+ex2.Message);}}
        private void Del_(){
            if(string.IsNullOrEmpty(selID)){MessageBox.Show("Select a record.");return;}
            if(MessageBox.Show($"Delete {selID}?","Confirm",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
            try{using var cn=DBConnection.GetConnection();cn.Open();
                using var cmd=new MySqlCommand("DELETE FROM salesorders WHERE orderID=@id",cn);
                cmd.Parameters.AddWithValue("@id",selID);cmd.ExecuteNonQuery();
                selID="";Reload();dgvL.DataSource=null;
            }catch(Exception ex){MessageBox.Show("Error: "+ex.Message);}}
        private void Inv_(){
            if(string.IsNullOrEmpty(selID)){MessageBox.Show("Select an order first.");return;}
            new InvoiceForm(selID).ShowDialog();}
    }
}
