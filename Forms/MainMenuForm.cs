using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace IDSMS.Forms
{
    public class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            Text = "IDSMS — Main Menu ("+Program.CurrentUserName+" / "+Program.CurrentUserRole+")";
            Size = new System.Drawing.Size(900, 640); StartPosition = FormStartPosition.CenterScreen;
            BackColor = System.Drawing.Color.FromArgb(245, 246, 250);

            Controls.Add(new Label { Text = "IDSMS — Integrated Demand & Supply Management System",
                Location = new System.Drawing.Point(20, 16), Size = new System.Drawing.Size(840, 32),
                Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(0, 72, 153), TextAlign = System.Drawing.ContentAlignment.MiddleCenter });

            Controls.Add(new Label { Text = "Welcome, " + Program.CurrentUserName + " | Role: " + Program.CurrentUserRole,
                Location = new System.Drawing.Point(20, 54), Size = new System.Drawing.Size(840, 22),
                ForeColor = System.Drawing.Color.SlateGray, TextAlign = System.Drawing.ContentAlignment.MiddleCenter });

            var buttons = GetMenuButtons();
            int bw = 190, bh = 56, col = 4, margin = 25, startX = 25, startY = 90;
            for (int i = 0; i < buttons.Count; i++)
            {
                int x = startX + (i % col) * (bw + margin);
                int y = startY + (i / col) * (bh + margin);
                Controls.Add(buttons[i]);
                buttons[i].Location = new System.Drawing.Point(x, y);
                buttons[i].Size = new System.Drawing.Size(bw, bh);
            }

            var btnLogout = new Button { Text = "Logout", Size = new System.Drawing.Size(110, 33),
                Location = new System.Drawing.Point(760, 565), BackColor = System.Drawing.Color.Gray,
                ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat };
            btnLogout.Click += (s, e) => { Close(); };
            Controls.Add(btnLogout);
        }

        private List<Button> GetMenuButtons()
        {
            var list = new List<Button>();
            string r = Program.CurrentUserRole;
            void AddBtn(string label, System.Drawing.Color color, Action action)
            { var b = new Button { Text = label, BackColor = color, ForeColor = System.Drawing.Color.White, FlatStyle = FlatStyle.Flat, Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold) };
              b.Click += (s, e) => action(); list.Add(b); }

            bool all = r == "Admin";
            if (all || r == "Sales" || r == "Finance") AddBtn("👥 Customer", System.Drawing.Color.FromArgb(30, 136, 229), () => new CustomerForm().ShowDialog());
            if (all || r == "Sales") AddBtn("📋 Quotation", System.Drawing.Color.FromArgb(67, 160, 71), () => new QuotationForm().ShowDialog());
            if (all || r == "Sales" || r == "Production") AddBtn("🛒 Sales Order", System.Drawing.Color.FromArgb(251, 140, 0), () => new SalesOrderForm().ShowDialog());
            if (all || r == "Finance" || r == "Sales") AddBtn("🧾 Invoice", System.Drawing.Color.FromArgb(216, 27, 96), () => new InvoiceForm().ShowDialog());
            if (all || r == "Production") AddBtn("🏭 Production Order", System.Drawing.Color.FromArgb(81, 45, 168), () => new ProductionOrderForm().ShowDialog());
            if (all || r == "Production" || r == "Inventory") AddBtn("🔩 Material Request", System.Drawing.Color.FromArgb(0, 131, 143), () => new MaterialRequestForm().ShowDialog());
            if (all || r == "Inventory" || r == "Production") AddBtn("📦 Inventory", System.Drawing.Color.FromArgb(33, 150, 243), () => new InventoryForm().ShowDialog());
            if (all || r == "Inventory" || r == "Finance") AddBtn("🛍 Purchase Order", System.Drawing.Color.FromArgb(121, 85, 72), () => new PurchaseOrderForm().ShowDialog());
            if (all || r == "Inventory") AddBtn("🏢 Supplier", System.Drawing.Color.FromArgb(96, 125, 139), () => new SupplierForm().ShowDialog());
            if (all || r == "Logistics") AddBtn("🚚 Delivery Note", System.Drawing.Color.FromArgb(239, 83, 80), () => new DeliveryNoteForm().ShowDialog());
            if (all || r == "Logistics" || r == "Sales") AddBtn("📡 Shipment Tracking", System.Drawing.Color.FromArgb(38, 166, 154), () => new ShipmentTrackingForm().ShowDialog());
            if (all || r == "Logistics") AddBtn("✍ Reply Slip", System.Drawing.Color.FromArgb(255, 112, 67), () => new ReplySlipForm().ShowDialog());
            if (all || r == "Sales" || r == "Finance") AddBtn("📣 Complaint", System.Drawing.Color.FromArgb(183, 28, 28), () => new ComplaintForm().ShowDialog());
            if (all || r == "Finance") AddBtn("📊 Reports", System.Drawing.Color.FromArgb(40, 53, 147), () => new ReportForm().ShowDialog());
            if (all) AddBtn("👤 User Mgmt", System.Drawing.Color.FromArgb(69, 90, 100), () => new UserManagementForm().ShowDialog());
            return list;
        }
    }
}
