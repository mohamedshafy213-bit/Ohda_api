-- ====================================================================
-- Migration: Add Missing Performance Indexes (Ohda Inventory System)
-- Targets: ProductItems, ProductExitRequestItems, ProductEntryRequestItems
-- Solves: Full table scans on in-stock queries, barcode lookups, and FK joins
-- ====================================================================

-- 1. Indexes for ProductItems (In-stock filtering & serial tracking)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductItems_ProductId' AND object_id = OBJECT_ID('ProductItems'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductItems_ProductId] 
    ON [ProductItems] ([ProductId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductItems_Status' AND object_id = OBJECT_ID('ProductItems'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductItems_Status] 
    ON [ProductItems] ([Status]);
END
GO

-- Composite index covering product + in-stock status (eliminates table scans on /instock queries)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductItems_ProductId_Status' AND object_id = OBJECT_ID('ProductItems'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductItems_ProductId_Status] 
    ON [ProductItems] ([ProductId], [Status])
    INCLUDE ([SerialNumber], [QRCode], [BinId], [IsDeleted]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductItems_ProductExitRequestId' AND object_id = OBJECT_ID('ProductItems'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductItems_ProductExitRequestId] 
    ON [ProductItems] ([ProductExitRequestId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductItems_BinId' AND object_id = OBJECT_ID('ProductItems'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductItems_BinId] 
    ON [ProductItems] ([BinId]);
END
GO

-- 2. Indexes for ProductExitRequestItems (Foreign Key joins)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductExitRequestItems_ProductExitRequestId' AND object_id = OBJECT_ID('ProductExitRequestItems'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductExitRequestItems_ProductExitRequestId] 
    ON [ProductExitRequestItems] ([ProductExitRequestId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductExitRequestItems_ProductId' AND object_id = OBJECT_ID('ProductExitRequestItems'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductExitRequestItems_ProductId] 
    ON [ProductExitRequestItems] ([ProductId]);
END
GO

-- 3. Indexes for ProductEntryRequestItems (Foreign Key joins)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductEntryRequestItems_ProductEntryRequestId' AND object_id = OBJECT_ID('ProductEntryRequestItems'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductEntryRequestItems_ProductEntryRequestId] 
    ON [ProductEntryRequestItems] ([ProductEntryRequestId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductEntryRequestItems_ProductId' AND object_id = OBJECT_ID('ProductEntryRequestItems'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductEntryRequestItems_ProductId] 
    ON [ProductEntryRequestItems] ([ProductId]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ProductEntryRequestItems_ProductStateId' AND object_id = OBJECT_ID('ProductEntryRequestItems'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductEntryRequestItems_ProductStateId] 
    ON [ProductEntryRequestItems] ([ProductStateId]);
END
GO
