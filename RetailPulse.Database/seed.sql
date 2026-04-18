USE RetailPulseDB;
GO

-- Categories
INSERT INTO Categories (Name, Description) VALUES
('Electronics',     'Consumer electronics and accessories'),
('Clothing',        'Apparel and fashion items'),
('Home & Kitchen',  'Household and kitchen products'),
('Sports',          'Sports equipment and accessories'),
('Books',           'Books and educational materials');
GO

-- Suppliers
INSERT INTO Suppliers (Name, ContactName, Email, Phone, Country) VALUES
('TechSource Inc.',      'James Whitfield',  'james@techsource.com',    '416-555-0101', 'Canada'),
('FashionHub Ltd.',      'Sara Mendes',      'sara@fashionhub.com',     '647-555-0202', 'Canada'),
('HomeGoods Co.',        'David Park',       'david@homegoods.com',     '905-555-0303', 'Canada'),
('SportZone Supply',     'Linda Tran',       'linda@sportzone.com',     '514-555-0404', 'Canada'),
('PageTurner Dist.',     'Ahmed Hassan',     'ahmed@pageturner.com',    '604-555-0505', 'Canada');
GO

-- Products
INSERT INTO Products (Name, SKU, CategoryId, SupplierId, UnitPrice, StockQuantity, ReorderLevel) VALUES
('Wireless Headphones',     'ELEC-001', 1, 1, 89.99,  45,  10),
('Bluetooth Speaker',       'ELEC-002', 1, 1, 59.99,  30,  10),
('USB-C Hub 7-Port',        'ELEC-003', 1, 1, 39.99,   8,  10),
('Mechanical Keyboard',     'ELEC-004', 1, 1, 129.99, 20,  10),
('Running Shoes',           'CLTH-001', 2, 2, 74.99,  60,  15),
('Winter Jacket',           'CLTH-002', 2, 2, 149.99, 25,  10),
('Cotton T-Shirt Pack',     'CLTH-003', 2, 2, 29.99,   5,  20),
('Coffee Maker',            'HOME-001', 3, 3, 69.99,  18,  10),
('Non-Stick Pan Set',       'HOME-002', 3, 3, 49.99,  35,  10),
('Air Purifier',            'HOME-003', 3, 3, 119.99,  7,  10),
('Yoga Mat',                'SPRT-001', 4, 4, 34.99,  50,  15),
('Dumbbell Set 20kg',       'SPRT-002', 4, 4, 89.99,  12,  10),
('Water Bottle 1L',         'SPRT-003', 4, 4, 19.99,   9,  20),
('Clean Code (Book)',       'BOOK-001', 5, 5, 44.99,  22,  10),
('The Pragmatic Programmer','BOOK-002', 5, 5, 49.99,  15,  10);
GO

-- Customers
INSERT INTO Customers (FirstName, LastName, Email, Phone, City) VALUES
('Emma',    'Thompson',  'emma.t@email.com',    '416-111-0001', 'Toronto'),
('Liam',    'Patel',     'liam.p@email.com',    '647-111-0002', 'Mississauga'),
('Olivia',  'Chen',      'olivia.c@email.com',  '905-111-0003', 'Brampton'),
('Noah',    'Williams',  'noah.w@email.com',    '416-111-0004', 'Toronto'),
('Ava',     'Rodriguez', 'ava.r@email.com',     '514-111-0005', 'Montreal'),
('William', 'Kim',       'william.k@email.com', '604-111-0006', 'Vancouver'),
('Sophia',  'Nguyen',    'sophia.n@email.com',  '780-111-0007', 'Edmonton'),
('James',   'Brown',     'james.b@email.com',   '403-111-0008', 'Calgary');
GO

-- Sales Orders + Items

INSERT INTO SalesOrders (CustomerId, OrderDate, Status, TotalAmount) VALUES
(1, DATEADD(day, -60, GETDATE()), 'Completed', 179.98),
(2, DATEADD(day, -52, GETDATE()), 'Completed', 129.99),
(3, DATEADD(day, -45, GETDATE()), 'Completed', 224.97),
(4, DATEADD(day, -38, GETDATE()), 'Completed', 89.99),
(5, DATEADD(day, -30, GETDATE()), 'Completed', 299.98),
(6, DATEADD(day, -22, GETDATE()), 'Completed', 164.98),
(7, DATEADD(day, -15, GETDATE()), 'Completed', 94.98),
(8, DATEADD(day, -10, GETDATE()), 'Completed', 189.98),
(1, DATEADD(day,  -7, GETDATE()), 'Pending',   149.99),
(3, DATEADD(day,  -3, GETDATE()), 'Pending',    69.99),
(2, DATEADD(day,  -1, GETDATE()), 'Pending',   219.98),
(5, GETDATE(),                    'Pending',    44.99);
GO

INSERT INTO SalesOrderItems (OrderId, ProductId, Quantity, UnitPrice) VALUES
(1,  1, 1, 89.99),
(1,  5, 1, 89.99),
(2,  4, 1, 129.99),
(3,  6, 1, 149.99),
(3,  9, 1, 49.99),
(3,  1, 1, 89.99),  -- adjusted for rounding
(4,  1, 1, 89.99),
(5,  6, 1, 149.99),
(5,  4, 1, 129.99),
(6,  8, 1, 69.99),
(6,  5, 1, 74.99),  -- adjusted
(7, 11, 1, 34.99),
(7, 13, 3, 19.99),
(8,  2, 1, 59.99),
(8, 10, 1, 119.99),
(9,  6, 1, 149.99),
(10, 8, 1, 69.99),
(11, 3, 1, 39.99),
(11, 4, 1, 129.99),  -- adjusted
(12,14, 1, 44.99);
GO

-- Restock Log
INSERT INTO RestockLog (ProductId, Quantity, RestockedAt, Notes) VALUES
(3,  50, DATEADD(day, -30, GETDATE()), 'Regular restock from TechSource'),
(7,  40, DATEADD(day, -25, GETDATE()), 'Restocked low inventory'),
(10, 30, DATEADD(day, -20, GETDATE()), 'Seasonal demand increase'),
(13, 60, DATEADD(day, -10, GETDATE()), 'Bulk order from SportZone');
GO