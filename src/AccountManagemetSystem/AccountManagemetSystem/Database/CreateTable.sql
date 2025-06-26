CREATE TABLE Modules (
    ModuleId INT IDENTITY(1,1) PRIMARY KEY,
    ModuleName NVARCHAR(100) NOT NULL UNIQUE,
    ModuleDescription NVARCHAR(255),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE Permissions (
    PermissionId INT IDENTITY(1,1) PRIMARY KEY,
    PermissionName NVARCHAR(100) NOT NULL,
    PermissionDescription NVARCHAR(255),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE RoleModulePermissions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RoleId NVARCHAR(450) NOT NULL, -- ASP.NET Identity Role Id
    ModuleId INT NOT NULL,
    PermissionId INT NOT NULL,
    IsGranted BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (ModuleId) REFERENCES Modules(ModuleId),
    FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId),
    CONSTRAINT UK_RoleModulePermissions UNIQUE (RoleId, ModuleId, PermissionId)
);

-- 2. Insert Initial Data
INSERT INTO Modules (ModuleName, ModuleDescription) VALUES
('UserManagement', 'User and Role Management'),
('ChartOfAccounts', 'Chart of Accounts Management'),
('VoucherEntry', 'Voucher Entry and Management'),
('Reports', 'Reports and Analytics');

INSERT INTO Permissions (PermissionName, PermissionDescription) VALUES
('View', 'View/Read access to the module'),
('Create', 'Create new records in the module'),
('Edit', 'Edit existing records in the module'),
('Delete', 'Delete records from the module'),
('Export', 'Export data from the module');