USE RetailPulseDB;
GO

-- ─── Roles ────────────────────────────────────────────
CREATE TABLE Roles (
    RoleId       INT IDENTITY(1,1) PRIMARY KEY,
    Name         NVARCHAR(50)  NOT NULL UNIQUE,
    Description  NVARCHAR(200)
);
GO

-- ─── Permissions ──────────────────────────────────────
CREATE TABLE Permissions (
    PermissionId  INT IDENTITY(1,1) PRIMARY KEY,
    Module        NVARCHAR(50)  NOT NULL,
    Action        NVARCHAR(50)  NOT NULL,
    Description   NVARCHAR(200),
    UNIQUE(Module, Action)
);
GO

-- ─── Role Permissions ─────────────────────────────────
CREATE TABLE RolePermissions (
    RoleId        INT NOT NULL REFERENCES Roles(RoleId),
    PermissionId  INT NOT NULL REFERENCES Permissions(PermissionId),
    PRIMARY KEY (RoleId, PermissionId)
);
GO

-- ─── Users ────────────────────────────────────────────
CREATE TABLE Users (
    UserId        INT IDENTITY(1,1) PRIMARY KEY,
    Username      NVARCHAR(100) NOT NULL UNIQUE,
    Email         NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash  NVARCHAR(255) NOT NULL,
    RoleId        INT           NOT NULL REFERENCES Roles(RoleId),
    IsActive      BIT           NOT NULL DEFAULT 1,
    CreatedAt     DATETIME2     NOT NULL DEFAULT GETDATE(),
    LastLoginAt   DATETIME2
);
GO

-- ─── Seed Roles ───────────────────────────────────────
INSERT INTO Roles (Name, Description) VALUES
('SuperAdmin', 'Full access including user and permission management'),
('Admin',      'Full access to all modules except user management'),
('Manager',    'Can add, edit and deactivate but cannot hard delete'),
('Supervisor', 'Can add and edit only'),
('Operator',   'View only access plus reports');
GO

-- ─── Seed Permissions ─────────────────────────────────
INSERT INTO Permissions (Module, Action, Description) VALUES
('Products',  'View',       'View products list and details'),
('Products',  'Add',        'Add new products'),
('Products',  'Edit',       'Edit existing products'),
('Products',  'Deactivate', 'Activate or deactivate products'),
('Products',  'Delete',     'Permanently delete products'),
('Categories','View',       'View categories'),
('Categories','Add',        'Add new categories'),
('Categories','Edit',       'Edit categories'),
('Categories','Delete',     'Delete categories'),
('Suppliers', 'View',       'View suppliers'),
('Suppliers', 'Add',        'Add new suppliers'),
('Suppliers', 'Edit',       'Edit suppliers'),
('Suppliers', 'Deactivate', 'Activate or deactivate suppliers'),
('Customers', 'View',       'View customers'),
('Customers', 'Add',        'Add new customers'),
('Customers', 'Edit',       'Edit customers'),
('Orders',    'View',       'View sales orders'),
('Orders',    'Add',        'Create new orders'),
('Orders',    'UpdateStatus','Update order status'),
('Inventory', 'View',       'View inventory levels'),
('Inventory', 'Restock',    'Restock products'),
('Reports',   'View',       'View reports and charts'),
('Reports',   'Download',   'Download CSV and PDF reports'),
('Users',     'View',       'View user list'),
('Users',     'Add',        'Add new users'),
('Users',     'Edit',       'Edit users'),
('Users',     'Deactivate', 'Activate or deactivate users');
GO

-- ─── Seed Role Permissions ────────────────────────────
-- SuperAdmin gets everything
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 1, PermissionId FROM Permissions;
GO

-- Admin gets everything except Users module
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 2, PermissionId FROM Permissions
WHERE Module != 'Users';
GO

-- Manager gets View, Add, Edit, Deactivate, Restock, Reports
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 3, PermissionId FROM Permissions
WHERE Action IN ('View', 'Add', 'Edit', 'Deactivate', 'Restock', 'UpdateStatus', 'Download');
GO

-- Supervisor gets View, Add, Edit, Reports
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 4, PermissionId FROM Permissions
WHERE Action IN ('View', 'Add', 'Edit', 'Download');
GO

-- Operator gets View and Reports only
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 5, PermissionId FROM Permissions
WHERE Action IN ('View', 'Download');
GO

-- ─── Seed Super Admin User ────────────────────────────
-- Password: Admin@123 (will be replaced by BCrypt hash in app)
INSERT INTO Users (Username, Email, PasswordHash, RoleId) VALUES
('superadmin', 'superadmin@retailpulse.com', '$2a$11$placeholder', 1);
GO