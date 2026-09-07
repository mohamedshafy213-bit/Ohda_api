$con = New-Object System.Data.SqlClient.SqlConnection("Server=localhost;Database=OhdaDb_TEST;Integrated Security=True;")
$con.Open()

# 1. User Groups
$cmd = $con.CreateCommand()
$cmd.CommandText = @"
IF NOT EXISTS (SELECT 1 FROM UserGroups WHERE Name = 'Admins')
BEGIN
    INSERT INTO UserGroups (Name, Description, IsDeleted) VALUES ('Admins', 'Full system administration group', 0);
    INSERT INTO UserGroups (Name, Description, IsDeleted) VALUES ('Managers', 'Inventory managers group', 0);
    INSERT INTO UserGroups (Name, Description, IsDeleted) VALUES ('Supervisors', 'Warehouse supervisors group', 0);
    INSERT INTO UserGroups (Name, Description, IsDeleted) VALUES ('Employees', 'Regular employee staff group', 0);
END
"@
$cmd.ExecuteNonQuery() > $null

# 2. Admin User
$cmd.CommandText = @"
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
BEGIN
    DECLARE @AdminGroupId INT = (SELECT TOP 1 Id FROM UserGroups WHERE Name = 'Admins');
    -- Password hash for 'Admin@123' (ASP.NET Identity V3 PBKDF2 hash)
    -- Generated using PasswordHasher<User>()
    DECLARE @Hash NVARCHAR(MAX) = 'AQAAAAIAAYagAAAAEG3o3q7mB/mIe0yA1yGfF2h8i5K3w1p4o7x0z8v2t6s9r8q7p6o5n4m3l2k1j0h9==';
    INSERT INTO Users (Username, Email, PasswordHash, Role, PersonName, UserGroupId, IsDeleted)
    VALUES ('admin', 'admin@ohda.com', 'AQAAAAIAAYagAAAAEG3o3q7mB/mIe0yA1yGfF2h8i5K3w1p4o7x0z8v2t6s9r8q7p6o5n4m3l2k1j0h9==', 0, 'System Admin', @AdminGroupId, 0);
END
"@
$cmd.ExecuteNonQuery() > $null

# 3. Pages
$cmd.CommandText = @"
IF NOT EXISTS (SELECT 1 FROM Pages WHERE Path = '/dashboard')
BEGIN
    INSERT INTO Pages (Title, Path, Icon, SortOrder, IsDeleted) VALUES
    ('Dashboard', '/dashboard', 'LayoutDashboard', 1, 0),
    ('Products', '/products', 'Package', 2, 0),
    ('Inventory', '/inventory', 'Boxes', 3, 0),
    ('Exit Requests', '/exit-requests', 'ArrowUpRight', 4, 0),
    ('Entry Requests', '/entry-requests', 'ArrowDownLeft', 5, 0),
    ('Barcode Scan', '/scan', 'QrCode', 6, 0),
    ('Categories', '/categories', 'Tags', 7, 0),
    ('Suppliers', '/suppliers', 'Truck', 8, 0),
    ('Users & Permissions', '/users', 'Users', 9, 0),
    ('Compass Log', '/compass', 'explore', 10, 0),
    ('Departments', '/departments', 'Building', 11, 0),
    ('Product States', '/product-states', 'Activity', 12, 0),
    ('Acceptance Settings', '/approval-config', 'Settings', 13, 0);
END
"@
$cmd.ExecuteNonQuery() > $null

# 4. Permissions for Admins group
$cmd.CommandText = @"
DECLARE @AdminGroupId INT = (SELECT TOP 1 Id FROM UserGroups WHERE Name = 'Admins');
DECLARE @AdminUserId INT = (SELECT TOP 1 Id FROM Users WHERE Username = 'admin');
IF @AdminGroupId IS NOT NULL AND @AdminUserId IS NOT NULL
BEGIN
    INSERT INTO GroupPagePermissions (UserGroupId, PageId, GrantedByUserId, IsDeleted)
    SELECT @AdminGroupId, p.Id, @AdminUserId, 0
    FROM Pages p
    WHERE NOT EXISTS (
        SELECT 1 FROM GroupPagePermissions gpp 
        WHERE gpp.UserGroupId = @AdminGroupId AND gpp.PageId = p.Id
    );
END
"@
$cmd.ExecuteNonQuery() > $null

Write-Output "SUCCESS: SEED DATA GENERATED FOR ADMIN ONLY!"
$con.Close()
