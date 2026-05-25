-- IDSMS Database Schema - Premium Living Furniture Co. Ltd.
CREATE DATABASE IF NOT EXISTS idsms_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE idsms_db;

CREATE TABLE IF NOT EXISTS users (
    userID VARCHAR(15) PRIMARY KEY,
    empname VARCHAR(50) NOT NULL,
    role ENUM('Admin','Sales','Production','Inventory','Logistics','Finance') NOT NULL,
    email VARCHAR(80), password VARCHAR(100) NOT NULL
) ENGINE=InnoDB;

INSERT IGNORE INTO users VALUES
('USR0000001','Administrator','Admin','admin@plfco.com','Admin@1234'),
('USR0000002','Sales Manager','Sales','sales@plfco.com','Pass@1234'),
('USR0000003','Production Manager','Production','prod@plfco.com','Pass@1234'),
('USR0000004','Inventory Officer','Inventory','inv@plfco.com','Pass@1234'),
('USR0000005','Logistics Officer','Logistics','log@plfco.com','Pass@1234'),
('USR0000006','Finance Officer','Finance','fin@plfco.com','Pass@1234');

CREATE TABLE IF NOT EXISTS customer (
    customerID VARCHAR(15) PRIMARY KEY, name VARCHAR(80) NOT NULL,
    contact VARCHAR(25), email VARCHAR(80),
    billingAddress VARCHAR(150), shippingAddress VARCHAR(150),
    type ENUM('B2B','B2C') DEFAULT 'B2C'
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS items (
    itemID VARCHAR(15) PRIMARY KEY, description VARCHAR(100) NOT NULL,
    type ENUM('FinalProduct','RawMaterial','Component') NOT NULL,
    unitcost DECIMAL(10,2) DEFAULT 0.00, unitprice DECIMAL(10,2) DEFAULT 0.00, unit VARCHAR(15) DEFAULT 'PCS'
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS suppliers (
    supplierID VARCHAR(15) PRIMARY KEY, suppname VARCHAR(80) NOT NULL,
    contact VARCHAR(25), email VARCHAR(80), payterms VARCHAR(20) DEFAULT 'Net30',
    ontimedlvrate DECIMAL(5,2) DEFAULT 0.00, defectrate DECIMAL(5,2) DEFAULT 0.00
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS quotations (
    quotationID VARCHAR(20) PRIMARY KEY, customerID VARCHAR(15),
    issuedate DATE, validuntil DATE,
    status ENUM('Draft','Sent','Accepted','Rejected','Expired') DEFAULT 'Draft',
    paymentmethod VARCHAR(30), deliverymethod VARCHAR(20), estimateddate DATE, versionno INT DEFAULT 1,
    FOREIGN KEY (customerID) REFERENCES customer(customerID) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS salesorders (
    orderID VARCHAR(25) PRIMARY KEY, customerID VARCHAR(15), orderdate DATE,
    status ENUM('Created','Confirmed','InProduction','Ready','Dispatched','Delivered','Closed') DEFAULT 'Created',
    deliverymethod VARCHAR(20), dispatchdate DATE, shippingaddress VARCHAR(150),
    FOREIGN KEY (customerID) REFERENCES customer(customerID) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS salesorderlines (
    lineID INT AUTO_INCREMENT PRIMARY KEY, orderID VARCHAR(25), itemID VARCHAR(15),
    qty INT DEFAULT 1, price DECIMAL(10,2) DEFAULT 0.00, discount DECIMAL(5,2) DEFAULT 0.00,
    FOREIGN KEY (orderID) REFERENCES salesorders(orderID) ON DELETE CASCADE,
    FOREIGN KEY (itemID) REFERENCES items(itemID) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS invoices (
    invoiceID VARCHAR(25) PRIMARY KEY, orderID VARCHAR(25), customerID VARCHAR(15),
    billingaddress VARCHAR(150), subtotal DECIMAL(10,2) DEFAULT 0.00,
    othercharges DECIMAL(10,2) DEFAULT 0.00, total DECIMAL(10,2) DEFAULT 0.00,
    duedate DATE, status ENUM('Unpaid','Partial','Paid','Overdue') DEFAULT 'Unpaid',
    othercomments VARCHAR(200),
    FOREIGN KEY (orderID) REFERENCES salesorders(orderID) ON DELETE SET NULL,
    FOREIGN KEY (customerID) REFERENCES customer(customerID) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS production (
    prodID VARCHAR(25) PRIMARY KEY, orderID VARCHAR(25), itemID VARCHAR(15), serialNo VARCHAR(12),
    step ENUM('Cutting','Assembly','Finishing','QC','Packing') DEFAULT 'Cutting',
    plannedstart DATE, plannedend DATE, priority ENUM('Low','Medium','High','Urgent') DEFAULT 'Medium',
    eta DATE, status ENUM('Scheduled','InProgress','Completed','OnHold','Cancelled') DEFAULT 'Scheduled',
    FOREIGN KEY (orderID) REFERENCES salesorders(orderID) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS materialrequests (
    mrid VARCHAR(25) PRIMARY KEY, prodID VARCHAR(25), requireddate DATE,
    deliverylocation VARCHAR(50), urgency ENUM('Normal','Urgent','Critical') DEFAULT 'Normal',
    priority ENUM('Low','Medium','High') DEFAULT 'Medium',
    status ENUM('Pending','Approved','Issued','Rejected') DEFAULT 'Pending',
    FOREIGN KEY (prodID) REFERENCES production(prodID) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS inventory (
    inventoryID VARCHAR(25) PRIMARY KEY, itemID VARCHAR(15),
    qty INT DEFAULT 0, location VARCHAR(20), minstocklv INT DEFAULT 0,
    status ENUM('InStock','LowStock','OutOfStock') DEFAULT 'InStock',
    FOREIGN KEY (itemID) REFERENCES items(itemID) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS purchaseorders (
    poid VARCHAR(25) PRIMARY KEY, supplierID VARCHAR(15), orderdate DATE, expecteddate DATE,
    status ENUM('Draft','Sent','Confirmed','Received','Cancelled') DEFAULT 'Draft',
    totalcost DECIMAL(10,2) DEFAULT 0.00,
    FOREIGN KEY (supplierID) REFERENCES suppliers(supplierID) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS deliverynotes (
    dnID VARCHAR(25) PRIMARY KEY, orderID VARCHAR(25), customerID VARCHAR(15),
    dndate DATE, dispatchdate DATE, deliverymethod VARCHAR(20),
    shippingaddress VARCHAR(150), driver VARCHAR(50), signedby VARCHAR(50),
    status ENUM('Prepared','Dispatched','Delivered','Confirmed') DEFAULT 'Prepared',
    FOREIGN KEY (orderID) REFERENCES salesorders(orderID) ON DELETE SET NULL,
    FOREIGN KEY (customerID) REFERENCES customer(customerID) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS shipments (
    shipmentID VARCHAR(25) PRIMARY KEY, orderID VARCHAR(25), dnID VARCHAR(25),
    dispatchdate DATE, driver VARCHAR(50),
    status ENUM('Preparing','InTransit','Delivered','Failed') DEFAULT 'Preparing',
    FOREIGN KEY (orderID) REFERENCES salesorders(orderID) ON DELETE SET NULL,
    FOREIGN KEY (dnID) REFERENCES deliverynotes(dnID) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS complaints (
    complaintID VARCHAR(25) PRIMARY KEY, customerID VARCHAR(15), orderID VARCHAR(25), serialNo VARCHAR(12),
    category ENUM('Damage','WrongItem','LateDelivery','QualityIssue','Other') DEFAULT 'Other',
    description VARCHAR(300), priority ENUM('Low','Medium','High','Critical') DEFAULT 'Medium',
    status ENUM('Open','InProgress','Resolved','Closed') DEFAULT 'Open',
    handledby VARCHAR(15), resolution VARCHAR(300),
    FOREIGN KEY (customerID) REFERENCES customer(customerID) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS audittrail (
    auditID VARCHAR(25) PRIMARY KEY, userID VARCHAR(15),
    actioncode VARCHAR(10) NOT NULL, module VARCHAR(30) NOT NULL,
    recordID VARCHAR(30), details VARCHAR(200),
    timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (userID) REFERENCES users(userID) ON DELETE SET NULL
) ENGINE=InnoDB;
-- Default login: USR0000001 / Admin@1234
