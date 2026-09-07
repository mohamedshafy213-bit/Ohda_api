IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE TABLE [Pages] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(100) NOT NULL,
        [Path] nvarchar(150) NOT NULL,
        [Icon] nvarchar(max) NULL,
        [SortOrder] int NOT NULL,
        [AllowedRoles] nvarchar(200) NOT NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_Pages] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE TABLE [SERLOGS] (
        [ID] int NOT NULL IDENTITY,
        [SERTIMESTAMP] nvarchar(100) NULL,
        [SERLEVEL] nvarchar(15) NULL,
        [SERTEMPLATE] nvarchar(max) NULL,
        [SERMESSAGE] nvarchar(max) NULL,
        [SEREXCEPTION] nvarchar(max) NULL,
        [SERPROPERTIES] nvarchar(max) NULL,
        [SERTS] datetime2 NULL DEFAULT (GETDATE()),
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_SERLOGS] PRIMARY KEY ([ID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE TABLE [Suppliers] (
        [Id] int NOT NULL IDENTITY,
        [CompanyName] nvarchar(max) NOT NULL,
        [ContactName] nvarchar(max) NULL,
        [Phone] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_Suppliers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [Username] nvarchar(100) NOT NULL,
        [Email] nvarchar(150) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Role] int NOT NULL,
        [PersonName] nvarchar(max) NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE TABLE [Products] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [SKU] nvarchar(100) NOT NULL,
        [Barcode] nvarchar(100) NOT NULL,
        [CategoryId] int NOT NULL,
        [SupplierId] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [InventoryType] int NOT NULL,
        [PurchasePrice] decimal(18,2) NULL,
        [AssetValue] decimal(18,2) NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Products_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE TABLE [Orders] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [OrderDate] datetime2 NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Orders_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE TABLE [Inventories] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [Quantity] int NOT NULL,
        [MinStock] int NOT NULL,
        [MaxStock] int NOT NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_Inventories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Inventories_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE TABLE [ProductExitRequests] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [RequestedQuantity] int NOT NULL,
        [Status] int NOT NULL,
        [RequestedByUserId] int NOT NULL,
        [SupervisorId] int NULL,
        [ManagerId] int NULL,
        [RejectionReason] nvarchar(max) NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_ProductExitRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductExitRequests_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductExitRequests_Users_ManagerId] FOREIGN KEY ([ManagerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductExitRequests_Users_RequestedByUserId] FOREIGN KEY ([RequestedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductExitRequests_Users_SupervisorId] FOREIGN KEY ([SupervisorId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE TABLE [ScanTransactions] (
        [Id] int NOT NULL IDENTITY,
        [BarcodeScanned] nvarchar(max) NOT NULL,
        [TransactionType] int NOT NULL,
        [Quantity] int NOT NULL,
        [ScannerDeviceId] nvarchar(max) NULL,
        [ProductId] int NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_ScanTransactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ScanTransactions_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE TABLE [OrderDetails] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [ProductId] int NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_OrderDetails] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderDetails_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderDetails_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AllowedRoles', N'DeleteDate', N'DeleteUserCode', N'Icon', N'InsertDate', N'InsertUserCode', N'IsDeleted', N'LastUpdate', N'Path', N'SortOrder', N'Title', N'UpdateUserCode') AND [object_id] = OBJECT_ID(N'[Pages]'))
        SET IDENTITY_INSERT [Pages] ON;
    EXEC(N'INSERT INTO [Pages] ([Id], [AllowedRoles], [DeleteDate], [DeleteUserCode], [Icon], [InsertDate], [InsertUserCode], [IsDeleted], [LastUpdate], [Path], [SortOrder], [Title], [UpdateUserCode])
    VALUES (1, N''Admin,Employee,Supervisor,Manager'', NULL, NULL, N''dashboard'', ''2026-07-23T12:00:34.6515664Z'', NULL, CAST(0 AS bit), NULL, N''/dashboard'', 1, N''Dashboard'', NULL),
    (2, N''Admin,Employee,Supervisor,Manager'', NULL, NULL, N''inventory_2'', ''2026-07-23T12:00:34.6517979Z'', NULL, CAST(0 AS bit), NULL, N''/products'', 2, N''Products'', NULL),
    (3, N''Admin,Supervisor,Manager'', NULL, NULL, N''warehouse'', ''2026-07-23T12:00:34.6517984Z'', NULL, CAST(0 AS bit), NULL, N''/inventory'', 3, N''Inventory Stock'', NULL),
    (4, N''Admin,Employee'', NULL, NULL, N''qr_code_scanner'', ''2026-07-23T12:00:34.6518048Z'', NULL, CAST(0 AS bit), NULL, N''/scan'', 4, N''Scan Barcode'', NULL),
    (5, N''Admin,Employee,Supervisor,Manager'', NULL, NULL, N''assignment_return'', ''2026-07-23T12:00:34.6518051Z'', NULL, CAST(0 AS bit), NULL, N''/exit-requests'', 5, N''Product Exit Requests'', NULL),
    (6, N''Admin,Employee'', NULL, NULL, N''shopping_cart'', ''2026-07-23T12:00:34.6518053Z'', NULL, CAST(0 AS bit), NULL, N''/orders'', 6, N''Orders'', NULL),
    (7, N''Admin,Manager'', NULL, NULL, N''category'', ''2026-07-23T12:00:34.6518055Z'', NULL, CAST(0 AS bit), NULL, N''/categories'', 7, N''Categories'', NULL),
    (8, N''Admin,Manager'', NULL, NULL, N''local_shipping'', ''2026-07-23T12:00:34.6518057Z'', NULL, CAST(0 AS bit), NULL, N''/suppliers'', 8, N''Suppliers'', NULL),
    (9, N''Admin'', NULL, NULL, N''group'', ''2026-07-23T12:00:34.6518058Z'', NULL, CAST(0 AS bit), NULL, N''/users'', 9, N''User Management'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AllowedRoles', N'DeleteDate', N'DeleteUserCode', N'Icon', N'InsertDate', N'InsertUserCode', N'IsDeleted', N'LastUpdate', N'Path', N'SortOrder', N'Title', N'UpdateUserCode') AND [object_id] = OBJECT_ID(N'[Pages]'))
        SET IDENTITY_INSERT [Pages] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE INDEX [IX_Inventories_ProductId] ON [Inventories] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE INDEX [IX_OrderDetails_OrderId] ON [OrderDetails] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE INDEX [IX_OrderDetails_ProductId] ON [OrderDetails] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE INDEX [IX_Orders_UserId] ON [Orders] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE INDEX [IX_ProductExitRequests_ManagerId] ON [ProductExitRequests] ([ManagerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE INDEX [IX_ProductExitRequests_ProductId] ON [ProductExitRequests] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE INDEX [IX_ProductExitRequests_RequestedByUserId] ON [ProductExitRequests] ([RequestedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE INDEX [IX_ProductExitRequests_SupervisorId] ON [ProductExitRequests] ([SupervisorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Products_Barcode] ON [Products] ([Barcode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Products_SKU] ON [Products] ([SKU]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE INDEX [IX_Products_SupplierId] ON [Products] ([SupplierId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE INDEX [IX_ScanTransactions_ProductId] ON [ScanTransactions] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723120035_InitialSqlServerMigration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260723120035_InitialSqlServerMigration', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    ALTER TABLE [ProductExitRequests] ADD [Purpose] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    ALTER TABLE [ProductExitRequests] ADD [RecipientDepartment] nvarchar(150) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    ALTER TABLE [ProductExitRequests] ADD [RecipientName] nvarchar(150) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Message] nvarchar(1000) NOT NULL,
        [IsRead] bit NOT NULL,
        [Type] int NOT NULL,
        [ReferenceId] int NULL,
        [ReferenceType] nvarchar(max) NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    CREATE TABLE [ProductEntryRequests] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [EnteredQuantity] int NOT NULL,
        [Status] int NOT NULL,
        [FromSource] nvarchar(200) NOT NULL,
        [InvoiceNumber] nvarchar(100) NULL,
        [Notes] nvarchar(500) NULL,
        [ReceivedByUserId] int NOT NULL,
        [SupervisorId] int NULL,
        [ManagerId] int NULL,
        [RejectionReason] nvarchar(max) NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_ProductEntryRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductEntryRequests_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductEntryRequests_Users_ManagerId] FOREIGN KEY ([ManagerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductEntryRequests_Users_ReceivedByUserId] FOREIGN KEY ([ReceivedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductEntryRequests_Users_SupervisorId] FOREIGN KEY ([SupervisorId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    CREATE TABLE [UserPagePermissions] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [PageId] int NOT NULL,
        [GrantedByUserId] int NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_UserPagePermissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserPagePermissions_Pages_PageId] FOREIGN KEY ([PageId]) REFERENCES [Pages] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserPagePermissions_Users_GrantedByUserId] FOREIGN KEY ([GrantedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserPagePermissions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T14:22:53.2477497Z''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T14:22:53.2481432Z''
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T14:22:53.2481441Z''
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T14:22:53.2481444Z''
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T14:22:53.2481446Z''
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T14:22:53.2481449Z''
    WHERE [Id] = 6;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T14:22:53.2481451Z''
    WHERE [Id] = 7;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T14:22:53.2481454Z''
    WHERE [Id] = 8;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T14:22:53.2481457Z''
    WHERE [Id] = 9;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    CREATE INDEX [IX_ProductEntryRequests_ManagerId] ON [ProductEntryRequests] ([ManagerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    CREATE INDEX [IX_ProductEntryRequests_ProductId] ON [ProductEntryRequests] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    CREATE INDEX [IX_ProductEntryRequests_ReceivedByUserId] ON [ProductEntryRequests] ([ReceivedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    CREATE INDEX [IX_ProductEntryRequests_SupervisorId] ON [ProductEntryRequests] ([SupervisorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    CREATE INDEX [IX_UserPagePermissions_GrantedByUserId] ON [UserPagePermissions] ([GrantedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    CREATE INDEX [IX_UserPagePermissions_PageId] ON [UserPagePermissions] ([PageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    CREATE INDEX [IX_UserPagePermissions_UserId] ON [UserPagePermissions] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723142254_Phase2_Extensions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260723142254_Phase2_Extensions', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723202644_GroupPagePermissions'
)
BEGIN
    ALTER TABLE [Users] ADD [UserGroupId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723202644_GroupPagePermissions'
)
BEGIN
    CREATE TABLE [UserGroups] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(250) NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_UserGroups] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723202644_GroupPagePermissions'
)
BEGIN
    CREATE TABLE [GroupPagePermissions] (
        [Id] int NOT NULL IDENTITY,
        [UserGroupId] int NOT NULL,
        [PageId] int NOT NULL,
        [GrantedByUserId] int NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_GroupPagePermissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GroupPagePermissions_Pages_PageId] FOREIGN KEY ([PageId]) REFERENCES [Pages] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_GroupPagePermissions_UserGroups_UserGroupId] FOREIGN KEY ([UserGroupId]) REFERENCES [UserGroups] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_GroupPagePermissions_Users_GrantedByUserId] FOREIGN KEY ([GrantedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723202644_GroupPagePermissions'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T20:26:43.5283175Z''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723202644_GroupPagePermissions'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T20:26:43.5285629Z''
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723202644_GroupPagePermissions'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T20:26:43.5285633Z''
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723202644_GroupPagePermissions'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T20:26:43.5285635Z''
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723202644_GroupPagePermissions'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T20:26:43.5285637Z''
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723202644_GroupPagePermissions'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T20:26:43.5285638Z''
    WHERE [Id] = 6;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723202644_GroupPagePermissions'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T20:26:43.5285640Z''
    WHERE [Id] = 7;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723202644_GroupPagePermissions'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T20:26:43.5285642Z''
    WHERE [Id] = 8;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723202644_GroupPagePermissions'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T20:26:43.5285643Z''
    WHERE [Id] = 9;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723202644_GroupPagePermissions'
)
BEGIN
    CREATE INDEX [IX_Users_UserGroupId] ON [Users] ([UserGroupId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723202644_GroupPagePermissions'
)
BEGIN
    CREATE INDEX [IX_GroupPagePermissions_GrantedByUserId] ON [GroupPagePermissions] ([GrantedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723202644_GroupPagePermissions'
)
BEGIN
    CREATE INDEX [IX_GroupPagePermissions_PageId] ON [GroupPagePermissions] ([PageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723202644_GroupPagePermissions'
)
BEGIN
    CREATE INDEX [IX_GroupPagePermissions_UserGroupId] ON [GroupPagePermissions] ([UserGroupId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723202644_GroupPagePermissions'
)
BEGIN
    ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_UserGroups_UserGroupId] FOREIGN KEY ([UserGroupId]) REFERENCES [UserGroups] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723202644_GroupPagePermissions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260723202644_GroupPagePermissions', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723205906_RemoveAllowedRolesColumn'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Pages]') AND [c].[name] = N'AllowedRoles');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Pages] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Pages] DROP COLUMN [AllowedRoles];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723205906_RemoveAllowedRolesColumn'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T20:59:05.3176619Z''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723205906_RemoveAllowedRolesColumn'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T20:59:05.3178688Z''
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723205906_RemoveAllowedRolesColumn'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T20:59:05.3178691Z''
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723205906_RemoveAllowedRolesColumn'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T20:59:05.3178693Z''
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723205906_RemoveAllowedRolesColumn'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T20:59:05.3178695Z''
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723205906_RemoveAllowedRolesColumn'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T20:59:05.3178696Z''
    WHERE [Id] = 6;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723205906_RemoveAllowedRolesColumn'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T20:59:05.3178698Z''
    WHERE [Id] = 7;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723205906_RemoveAllowedRolesColumn'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T20:59:05.3178699Z''
    WHERE [Id] = 8;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723205906_RemoveAllowedRolesColumn'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-23T20:59:05.3178700Z''
    WHERE [Id] = 9;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260723205906_RemoveAllowedRolesColumn'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260723205906_RemoveAllowedRolesColumn', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727162254_Add_Compass_And_ProductItems'
)
BEGIN
    CREATE TABLE [Compasses] (
        [Id] int NOT NULL IDENTITY,
        [SerialNumber] nvarchar(100) NOT NULL,
        [ProductName] nvarchar(200) NOT NULL,
        [RecipientName] nvarchar(150) NOT NULL,
        [Place] nvarchar(150) NOT NULL,
        [ExitDate] datetime2 NOT NULL,
        [ProductExitRequestId] int NULL,
        [Notes] nvarchar(max) NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_Compasses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Compasses_ProductExitRequests_ProductExitRequestId] FOREIGN KEY ([ProductExitRequestId]) REFERENCES [ProductExitRequests] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727162254_Add_Compass_And_ProductItems'
)
BEGIN
    CREATE TABLE [ProductItems] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [SerialNumber] nvarchar(100) NOT NULL,
        [QRCode] nvarchar(500) NOT NULL,
        [Status] int NOT NULL,
        [RecipientName] nvarchar(max) NULL,
        [Place] nvarchar(max) NULL,
        [ExitDate] datetime2 NULL,
        [ProductExitRequestId] int NULL,
        [Notes] nvarchar(max) NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_ProductItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductItems_ProductExitRequests_ProductExitRequestId] FOREIGN KEY ([ProductExitRequestId]) REFERENCES [ProductExitRequests] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_ProductItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727162254_Add_Compass_And_ProductItems'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-27T16:22:53.5261468Z''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727162254_Add_Compass_And_ProductItems'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-27T16:22:53.5263428Z''
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727162254_Add_Compass_And_ProductItems'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-27T16:22:53.5263432Z''
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727162254_Add_Compass_And_ProductItems'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-27T16:22:53.5263434Z''
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727162254_Add_Compass_And_ProductItems'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-27T16:22:53.5263436Z''
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727162254_Add_Compass_And_ProductItems'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-27T16:22:53.5263438Z''
    WHERE [Id] = 6;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727162254_Add_Compass_And_ProductItems'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-27T16:22:53.5263439Z''
    WHERE [Id] = 7;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727162254_Add_Compass_And_ProductItems'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-27T16:22:53.5263441Z''
    WHERE [Id] = 8;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727162254_Add_Compass_And_ProductItems'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-07-27T16:22:53.5263458Z''
    WHERE [Id] = 9;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727162254_Add_Compass_And_ProductItems'
)
BEGIN
    CREATE INDEX [IX_Compasses_ProductExitRequestId] ON [Compasses] ([ProductExitRequestId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727162254_Add_Compass_And_ProductItems'
)
BEGIN
    CREATE INDEX [IX_ProductItems_ProductExitRequestId] ON [ProductItems] ([ProductExitRequestId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727162254_Add_Compass_And_ProductItems'
)
BEGIN
    CREATE INDEX [IX_ProductItems_ProductId] ON [ProductItems] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727162254_Add_Compass_And_ProductItems'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProductItems_SerialNumber] ON [ProductItems] ([SerialNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260727162254_Add_Compass_And_ProductItems'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260727162254_Add_Compass_And_ProductItems', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826120241_AddApprovalConfigRules'
)
BEGIN
    CREATE TABLE [ApprovalConfigs] (
        [Id] int NOT NULL IDENTITY,
        [RequestType] int NOT NULL,
        [UserGroupId] int NOT NULL,
        [WorkflowRole] int NOT NULL,
        [IsActive] bit NOT NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_ApprovalConfigs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ApprovalConfigs_UserGroups_UserGroupId] FOREIGN KEY ([UserGroupId]) REFERENCES [UserGroups] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826120241_AddApprovalConfigRules'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-08-26T12:02:37.2237103Z''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826120241_AddApprovalConfigRules'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-08-26T12:02:37.2244449Z''
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826120241_AddApprovalConfigRules'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-08-26T12:02:37.2244461Z''
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826120241_AddApprovalConfigRules'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-08-26T12:02:37.2244467Z''
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826120241_AddApprovalConfigRules'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-08-26T12:02:37.2244472Z''
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826120241_AddApprovalConfigRules'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-08-26T12:02:37.2244477Z''
    WHERE [Id] = 6;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826120241_AddApprovalConfigRules'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-08-26T12:02:37.2244481Z''
    WHERE [Id] = 7;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826120241_AddApprovalConfigRules'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-08-26T12:02:37.2244487Z''
    WHERE [Id] = 8;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826120241_AddApprovalConfigRules'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-08-26T12:02:37.2244492Z''
    WHERE [Id] = 9;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826120241_AddApprovalConfigRules'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'DeleteDate', N'DeleteUserCode', N'Icon', N'InsertDate', N'InsertUserCode', N'IsDeleted', N'LastUpdate', N'Path', N'SortOrder', N'Title', N'UpdateUserCode') AND [object_id] = OBJECT_ID(N'[Pages]'))
        SET IDENTITY_INSERT [Pages] ON;
    EXEC(N'INSERT INTO [Pages] ([Id], [DeleteDate], [DeleteUserCode], [Icon], [InsertDate], [InsertUserCode], [IsDeleted], [LastUpdate], [Path], [SortOrder], [Title], [UpdateUserCode])
    VALUES (11, NULL, NULL, N''explore'', ''2026-08-26T12:02:37.2244496Z'', NULL, CAST(0 AS bit), NULL, N''/compass'', 10, N''Compass Log'', NULL)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'DeleteDate', N'DeleteUserCode', N'Icon', N'InsertDate', N'InsertUserCode', N'IsDeleted', N'LastUpdate', N'Path', N'SortOrder', N'Title', N'UpdateUserCode') AND [object_id] = OBJECT_ID(N'[Pages]'))
        SET IDENTITY_INSERT [Pages] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826120241_AddApprovalConfigRules'
)
BEGIN
    CREATE INDEX [IX_ApprovalConfigs_UserGroupId] ON [ApprovalConfigs] ([UserGroupId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826120241_AddApprovalConfigRules'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260826120241_AddApprovalConfigRules', N'9.0.0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    ALTER TABLE [ProductExitRequests] ADD [DepartmentId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    ALTER TABLE [ProductEntryRequests] ADD [DepartmentId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    ALTER TABLE [ProductEntryRequests] ADD [ProductStateId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    ALTER TABLE [Compasses] ADD [DepartmentId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    ALTER TABLE [Compasses] ADD [ProductEntryRequestId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    ALTER TABLE [Compasses] ADD [ProductStateId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    ALTER TABLE [Compasses] ADD [Type] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    CREATE TABLE [Departments] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_Departments] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    CREATE TABLE [ProductStates] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Code] nvarchar(max) NULL,
        [InsertUserCode] nvarchar(max) NULL,
        [InsertDate] datetime2 NULL,
        [UpdateUserCode] nvarchar(max) NULL,
        [LastUpdate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [DeleteUserCode] nvarchar(max) NULL,
        [DeleteDate] datetime2 NULL,
        CONSTRAINT [PK_ProductStates] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-08-26T13:06:22.1977685Z''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-08-26T13:06:22.1979613Z''
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-08-26T13:06:22.1979617Z''
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-08-26T13:06:22.1979619Z''
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-08-26T13:06:22.1979620Z''
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-08-26T13:06:22.1979622Z''
    WHERE [Id] = 6;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-08-26T13:06:22.1979624Z''
    WHERE [Id] = 7;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-08-26T13:06:22.1979625Z''
    WHERE [Id] = 8;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-08-26T13:06:22.1979626Z''
    WHERE [Id] = 9;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    EXEC(N'UPDATE [Pages] SET [InsertDate] = ''2026-08-26T13:06:22.1979629Z''
    WHERE [Id] = 11;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    CREATE INDEX [IX_ProductExitRequests_DepartmentId] ON [ProductExitRequests] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    CREATE INDEX [IX_ProductEntryRequests_DepartmentId] ON [ProductEntryRequests] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    CREATE INDEX [IX_ProductEntryRequests_ProductStateId] ON [ProductEntryRequests] ([ProductStateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    CREATE INDEX [IX_Compasses_DepartmentId] ON [Compasses] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    CREATE INDEX [IX_Compasses_ProductEntryRequestId] ON [Compasses] ([ProductEntryRequestId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    CREATE INDEX [IX_Compasses_ProductStateId] ON [Compasses] ([ProductStateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    ALTER TABLE [Compasses] ADD CONSTRAINT [FK_Compasses_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    ALTER TABLE [Compasses] ADD CONSTRAINT [FK_Compasses_ProductEntryRequests_ProductEntryRequestId] FOREIGN KEY ([ProductEntryRequestId]) REFERENCES [ProductEntryRequests] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    ALTER TABLE [Compasses] ADD CONSTRAINT [FK_Compasses_ProductStates_ProductStateId] FOREIGN KEY ([ProductStateId]) REFERENCES [ProductStates] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    ALTER TABLE [ProductEntryRequests] ADD CONSTRAINT [FK_ProductEntryRequests_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    ALTER TABLE [ProductEntryRequests] ADD CONSTRAINT [FK_ProductEntryRequests_ProductStates_ProductStateId] FOREIGN KEY ([ProductStateId]) REFERENCES [ProductStates] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    ALTER TABLE [ProductExitRequests] ADD CONSTRAINT [FK_ProductExitRequests_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260826130623_AddDepartmentsAndStates'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260826130623_AddDepartmentsAndStates', N'9.0.0');
END;

COMMIT;
GO

