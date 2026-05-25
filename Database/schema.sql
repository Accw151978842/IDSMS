-- IDSMS Database Schema for MySQL
-- Run this entire file in MySQL Workbench or CLI
CREATE DATABASE IF NOT EXISTS idsms_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE idsms_db;

CREATE TABLE IF NOT EXISTS users (
  userID VARCHAR(20) PRIMARY KEY,
  empname VARCHAR(100) NOT NULL,
  role ENUM('Admin','Sales','Production','Inventory','Logistics','Finance') NOT NULL,
  email VARCHAR(100),
  password VARCHAR(255) NOT NULL,
  failcount INT DEFAULT 0,
  locked TINYINT(1) DEFAULT 0
);

CREATE TABLE IF NOT EXISTS customer (
  customerID VARCHAR(20) PRIMARY KEY,
  name VARCHAR(100) NOT NULL,
  contact VARCHAR(50),
  email VARCHAR(100),
  address TEXT,
  creditlimit DECIMAL(12,2) DEFAULT 0
);

CREATE TABLE IF NOT EXISTS items (
  itemID VARCHAR(20) PRIMARY KEY,
  description VARCHAR(200),
  unit VARCHAR(20),
  unitprice DECIMAL(12,2) DEFAULT 0
);

CREATE TABLE IF NOT EXISTS suppliers (
  supplierID VARCHAR(20) PRIMARY KEY,
  suppname VARCHAR(100) NOT NULL,
  contact VARCHAR(50),
  email VARCHAR(100),
  payterms VARCHAR(50),
  ontimedlvrate DECIMAL(5,2) DEFAULT 0,
  defectrate DECIMAL(5,2) DEFAULT 0
);

CREATE TABLE IF NOT EXISTS salesorders (
  orderID VARCHAR(25) PRIMARY KEY,
  customerID VARCHAR(20),
  orderdate DATE,
  status VARCHAR(30),
  deliverymethod VARCHAR(30),
  dispatchdate DATE,
  shippingaddress TEXT,
  FOREIGN KEY (customerID) REFERENCES customer(customerID)
);

CREATE TABLE IF NOT EXISTS salesorderlines (
  lineID INT AUTO_INCREMENT PRIMARY KEY,
  orderID VARCHAR(25),
  itemID VARCHAR(20),
  qty INT DEFAULT 1,
  price DECIMAL(12,2) DEFAULT 0,
  discount DECIMAL(5,2) DEFAULT 0,
  FOREIGN KEY (orderID) REFERENCES salesorders(orderID),
  FOREIGN KEY (itemID) REFERENCES items(itemID)
);

CREATE TABLE IF NOT EXISTS quotations (
  quotationID VARCHAR(25) PRIMARY KEY,
  customerID VARCHAR(20),
  quotationdate DATE,
  expirydate DATE,
  status VARCHAR(20),
  totalamt DECIMAL(12,2) DEFAULT 0,
  notes TEXT
);

CREATE TABLE IF NOT EXISTS invoices (
  invoiceID VARCHAR(25) PRIMARY KEY,
  orderID VARCHAR(25),
  customerID VARCHAR(20),
  issuedate DATE,
  duedate DATE,
  status VARCHAR(20),
  total DECIMAL(12,2) DEFAULT 0
);

CREATE TABLE IF NOT EXISTS productionorders (
  productionID VARCHAR(25) PRIMARY KEY,
  orderID VARCHAR(25),
  itemID VARCHAR(20),
  qty INT DEFAULT 0,
  startdate DATE,
  enddate DATE,
  status VARCHAR(20),
  assignedto VARCHAR(50)
);

CREATE TABLE IF NOT EXISTS materialrequests (
  mrID VARCHAR(25) PRIMARY KEY,
  productionID VARCHAR(25),
  itemID VARCHAR(20),
  qtyreq INT DEFAULT 0,
  qtyissued INT DEFAULT 0,
  status VARCHAR(20),
  remarks TEXT
);

CREATE TABLE IF NOT EXISTS inventory (
  inventoryID VARCHAR(25) PRIMARY KEY,
  itemID VARCHAR(20),
  qty INT DEFAULT 0,
  location VARCHAR(50),
  minstocklv INT DEFAULT 0,
  status VARCHAR(20)
);

CREATE TABLE IF NOT EXISTS purchaseorders (
  poid VARCHAR(25) PRIMARY KEY,
  supplierID VARCHAR(20),
  orderdate DATE,
  expecteddate DATE,
  status VARCHAR(20),
  totalcost DECIMAL(12,2) DEFAULT 0
);

CREATE TABLE IF NOT EXISTS deliverynotes (
  dnID VARCHAR(25) PRIMARY KEY,
  orderID VARCHAR(25),
  customerID VARCHAR(20),
  dndate DATE,
  dispatchdate DATE,
  deliverymethod VARCHAR(30),
  shippingaddress TEXT,
  driver VARCHAR(50),
  signedby VARCHAR(50),
  status VARCHAR(20)
);

CREATE TABLE IF NOT EXISTS shipments (
  shipmentID VARCHAR(25) PRIMARY KEY,
  orderID VARCHAR(25),
  dnID VARCHAR(25),
  dispatchdate DATE,
  driver VARCHAR(50),
  status VARCHAR(20)
);

CREATE TABLE IF NOT EXISTS complaints (
  complaintID VARCHAR(25) PRIMARY KEY,
  customerID VARCHAR(20),
  orderID VARCHAR(25),
  serialNo VARCHAR(50),
  category VARCHAR(30),
  description TEXT,
  priority VARCHAR(10),
  status VARCHAR(20),
  handledby VARCHAR(50),
  resolution TEXT
);

CREATE TABLE IF NOT EXISTS audittrail (
  auditID INT AUTO_INCREMENT PRIMARY KEY,
  userID VARCHAR(20),
  actioncode VARCHAR(10),
  module VARCHAR(50),
  recordID VARCHAR(50),
  details TEXT,
  timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Default admin account (password: Admin@1234)
INSERT IGNORE INTO users(userID,empname,role,email,password)
VALUES('USR0000001','System Admin','Admin','admin@idsms.local','Admin@1234');
