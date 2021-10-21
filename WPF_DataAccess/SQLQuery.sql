use SQLServDB;
--use SQLServerDB;

CREATE TABLE Customers (
ID INT primary key identity(1,1) not null,
FirstName NVARCHAR(30) not null,
LastName NVARCHAR(30) not null,
PhoneNumber INT,
Email NVARCHAR (30) not null
)

--DROP TABLE Customers;

SELECT * FROM Customers;

----------------------------------

use AccessDB;

CREATE TABLE Products (
ID INT primary key identity(1,1) not null,
EmailCustomer NVARCHAR(30) not null,
ProductName NVARCHAR (30) not null,
ProductCode NVARCHAR(30) not null
)

--DROP TABLE Products;

SELECT * FROM Products;
