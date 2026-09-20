-- ==============================================================================
-- Clean Seed Data Script: Keeps ONLY SuperAdmin, Pages, and SuperAdmin Permissions
-- ==============================================================================

BEGIN;

-- 1. Detach superadmin from user groups or branches
UPDATE users 
SET user_group_id = NULL, branch_id = NULL 
WHERE id = 1 OR lower(username) = 'superadmin';

-- 2. Truncate all sample transaction and entity tables
TRUNCATE TABLE 
    product_exit_request_items,
    product_exit_requests,
    product_entry_request_items,
    product_entry_requests,
    product_items,
    inventories,
    scan_transactions,
    order_details,
    orders,
    compasses,
    notifications,
    warehouse_bins,
    products,
    suppliers,
    categories,
    departments,
    product_states,
    approval_configs,
    group_page_permissions
CASCADE;

-- 3. Remove non-superadmin users and user groups
DELETE FROM user_page_permissions WHERE user_id != 1;
DELETE FROM users WHERE id != 1 AND lower(username) != 'superadmin';
DELETE FROM user_groups;

-- 4. Keep only the main system branch (needed for tenant FK on UserPagePermissions)
DELETE FROM branches WHERE id != 1 AND code != 'MAIN';

-- 5. Ensure superadmin has explicit permissions for all active pages
INSERT INTO user_page_permissions (user_id, page_id, branch_id, granted_by_user_id, is_deleted, insert_date)
SELECT 1, p.id, 1, 1, false, NOW()
FROM pages p
WHERE NOT EXISTS (
    SELECT 1 FROM user_page_permissions upp WHERE upp.user_id = 1 AND upp.page_id = p.id
);

COMMIT;
