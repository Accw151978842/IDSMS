# IDSMS — Integrated Demand & Supply Management System

## Architecture: 2-Tier (Windows Forms ↔ MySQL)

```
┌──────────────────────────────┐
│  Layer 1: Windows Forms UI   │
│  C# .NET Framework 4.8       │
│  Direct SQL via MySql.Data   │
└──────────────┬───────────────┘
               │ TCP :3306
┌──────────────▼───────────────┐
│  Layer 2: MySQL Database     │
│  idsms_db                    │
└──────────────────────────────┘
```

## Quick Start

### 1. Database Setup
- Open **MySQL Workbench**
- Run `Database/schema.sql`
- Default admin: `USR0000001` / `Admin@1234`

### 2. Configure Connection
Open `Database/DBConnection.cs` and update:
```csharp
"Server=localhost;Database=idsms_db;Uid=root;Pwd=YOUR_PASSWORD;"
```

### 3. Install NuGet
In Visual Studio Package Manager Console:
```
Install-Package MySql.Data
```

### 4. Run
Open `IDSMS.csproj` → Press **F5**

## Module Permissions
| Role | Modules |
|------|--------|
| Admin | All |
| Sales | Customer, Quotation, Sales Order, Invoice, Complaint, Shipment |
| Production | Sales Order, Production Order, Material Request, Inventory |
| Inventory | Inventory, Material Request, Purchase Order, Supplier |
| Logistics | Delivery Note, Shipment Tracking, Reply Slip |
| Finance | Invoice, Purchase Order, Reports |

## Forms Included
- LoginForm, MainMenuForm
- CustomerForm, QuotationForm, SalesOrderForm, InvoiceForm
- ProductionOrderForm, MaterialRequestForm
- InventoryForm, PurchaseOrderForm, SupplierForm
- DeliveryNoteForm, ShipmentTrackingForm, ReplySlipForm
- ComplaintForm, UserManagementForm, ReportForm
