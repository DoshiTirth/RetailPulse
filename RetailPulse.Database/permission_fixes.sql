USE RetailPulseDB;

-- Remove ALL Users module permissions from Operator (role 5)
DELETE FROM RolePermissions 
WHERE RoleId = 5 
AND PermissionId IN (
    SELECT PermissionId FROM Permissions WHERE Module = 'Users'
);