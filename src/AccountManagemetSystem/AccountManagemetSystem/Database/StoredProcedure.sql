-- ===================================
-- STORED PROCEDURES
-- ===================================

-- 1. Get All Modules
CREATE PROCEDURE sp_GetAllModules
AS
BEGIN
    SELECT ModuleId, ModuleName, ModuleDescription, IsActive
    FROM Modules
    WHERE IsActive = 1
    ORDER BY ModuleName;
END;
GO

-- 2. Get All Permissions
CREATE PROCEDURE sp_GetAllPermissions
AS
BEGIN
    SELECT PermissionId, PermissionName, PermissionDescription, IsActive
    FROM Permissions
    WHERE IsActive = 1
    ORDER BY PermissionName;
END;
GO

-- 3. Get Role Permissions for a specific role
CREATE PROCEDURE sp_GetRolePermissions
    @RoleId NVARCHAR(450)
AS
BEGIN
    SELECT 
        m.ModuleId,
        m.ModuleName,
        p.PermissionId,
        p.PermissionName,
        ISNULL(rmp.IsGranted, 0) AS IsGranted
    FROM Modules m
    CROSS JOIN Permissions p
    LEFT JOIN RoleModulePermissions rmp ON m.ModuleId = rmp.ModuleId 
        AND p.PermissionId = rmp.PermissionId 
        AND rmp.RoleId = @RoleId
    WHERE m.IsActive = 1 AND p.IsActive = 1
    ORDER BY m.ModuleName, p.PermissionName;
END;
GO

-- 4. Save Role Permissions
CREATE PROCEDURE sp_SaveRolePermissions
    @RoleId NVARCHAR(450),
    @ModuleId INT,
    @PermissionId INT,
    @IsGranted BIT
AS
BEGIN
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Check if permission already exists
        IF EXISTS (SELECT 1 FROM RoleModulePermissions 
                  WHERE RoleId = @RoleId AND ModuleId = @ModuleId AND PermissionId = @PermissionId)
        BEGIN
            -- Update existing permission
            UPDATE RoleModulePermissions 
            SET IsGranted = @IsGranted
            WHERE RoleId = @RoleId AND ModuleId = @ModuleId AND PermissionId = @PermissionId;
        END
        ELSE
        BEGIN
            -- Insert new permission
            INSERT INTO RoleModulePermissions (RoleId, ModuleId, PermissionId, IsGranted)
            VALUES (@RoleId, @ModuleId, @PermissionId, @IsGranted);
        END
        
        COMMIT TRANSACTION;
        SELECT 1 AS Success, 'Permission updated successfully' AS Message;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END;
GO

-- 5. Check User Permission
CREATE PROCEDURE sp_CheckUserPermission
    @UserId NVARCHAR(450),
    @ModuleName NVARCHAR(100),
    @PermissionName NVARCHAR(100)
AS
BEGIN
    SELECT COUNT(*) AS HasPermission
    FROM AspNetUserRoles ur
    INNER JOIN RoleModulePermissions rmp ON ur.RoleId = rmp.RoleId
    INNER JOIN Modules m ON rmp.ModuleId = m.ModuleId
    INNER JOIN Permissions p ON rmp.PermissionId = p.PermissionId
    WHERE ur.UserId = @UserId 
        AND m.ModuleName = @ModuleName 
        AND p.PermissionName = @PermissionName
        AND rmp.IsGranted = 1
        AND m.IsActive = 1 
        AND p.IsActive = 1;
END;
GO

-- 6. Get Users with Roles
CREATE PROCEDURE sp_GetUsersWithRoles
AS
BEGIN
    SELECT 
        u.Id AS UserId,
        u.UserName,
        u.Email,
        u.EmailConfirmed,
        u.LockoutEnabled,
        STRING_AGG(r.Name, ', ') AS Roles
    FROM AspNetUsers u
    LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
    LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
    GROUP BY u.Id, u.UserName, u.Email, u.EmailConfirmed, u.LockoutEnabled
    ORDER BY u.UserName;
END;
GO

-- 7. Manage User Roles
CREATE PROCEDURE sp_ManageUserRole
    @Action NVARCHAR(10), -- 'ADD' or 'REMOVE'
    @UserId NVARCHAR(450),
    @RoleId NVARCHAR(450)
AS
BEGIN
    BEGIN TRANSACTION;
    BEGIN TRY
        IF @Action = 'ADD'
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
            BEGIN
                INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);
                SELECT 1 AS Success, 'Role assigned successfully' AS Message;
            END
            ELSE
            BEGIN
                SELECT 0 AS Success, 'User already has this role' AS Message;
            END
        END
        ELSE IF @Action = 'REMOVE'
        BEGIN
            IF EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
            BEGIN
                DELETE FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId;
                SELECT 1 AS Success, 'Role removed successfully' AS Message;
            END
            ELSE
            BEGIN
                SELECT 0 AS Success, 'User does not have this role' AS Message;
            END
        END
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END;
GO

-- 8. Initialize Default Roles and Permissions
CREATE PROCEDURE sp_InitializeDefaultRolesPermissions
AS
BEGIN
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Create default roles if they don't exist
        IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Admin')
        BEGIN
            INSERT INTO AspNetRoles (Id, Name, NormalizedName) 
            VALUES (NEWID(), 'Admin', 'ADMIN');
        END

        IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Accountant')
        BEGIN
            INSERT INTO AspNetRoles (Id, Name, NormalizedName) 
            VALUES (NEWID(), 'Accountant', 'ACCOUNTANT');
        END

        IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Viewer')
        BEGIN
            INSERT INTO AspNetRoles (Id, Name, NormalizedName) 
            VALUES (NEWID(), 'Viewer', 'VIEWER');
        END

        -- Grant Admin full permissions to all modules
        DECLARE @AdminRoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Admin');
        
        INSERT INTO RoleModulePermissions (RoleId, ModuleId, PermissionId, IsGranted)
        SELECT @AdminRoleId, m.ModuleId, p.PermissionId, 1
        FROM Modules m CROSS JOIN Permissions p
        WHERE NOT EXISTS (
            SELECT 1 FROM RoleModulePermissions 
            WHERE RoleId = @AdminRoleId AND ModuleId = m.ModuleId AND PermissionId = p.PermissionId
        );

        -- Grant Accountant permissions (View, Create, Edit for ChartOfAccounts and VoucherEntry)
        DECLARE @AccountantRoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Accountant');
        
        INSERT INTO RoleModulePermissions (RoleId, ModuleId, PermissionId, IsGranted)
        SELECT @AccountantRoleId, m.ModuleId, p.PermissionId, 1
        FROM Modules m 
        CROSS JOIN Permissions p
        WHERE m.ModuleName IN ('ChartOfAccounts', 'VoucherEntry', 'Reports')
            AND p.PermissionName IN ('View', 'Create', 'Edit', 'Export')
            AND NOT EXISTS (
                SELECT 1 FROM RoleModulePermissions 
                WHERE RoleId = @AccountantRoleId AND ModuleId = m.ModuleId AND PermissionId = p.PermissionId
            );

        -- Grant Viewer only View permission to all modules
        DECLARE @ViewerRoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Viewer');
        
        INSERT INTO RoleModulePermissions (RoleId, ModuleId, PermissionId, IsGranted)
        SELECT @ViewerRoleId, m.ModuleId, p.PermissionId, 1
        FROM Modules m 
        CROSS JOIN Permissions p
        WHERE p.PermissionName = 'View'
            AND NOT EXISTS (
                SELECT 1 FROM RoleModulePermissions 
                WHERE RoleId = @ViewerRoleId AND ModuleId = m.ModuleId AND PermissionId = p.PermissionId
            );

        COMMIT TRANSACTION;
        SELECT 1 AS Success, 'Default roles and permissions initialized successfully' AS Message;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END;
GO

