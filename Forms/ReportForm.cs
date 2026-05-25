using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using IDSMS.Database;

namespace IDSMS.Forms
{
    public class ReportForm : Form
    {
        private DataGridView dgv;
        private ComboBox cboType;
        private Label lblTitle;

        public ReportForm()
        {
            Text="Reports & Dashboard";Size=new System.Drawing.Size(1000,640);StartPosition=FormStartPosition.CenterParent;
            Controls.Add(new Label{Text="Report:",Location=new System.Drawing.Point(10,15),Size=new System.Drawing.Size(60,23)});
            cboType=new ComboBox{Location=new System.Drawing.Point(75,12),Size=new System.Drawing.Size(240,23),DropDownStyle=ComboBoxStyle.DropDownList};
            cboType.Items.AddRange(new object[]{
                "Sales Orders by Status","Invoice Summary (AR)","Inventory Stock Levels","Low Stock Alerts",
                "Purchase Orders Summary","Delivery Performance","Complaints by Priority",
                "Supplier KPI","Audit Trail (Last 100)"});
            cboType.SelectedIndex=0;Controls.Add(cboType);
            var btnRun=new Button{Text="Run Report",Location=new System.Drawing.Point(330,10),Size=new System.Drawing.Size(110,27),BackColor=System.Drawing.Color.FromArgb(0,120,215),ForeColor=System.Drawing.Color.White,FlatStyle=FlatStyle.Flat};
            btnRun.Click+=Run_;Controls.Add(btnRun);
            lblTitle=new Label{Text="Select a report and click Run",Location=new System.Drawing.Point(10,47),Size=new System.Drawing.Size(960,25),Font=new System.Drawing.Font("Segoe UI",10,System.Drawing.FontStyle.Bold),ForeColor=System.Drawing.Color.DarkBlue};Controls.Add(lblTitle);
            dgv=new DataGridView{Location=new System.Drawing.Point(10,78),Size=new System.Drawing.Size(960,520),ReadOnly=true,AllowUserToAddRows=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,BackgroundColor=System.Drawing.Color.White};Controls.Add(dgv);
        }
        private void Run_(object? s,EventArgs e)
        {
            string t=cboType.Text;
            string sql=t switch{
                "Sales Orders by Status" =>"SELECT status,COUNT(*) AS Count FROM salesorders GROUP BY status ORDER BY Count DESC",
                "Invoice Summary (AR)"   =>"SELECT i.invoiceID,c.name AS Customer,i.total,i.duedate,i.status FROM invoices i LEFT JOIN customer c ON i.customerID=c.customerID WHERE i.status!='Paid' ORDER BY i.duedate",
                "Inventory Stock Levels" =>"SELECT i.itemID,i.description,inv.qty,inv.minstocklv,inv.location,inv.status FROM inventory inv JOIN items i ON inv.itemID=i.itemID ORDER BY inv.qty",
                "Low Stock Alerts"       =>"SELECT i.itemID,i.description,inv.qty,inv.minstocklv,inv.location FROM inventory inv JOIN items i ON inv.itemID=i.itemID WHERE inv.qty<=inv.minstocklv ORDER BY inv.qty",
                "Purchase Orders Summary"=>"SELECT po.poid,s.suppname AS Supplier,po.orderdate,po.expecteddate,po.status,po.totalcost FROM purchaseorders po LEFT JOIN suppliers s ON po.supplierID=s.supplierID ORDER BY po.orderdate DESC LIMIT 200",
                "Delivery Performance"   =>"SELECT dn.dnID,c.name AS Customer,dn.dispatchdate,dn.status,dn.driver FROM deliverynotes dn LEFT JOIN customer c ON dn.customerID=c.customerID ORDER BY dn.dndate DESC LIMIT 200",
                "Complaints by Priority" =>"SELECT priority,status,COUNT(*) AS Count FROM complaints GROUP BY priority,status ORDER BY priority,status",
                "Supplier KPI"           =>"SELECT supplierID,suppname,ontimedlvrate AS OnTime_pct,defectrate AS Defect_pct,payterms FROM suppliers ORDER BY ontimedlvrate DESC",
                "Audit Trail (Last 100)" =>"SELECT at.auditID,u.empname AS UserName,at.actioncode,at.module,at.recordID,at.details,at.timestamp FROM audittrail at LEFT JOIN users u ON at.userID=u.userID ORDER BY at.timestamp DESC LIMIT 100",
                _=>"SELECT 'No query' AS Message"
            };
            try{using var cn=DBConnection.GetConnection();cn.Open();
                using var cmd=new MySqlCommand(sql,cn);
                var dt=new DataTable();new MySqlDataAdapter(cmd).Fill(dt);
                dgv.DataSource=dt;lblTitle.Text=$"{t} - {dt.Rows.Count} records";
            }catch(Exception ex){MessageBox.Show("Report error: "+ex.Message);}
        }
    }
}
