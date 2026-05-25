using System;
using System.Windows.Forms;

namespace IDSMS.Forms
{
    public class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            Text = "IDSMS - Main Menu  [" + Program.CurrentUserRole + ": " + Program.CurrentUserName + "]";
            Size = new System.Drawing.Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            FormClosed += (s, e) => Application.Exit();

            var lblTitle = new Label
            {
                Text = "Integrated Distribution & Supply Management System",
                Font = new System.Drawing.Font("Segoe UI", 13, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(30, 30, 80),
                Location = new System.Drawing.Point(10, 15),
                Size = new System.Drawing.Size(860, 35)
            };
            Controls.Add(lblTitle);

            var lblSub = new Label
            {
                Text = "Role: " + Program.CurrentUserRole + "   |   User: " + Program.CurrentUserName,
                Location = new System.Drawing.Point(12, 50),
                Size = new System.Drawing.Size(600, 22),
                ForeColor = System.Drawing.Color.Gray
            };
            Controls.Add(lblSub);

            string role = Program.CurrentUserRole;

            // Sales
            if (role == "Admin" || role == "Sales")
            {
                AddBtn("Customer Mgmt",       10,  90, () => new CustomerForm().ShowDialog());
                AddBtn("Quotation",          170,  90, () => new QuotationForm().ShowDialog());
                AddBtn("Sales Order",        330,  90, () => new SalesOrderForm().ShowDialog());
                AddBtn("Invoice",            490,  90, () => new InvoiceForm().ShowDialog());
            }

            // Production
            if (role == "Admin" || role == "Production")
            {
                AddBtn("Production Order",    10, 165, () => new ProductionOrderForm().ShowDialog());
                AddBtn("Material Request",   170, 165, () => new MaterialRequestForm().ShowDialog());
            }

            // Inventory
            if (role == "Admin" || role == "Inventory")
            {
                AddBtn("Inventory",          330, 165, () => new InventoryForm().ShowDialog());
                AddBtn("Purchase Order",     490, 165, () => new PurchaseOrderForm().ShowDialog());
                AddBtn("Supplier",           650, 165, () => new SupplierForm().ShowDialog());
            }

            // Logistics
            if (role == "Admin" || role == "Logistics")
            {
                AddBtn("Delivery Note",       10, 240, () => new DeliveryNoteForm().ShowDialog());
                AddBtn("Shipment Tracking",  170, 240, () => new ShipmentTrackingForm().ShowDialog());
                AddBtn("Reply Slip",         330, 240, () => new ReplySlipForm().ShowDialog());
            }

            // Customer Service
            if (role == "Admin" || role == "Sales" || role == "Logistics")
            {
                AddBtn("Complaint",          490, 240, () => new ComplaintForm().ShowDialog());
            }

            // Finance
            if (role == "Admin" || role == "Finance")
            {
                AddBtn("Reports",             10, 315, () => new ReportForm().ShowDialog());
            }

            // Admin only
            if (role == "Admin")
            {
                AddBtn("User Management",    170, 315, () => new UserManagementForm().ShowDialog());
            }

            var btnLogout = new Button
            {
                Text = "Logout",
                Location = new System.Drawing.Point(760, 520),
                Size = new System.Drawing.Size(100, 33),
                BackColor = System.Drawing.Color.FromArgb(200, 50, 50),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLogout.Click += (s, e) => { new LoginForm().Show(); Close(); };
            Controls.Add(btnLogout);
        }

        private void AddBtn(string text, int x, int y, Action action)
        {
            var btn = new Button
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(150, 55),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 9)
            };
            btn.Click += (s, e) => action();
            Controls.Add(btn);
        }
    }
}
