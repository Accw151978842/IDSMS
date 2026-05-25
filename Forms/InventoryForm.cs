using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    public class InventoryForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSrch,txtID,txt_itemID,txt_qty,txt_location,txt_minstocklv;
        private ComboBox cbo_status;
        private string selID="";

        public InventoryForm():this(""){ }
        public InventoryForm(string filterID)
        {
            Text="Inventory Management";Size=new System.Drawing.Size(1000,560);StartPosition=FormStartPosition.CenterParent;
            txtSrch=new TextBox{Location=new System.Drawing.Point(70,7),Size=new System.Drawing.Size(200,23)};
            Controls.Add(new Label{Text="Search:",Location=new System.Drawing.Point(10,10),Size=new System.Drawing.Size(55,23)});Controls.Add(txtSrch);
            var btnSrch=new Button{Text="Search",Location=new System.Drawing.Point(280,5),Size=new System.Drawing.Size(80,27),BackColor=System.Drawing.Color.SteelBlue,ForeColor=System.Drawing.Color.White,FlatStyle=FlatStyle.Flat};
            btnSrch.Click+=(s,e)=>Reload(txtSrch.Text);Controls.Add(btnSrch);
            dgv=new DataGridView{Location=new System.Drawing.Point(10,42),Size=new System.Drawing.Size(600,440),ReadOnly=true,AllowUserToAddRows=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,BackgroundColor=System.Drawing.Color.White};
            dgv.SelectionChanged+=Sel;Controls.Add(dgv);
            int px=625,py=42,lw=140,fw=200;
            Controls.Add(new Label{Text="Inventory Record",Location=new System.Drawing.Point(px,py-22),Font=new System.Drawing.Font("Segoe UI",9,System.Drawing.FontStyle.Bold)});
            txtID=new TextBox{Size=new System.Drawing.Size(fw,23),ReadOnly=true};
            void Row(string lbl,Control ctrl){Controls.Add(new Label{Text=lbl,Location=new System.Drawing.Point(px,py),Size=new System.Drawing.Size(lw,23)});ctrl.Location=new System.Drawing.Point(px+lw+5,py);Controls.Add(ctrl);py+=33;}
            Row("Inventory ID:",txtID);
            txt_itemID=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Item ID:",txt_itemID);
            txt_qty=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Quantity:",txt_qty);
            txt_location=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Location:",txt_location);
            txt_minstocklv=new TextBox{Size=new System.Drawing.Size(fw,23)};Row("Min Stock Level:",txt_minstocklv);
            cbo_status=new ComboBox{Size=new System.Drawing.Size(fw,23),DropDownStyle=ComboBoxStyle.DropDownList};
            cbo_status.Items.AddRange(new object[]{"InStock","LowStock","OutOfStock"});Row("Status:",cbo_status);
            var btnNew=Btn("New",px,py,System.Drawing.Color.ForestGreen);var btnSave=Btn("Save",px+90,py,System.Drawing.Color.RoyalBlue);var btnDel=Btn("Delete",px+180,py,System.Drawing.Color.Crimson);
            btnNew.Click+=(s,e)=>New_();btnSave.Click+=(s,e)=>Save_();btnDel.Click+=(s,e)=>Del_();
            Controls.AddRange(new Control[]{btnNew,btnSave,btnDel});Reload(filterID);
        }
        private Button Btn(string t,int x,int y,System.Drawing.Color c)=>new Button{Text=t,Location=new System.Drawing.Point(x,y),Size=new System.Drawing.Size(80,30),BackColor=c,ForeColor=System.Drawing.Color.White,FlatStyle=FlatStyle.Flat};
        private void Reload(string s=""){
            try{using var cn=DBConnection.GetConnection();cn.Open();
                string sql=string.IsNullOrEmpty(s)?@"SELECT inv.inventoryID,i.itemID,i.description,inv.qty,inv.location,inv.minstocklv,inv.status FROM inventory inv LEFT JOIN items i ON inv.itemID=i.itemID ORDER BY inv.status,inv.qty LIMIT 300":@"SELECT inv.inventoryID,i.itemID,i.description,inv.qty,inv.location,inv.minstocklv,inv.status FROM inventory inv LEFT JOIN items i ON inv.itemID=i.itemID WHERE inv.inventoryID LIKE @s OR inv.itemID LIKE @s LIMIT 300";
                using var cmd=new MySqlCommand(sql,cn);if(!string.IsNullOrEmpty(s))cmd.Parameters.AddWithValue("@s",$"%{s}%");
                var dt=new DataTable();new MySqlDataAdapter(cmd).Fill(dt);dgv.DataSource=dt;
            }catch(Exception ex){MessageBox.Show("Load error: "+ex.Message);}}
        private void Sel(object? s,EventArgs e){
            if(dgv.SelectedRows.Count==0)return;
            var row=dgv.SelectedRows[0].DataBoundItem as DataRowView;if(row==null)return;
            selID=row["inventoryID"]?.ToString()??"";
            try{using var cn=DBConnection.GetConnection();cn.Open();
                using var cmd=new MySqlCommand("SELECT * FROM inventory WHERE inventoryID=@id",cn);
                cmd.Parameters.AddWithValue("@id",selID);
                using var r=cmd.ExecuteReader();
                if(r.Read()){txtID.Text=r["inventoryID"]?.ToString()??"";
                    txt_itemID.Text=r["itemID"]?.ToString()??"";
                    txt_qty.Text=r["qty"]?.ToString()??"";
                    txt_location.Text=r["location"]?.ToString()??"";
                    txt_minstocklv.Text=r["minstocklv"]?.ToString()??"";
                    cbo_status.Text=r["status"]?.ToString()??"";
                }}catch{}
        }
        private void New_(){selID="INV-"+DateTime.Now.ToString("yyyyMMddHHmmss");txtID.Text=selID;
            txt_itemID.Text=txt_qty.Text=txt_location.Text=txt_minstocklv.Text="";cbo_status.Text="InStock";}
        private void Save_(){
            if(string.IsNullOrEmpty(txtID.Text)){MessageBox.Show("Click New first.");return;}
            try{using var cn=DBConnection.GetConnection();cn.Open();
                bool ex;using(var chk=new MySqlCommand("SELECT COUNT(*) FROM inventory WHERE inventoryID=@id",cn)){chk.Parameters.AddWithValue("@id",txtID.Text);ex=(long)chk.ExecuteScalar()>0;}
                string sql=ex?"UPDATE inventory SET itemID=@i,qty=@q,location=@l,minstocklv=@ms,status=@ss WHERE inventoryID=@id"
                             :"INSERT INTO inventory(inventoryID,itemID,qty,location,minstocklv,status)VALUES(@id,@i,@q,@l,@ms,@ss)";
                using var cmd=new MySqlCommand(sql,cn);
                cmd.Parameters.AddWithValue("@id",txtID.Text);cmd.Parameters.AddWithValue("@i",txt_itemID.Text);
                int q=0;int.TryParse(txt_qty.Text,out q);cmd.Parameters.AddWithValue("@q",q);
                cmd.Parameters.AddWithValue("@l",txt_location.Text);
                int ms=0;int.TryParse(txt_minstocklv.Text,out ms);cmd.Parameters.AddWithValue("@ms",ms);
                cmd.Parameters.AddWithValue("@ss",cbo_status.Text);cmd.ExecuteNonQuery();
                Reload();MessageBox.Show("Saved!","OK",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }catch(Exception ex2){MessageBox.Show("Save error: "+ex2.Message);}}
        private void Del_(){
            if(string.IsNullOrEmpty(selID)){MessageBox.Show("Select a record.");return;}
            if(MessageBox.Show($"Delete {selID}?","Confirm",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
            try{using var cn=DBConnection.GetConnection();cn.Open();
                using var cmd=new MySqlCommand("DELETE FROM inventory WHERE inventoryID=@id",cn);
                cmd.Parameters.AddWithValue("@id",selID);cmd.ExecuteNonQuery();selID="";Reload();
            }catch(Exception ex){MessageBox.Show("Delete error: "+ex.Message);}}
    }
}
