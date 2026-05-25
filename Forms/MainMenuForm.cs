using System;
using System.Windows.Forms;

namespace IDSMS.Forms
{
    public class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            Text = $"IDSMS - {Program.CurrentUserName} [{Program.CurrentUserRole}]";
            Size = new System.Drawing.Size(920, 640); StartPosition = FormStartPosition.CenterScreen;
            var header = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = System.Drawing.Color.FromArgb(0, 84, 166) };
            var lh = new Label { Text = "Premium Living Furniture - IDSMS",
                Font = new System.Drawing.Font("Segoe UI", 13, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.White, Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter };
            var btnOut = new Button { Text = "Logout", Size = new System.Drawing.Size(90, 36), Dock = DockStyle.Right,
                BackColor = System.Drawing.Color.Crimson, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat };
            btnOut.Click += (s, e) => Close();
            header.Controls.AddRange(new Control[] { lh, btnOut });
            var status = new StatusStrip();
            status.Items.Add(new ToolStripStatusLabel($"User: {Program.CurrentUserName}  |  Role: {Program.CurrentUserRole}  |  {DateTime.Now:dd/MM/yyyy HH:mm}"));
            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(25),
                FlowDirection = FlowDirection.LeftToRight, WrapContents = true, AutoScroll = true };
            var modules = new (string n, string[] roles)[]
            {
                ("Sales Orders",new[]{"Sales","Admin","Finance"}),("Customers",new[]{"Sales","Admin"}),
                ("Quotations",new[]{"Sales","Admin"}),("Invoices",new[]{"Finance","Admin"}),
                ("Production Orders",new[]{"Production","Admin"}),("Material Requests",new[]{"Production","Inventory","Admin"}),
                ("Inventory",new[]{"Inventory","Admin"}),("Purchase Orders",new[]{"Inventory","Finance","Admin"}),
                ("Suppliers",new[]{"Inventory","Admin"}),("Delivery Notes",new[]{"Logistics","Admin"}),
                ("Shipment Tracking",new[]{"Logistics","Sales","Admin"}),("Reply Slips",new[]{"Logistics","Admin"}),
                ("Complaints",new[]{"Sales","Admin"}),("Users",new[]{"Admin"}),
                ("Reports",new[]{"Admin","Finance","Sales","Inventory"}),
            };
            string role = Program.CurrentUserRole;
            foreach (var (n, roles) in modules)
            {
                bool ok = Array.Exists(roles, r => r == role);
                var b = new Button { Text = n, Size = new System.Drawing.Size(145, 95), Margin = new Padding(8),
                    FlatStyle = FlatStyle.Flat, Font = new System.Drawing.Font("Segoe UI", 9), Enabled = ok, Tag = n,
                    BackColor = ok ? System.Drawing.Color.FromArgb(224,240,255) : System.Drawing.SystemColors.Control,
                    ForeColor = ok ? System.Drawing.Color.FromArgb(0,70,140) : System.Drawing.Color.Gray };
                b.Click += Btn_Click; panel.Controls.Add(b);
            }
            Controls.Add(panel); Controls.Add(header); Controls.Add(status);
        }
        private void Btn_Click(object? sender, EventArgs e)
        {
            var tag = ((Button)sender!).Tag?.ToString() ?? "";
            Form? f = tag switch
            {
                "Sales Orders"=>new SalesOrderForm(),"Customers"=>new CustomerForm(),
                "Quotations"=>new QuotationForm(),"Invoices"=>new InvoiceForm(""),
                "Production Orders"=>new ProductionOrderForm(),"Material Requests"=>new MaterialRequestForm(),
                "Inventory"=>new InventoryForm(),"Purchase Orders"=>new PurchaseOrderForm(),
                "Suppliers"=>new SupplierForm(),"Delivery Notes"=>new DeliveryNoteForm(),
                "Shipment Tracking"=>new ShipmentTrackingForm(),"Reply Slips"=>new ReplySlipForm(),
                "Complaints"=>new ComplaintForm(),"Users"=>new UserManagementForm(),
                "Reports"=>new ReportForm(),_=>null
            };
            f?.ShowDialog();
        }
    }
}
