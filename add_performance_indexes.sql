-- ====================================================================
-- Consolidated High-Performance Indexes for Multi-Tenant Ohda System
-- Architecture: BranchId leading column + Soft Delete coverage
-- Solves: Eliminates table scans, enables direct index seeks on tenant queries
-- ====================================================================

-- 1. ProductItems (Composite Tenant + Status + Product lookup)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductItems_BranchId_Status_ProductId' AND object_id = OBJECT_ID('ProductItems'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductItems_BranchId_Status_ProductId] 
    ON [ProductItems] ([BranchId], [Status], [ProductId])
    INCLUDE ([SerialNumber], [QRCode], [BinId], [IsDeleted], [ProductExitRequestId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductItems_BinId' AND object_id = OBJECT_ID('ProductItems'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductItems_BinId] 
    ON [ProductItems] ([BinId])
    WHERE [IsDeleted] = 0;
END
GO

-- 2. ProductExitRequests & Items
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductExitRequests_BranchId_IsDeleted' AND object_id = OBJECT_ID('ProductExitRequests'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductExitRequests_BranchId_IsDeleted] 
    ON [ProductExitRequests] ([BranchId], [IsDeleted])
    INCLUDE ([RequestNumber], [RequestDate], [Status], [DepartmentId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductExitRequestItems_ProductExitRequestId' AND object_id = OBJECT_ID('ProductExitRequestItems'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductExitRequestItems_ProductExitRequestId] 
    ON [ProductExitRequestItems] ([ProductExitRequestId])
    INCLUDE ([ProductId], [Quantity], [IsDeleted]);
END
GO

-- 3. ProductEntryRequests & Items
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductEntryRequests_BranchId_IsDeleted' AND object_id = OBJECT_ID('ProductEntryRequests'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductEntryRequests_BranchId_IsDeleted] 
    ON [ProductEntryRequests] ([BranchId], [IsDeleted])
    INCLUDE ([RequestNumber], [RequestDate], [Status], [SupplierId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductEntryRequestItems_ProductEntryRequestId' AND object_id = OBJECT_ID('ProductEntryRequestItems'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductEntryRequestItems_ProductEntryRequestId] 
    ON [ProductEntryRequestItems] ([ProductEntryRequestId])
    INCLUDE ([ProductId], [Quantity], [ProductStateId], [IsDeleted]);
END
GO

-- 4. Products & Categories
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_BranchId_IsDeleted' AND object_id = OBJECT_ID('Products'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Products_BranchId_IsDeleted] 
    ON [Products] ([BranchId], [IsDeleted])
    INCLUDE ([Name], [SKU], [Barcode], [CategoryId]);
END
GO

-- 5. Notifications
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Notifications_BranchId_UserId_IsRead' AND object_id = OBJECT_ID('Notifications'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Notifications_BranchId_UserId_IsRead] 
    ON [Notifications] ([BranchId], [UserId], [IsRead])
    INCLUDE ([Title], [Message], [InsertDate], [IsDeleted]);
END
GO

-- 6. WarehouseBins
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WarehouseBins_BranchId_IsDeleted' AND object_id = OBJECT_ID('WarehouseBins'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_WarehouseBins_BranchId_IsDeleted] 
    ON [WarehouseBins] ([BranchId], [IsDeleted])
    INCLUDE ([Name], [Code], [IsActive]);
END
GO
