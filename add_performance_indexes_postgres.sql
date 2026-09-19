-- ====================================================================
-- PostgreSQL Migration: Add Missing Performance Indexes (Ohda Inventory)
-- ====================================================================

CREATE INDEX IF NOT EXISTS ix_product_items_product_id ON "ProductItems" ("ProductId");
CREATE INDEX IF NOT EXISTS ix_product_items_status ON "ProductItems" ("Status");
CREATE INDEX IF NOT EXISTS ix_product_items_product_id_status ON "ProductItems" ("ProductId", "Status");
CREATE INDEX IF NOT EXISTS ix_product_items_product_exit_request_id ON "ProductItems" ("ProductExitRequestId");
CREATE INDEX IF NOT EXISTS ix_product_items_bin_id ON "ProductItems" ("BinId");
CREATE INDEX IF NOT EXISTS ix_product_items_is_deleted ON "ProductItems" ("IsDeleted");

CREATE INDEX IF NOT EXISTS ix_product_exit_request_items_request_id ON "ProductExitRequestItems" ("ProductExitRequestId");
CREATE INDEX IF NOT EXISTS ix_product_exit_request_items_product_id ON "ProductExitRequestItems" ("ProductId");

CREATE INDEX IF NOT EXISTS ix_product_entry_request_items_request_id ON "ProductEntryRequestItems" ("ProductEntryRequestId");
CREATE INDEX IF NOT EXISTS ix_product_entry_request_items_product_id ON "ProductEntryRequestItems" ("ProductId");
CREATE INDEX IF NOT EXISTS ix_product_entry_request_items_state_id ON "ProductEntryRequestItems" ("ProductStateId");
