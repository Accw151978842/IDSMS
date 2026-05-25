# IDSMS — Integrated Delivery Services Management System
## Premium Living Furniture Co. Ltd.

### Architecture: 2-Tier (Windows Forms ↔ MySQL)

```
┌──────────────────────────────────┐
│  Presentation + Logic Layer      │
│  Windows Forms (.NET 8 / C#)     │
│  - All UI & business rules       │
│  - Direct SQL via MySql.Data     │
└──────────────┬───────────────────┘
               │ TCP/IP :3306
┌──────────────▼───────────────────┐
│         Data Layer               │
│         MySQL 8.x                │
│         Database: idsms_db       │
└──────────────────────────────────┘
```

---

## Quick Start

### 1. Database Setup
Open MySQL Workbench / phpMyAdmin and run:
```
Database/schema.sql
```

### 2. Update Connection String
Edit `Database/DBConnection.cs` line 7:
```csharp
"Server=localhost;Database=idsms_db;Uid=root;Pwd=YOUR_PASSWORD;CharSet=utf8mb4;"
```

### 3. Install NuGet Package
In Visual Studio 2022 → Tools → NuGet Package Manager → Package Manager Console:
```
Install-Package MySql.Data
```

### 4. Build & Run
Open `IDSMS.csproj` in Visual Studio 2022 → Press F5

---

## Default Login
| UserID      | Password    | Role       |
|-------------|-------------|------------|
| USR0000001  | Admin@1234  | Admin      |
| USR0000002  | Pass@1234   | Sales      |
| USR0000003  | Pass@1234   | Production |
| USR0000004  | Pass@1234   | Inventory  |
| USR0000005  | Pass@1234   | Logistics  |
| USR0000006  | Pass@1234   | Finance    |

⚠️ Change all passwords after first login!

---

## Role-Based Access

| Module                | Admin | Sales | Prod | Inv | Log | Fin |
|----------------------|-------|-------|------|-----|-----|-----|
| Sales Orders         | ✓     | ✓     |      |     |     |     |
| Customers            | ✓     | ✓     |      |     |     |     |
| Quotations           | ✓     | ✓     |      |     |     |     |
| Invoices             | ✓     |       |      |     |     | ✓   |
| Production Orders    | ✓     |       | ✓    |     |     |     |
| Material Requests    | ✓     |       | ✓    | ✓   |     |     |
| Inventory            | ✓     |       |      | ✓   |     |     |
| Purchase Orders      | ✓     |       |      | ✓   |     | ✓   |
| Suppliers            | ✓     |       |      | ✓   |     |     |
| Delivery Notes       | ✓     |       |      |     | ✓   |     |
| Shipment Tracking    | ✓     | ✓     |      |     | ✓   |     |
| Reply Slips          | ✓     |       |      |     | ✓   |     |
| Complaints           | ✓     | ✓     |      |     |     |     |
| User Management      | ✓     |       |      |     |     |     |
| Reports              | ✓     | ✓     |      | ✓   |     | ✓   |

---

## Project Structure
```
IDSMS/
├── IDSMS.csproj
├── Program.cs
├── README.md
├── Database/
│   ├── DBConnection.cs
│   └── schema.sql
└── Forms/
    ├── LoginForm.cs
    ├── MainMenuForm.cs
    ├── CustomerForm.cs
    ├── QuotationForm.cs
    ├── SalesOrderForm.cs
    ├── InvoiceForm.cs
    ├── ProductionOrderForm.cs
    ├── MaterialRequestForm.cs
    ├── InventoryForm.cs
    ├── PurchaseOrderForm.cs
    ├── SupplierForm.cs
    ├── DeliveryNoteForm.cs
    ├── ShipmentTrackingForm.cs
    ├── ReplySlipForm.cs
    ├── ComplaintForm.cs
    ├── UserManagementForm.cs
    └── ReportForm.cs
```
