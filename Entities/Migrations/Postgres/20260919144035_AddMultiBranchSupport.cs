using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Entities.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddMultiBranchSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_products_barcode",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_products_sku",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_product_items_serial_number",
                table: "product_items");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "must_change_password",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "user_page_permissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "user_groups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "suppliers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "scan_transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "product_states",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // bin_id may already exist on product_items if it was added manually — guard with IF NOT EXISTS
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                   WHERE table_name='product_items' AND column_name='bin_id') THEN
                        ALTER TABLE product_items ADD COLUMN bin_id integer;
                    END IF;
                END $$;
            ");

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "product_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "product_exit_requests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "product_exit_request_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "product_entry_requests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // bin_id may already exist on product_entry_request_items — guard with IF NOT EXISTS
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                   WHERE table_name='product_entry_request_items' AND column_name='bin_id') THEN
                        ALTER TABLE product_entry_request_items ADD COLUMN bin_id integer;
                    END IF;
                END $$;
            ");

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "product_entry_request_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "order_details",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // bin_id may already exist on inventories — guard with IF NOT EXISTS
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                                   WHERE table_name='inventories' AND column_name='bin_id') THEN
                        ALTER TABLE inventories ADD COLUMN bin_id integer;
                    END IF;
                END $$;
            ");

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "inventories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "group_page_permissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "departments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "compasses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "categories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "approval_configs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS branches (
                    id integer GENERATED BY DEFAULT AS IDENTITY,
                    name character varying(150) NOT NULL,
                    code character varying(50) NOT NULL,
                    is_active boolean NOT NULL,
                    contact_name text,
                    contact_email text,
                    contact_phone text,
                    logo_url text,
                    default_language character varying(10) NOT NULL DEFAULT 'ar',
                    currency character varying(10) NOT NULL DEFAULT 'SAR',
                    time_zone character varying(50) NOT NULL DEFAULT 'Asia/Riyadh',
                    industry_template character varying(50) NOT NULL DEFAULT 'General',
                    created_date timestamp without time zone NOT NULL,
                    suspended_date timestamp without time zone,
                    max_users integer NOT NULL,
                    max_products integer NOT NULL,
                    max_storage_mb integer NOT NULL,
                    insert_user_code text,
                    insert_date timestamp without time zone,
                    update_user_code text,
                    last_update timestamp without time zone,
                    is_deleted boolean NOT NULL,
                    delete_user_code text,
                    delete_date timestamp without time zone,
                    CONSTRAINT pk_branches PRIMARY KEY (id)
                );

                -- Seed default branch if branches table is empty
                DO $$ 
                DECLARE
                    default_branch_id integer;
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM branches LIMIT 1) THEN
                        INSERT INTO branches (id, name, code, is_active, default_language, currency, time_zone, industry_template, created_date, max_users, max_products, max_storage_mb, is_deleted)
                        OVERRIDING SYSTEM VALUE
                        VALUES (1, 'الفرع الرئيسي - Main Branch', 'MAIN', true, 'ar', 'SAR', 'Asia/Riyadh', 'General', NOW(), 100, 10000, 5120, false);
                    END IF;

                    SELECT id INTO default_branch_id FROM branches ORDER BY id ASC LIMIT 1;

                    -- Update any 0 branch_id to the default branch id
                    UPDATE approval_configs SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE categories SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE compasses SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE departments SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE group_page_permissions SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE inventories SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE notifications SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE order_details SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE orders SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE product_entry_requests SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE product_entry_request_items SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE product_exit_requests SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE product_exit_request_items SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE product_items SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE product_states SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE products SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE scan_transactions SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE suppliers SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE user_groups SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE user_page_permissions SET branch_id = default_branch_id WHERE branch_id = 0 OR branch_id IS NULL;
                    UPDATE users SET branch_id = default_branch_id WHERE branch_id = 0;
                END $$;
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS platform_audit_logs (
                    id integer GENERATED BY DEFAULT AS IDENTITY,
                    user_id integer,
                    action character varying(100) NOT NULL,
                    branch_id integer,
                    reason character varying(500),
                    timestamp timestamp without time zone NOT NULL,
                    ip_address character varying(50),
                    details text,
                    insert_user_code text,
                    insert_date timestamp without time zone,
                    update_user_code text,
                    last_update timestamp without time zone,
                    is_deleted boolean NOT NULL,
                    delete_user_code text,
                    delete_date timestamp without time zone,
                    CONSTRAINT pk_platform_audit_logs PRIMARY KEY (id),
                    CONSTRAINT fk_platform_audit_logs_branches_branch_id FOREIGN KEY (branch_id) REFERENCES branches (id) ON DELETE SET NULL,
                    CONSTRAINT fk_platform_audit_logs_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE SET NULL
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS warehouse_bins (
                    id integer GENERATED BY DEFAULT AS IDENTITY,
                    code text NOT NULL,
                    name text NOT NULL,
                    aisle text,
                    shelf text,
                    capacity integer,
                    description text,
                    is_active boolean NOT NULL,
                    department_id integer NOT NULL,
                    insert_user_code text,
                    insert_date timestamp without time zone,
                    update_user_code text,
                    last_update timestamp without time zone,
                    is_deleted boolean NOT NULL,
                    delete_user_code text,
                    delete_date timestamp without time zone,
                    branch_id integer NOT NULL DEFAULT 0,
                    CONSTRAINT pk_warehouse_bins PRIMARY KEY (id),
                    CONSTRAINT fk_warehouse_bins_branches_branch_id FOREIGN KEY (branch_id) REFERENCES branches (id) ON DELETE CASCADE,
                    CONSTRAINT fk_warehouse_bins_departments_department_id FOREIGN KEY (department_id) REFERENCES departments (id) ON DELETE CASCADE
                );

                DO $$ 
                DECLARE
                    default_branch_id integer;
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name='warehouse_bins' AND column_name='branch_id') THEN
                        ALTER TABLE warehouse_bins ADD COLUMN branch_id integer NOT NULL DEFAULT 0;
                    END IF;

                    SELECT id INTO default_branch_id FROM branches ORDER BY id ASC LIMIT 1;
                    IF default_branch_id IS NOT NULL THEN
                        EXECUTE 'UPDATE warehouse_bins SET branch_id = ' || default_branch_id || ' WHERE branch_id = 0 OR branch_id IS NULL';
                    END IF;
                END $$;
            ");

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 1,
                column: "insert_date",
                value: new DateTime(2026, 9, 19, 14, 40, 33, 529, DateTimeKind.Utc).AddTicks(3469));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 2,
                column: "insert_date",
                value: new DateTime(2026, 9, 19, 14, 40, 33, 529, DateTimeKind.Utc).AddTicks(5671));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 3,
                column: "insert_date",
                value: new DateTime(2026, 9, 19, 14, 40, 33, 529, DateTimeKind.Utc).AddTicks(5675));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 4,
                column: "insert_date",
                value: new DateTime(2026, 9, 19, 14, 40, 33, 529, DateTimeKind.Utc).AddTicks(5677));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 5,
                column: "insert_date",
                value: new DateTime(2026, 9, 19, 14, 40, 33, 529, DateTimeKind.Utc).AddTicks(5678));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 6,
                column: "insert_date",
                value: new DateTime(2026, 9, 19, 14, 40, 33, 529, DateTimeKind.Utc).AddTicks(5680));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 7,
                column: "insert_date",
                value: new DateTime(2026, 9, 19, 14, 40, 33, 529, DateTimeKind.Utc).AddTicks(5681));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 8,
                column: "insert_date",
                value: new DateTime(2026, 9, 19, 14, 40, 33, 529, DateTimeKind.Utc).AddTicks(5683));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 9,
                column: "insert_date",
                value: new DateTime(2026, 9, 19, 14, 40, 33, 529, DateTimeKind.Utc).AddTicks(5684));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 11,
                column: "insert_date",
                value: new DateTime(2026, 9, 19, 14, 40, 33, 529, DateTimeKind.Utc).AddTicks(5686));

            migrationBuilder.CreateIndex(
                name: "ix_users_branch_id",
                table: "users",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_page_permissions_branch_id",
                table: "user_page_permissions",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_groups_branch_id",
                table: "user_groups",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_branch_id",
                table: "suppliers",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_scan_transactions_branch_id",
                table: "scan_transactions",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_branch_id",
                table: "products",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_branch_id_barcode",
                table: "products",
                columns: new[] { "branch_id", "barcode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_branch_id_sku",
                table: "products",
                columns: new[] { "branch_id", "sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_states_branch_id",
                table: "product_states",
                column: "branch_id");

            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE tablename='product_items' AND indexname='ix_product_items_bin_id') THEN
                        CREATE INDEX ix_product_items_bin_id ON product_items (bin_id);
                    END IF;
                END $$;
            ");

            migrationBuilder.CreateIndex(
                name: "ix_product_items_branch_id",
                table: "product_items",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_items_branch_id_serial_number",
                table: "product_items",
                columns: new[] { "branch_id", "serial_number" },
                unique: true);

            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE tablename='product_items' AND indexname='ix_product_items_is_deleted') THEN
                        CREATE INDEX ix_product_items_is_deleted ON product_items (is_deleted);
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE tablename='product_items' AND indexname='ix_product_items_product_id_status') THEN
                        CREATE INDEX ix_product_items_product_id_status ON product_items (product_id, status);
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE tablename='product_items' AND indexname='ix_product_items_status') THEN
                        CREATE INDEX ix_product_items_status ON product_items (status);
                    END IF;
                END $$;
            ");

            migrationBuilder.CreateIndex(
                name: "ix_product_exit_requests_branch_id",
                table: "product_exit_requests",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_exit_request_items_branch_id",
                table: "product_exit_request_items",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_entry_requests_branch_id",
                table: "product_entry_requests",
                column: "branch_id");

            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE tablename='product_entry_request_items' AND indexname='ix_product_entry_request_items_bin_id') THEN
                        CREATE INDEX ix_product_entry_request_items_bin_id ON product_entry_request_items (bin_id);
                    END IF;
                END $$;
            ");

            migrationBuilder.CreateIndex(
                name: "ix_product_entry_request_items_branch_id",
                table: "product_entry_request_items",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_branch_id",
                table: "orders",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_details_branch_id",
                table: "order_details",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_branch_id",
                table: "notifications",
                column: "branch_id");

            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_indexes WHERE tablename='inventories' AND indexname='ix_inventories_bin_id') THEN
                        CREATE INDEX ix_inventories_bin_id ON inventories (bin_id);
                    END IF;
                END $$;
            ");

            migrationBuilder.CreateIndex(
                name: "ix_inventories_branch_id",
                table: "inventories",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_page_permissions_branch_id",
                table: "group_page_permissions",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_departments_branch_id",
                table: "departments",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_compasses_branch_id",
                table: "compasses",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_branch_id",
                table: "categories",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_approval_configs_branch_id",
                table: "approval_configs",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_branches_code",
                table: "branches",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_branches_is_active",
                table: "branches",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_branches_is_deleted",
                table: "branches",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_platform_audit_logs_branch_id",
                table: "platform_audit_logs",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_platform_audit_logs_timestamp",
                table: "platform_audit_logs",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_platform_audit_logs_user_id",
                table: "platform_audit_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_warehouse_bins_branch_id",
                table: "warehouse_bins",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_warehouse_bins_department_id",
                table: "warehouse_bins",
                column: "department_id");

            migrationBuilder.AddForeignKey(
                name: "fk_approval_configs_branches_branch_id",
                table: "approval_configs",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_categories_branches_branch_id",
                table: "categories",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_compasses_branches_branch_id",
                table: "compasses",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_departments_branches_branch_id",
                table: "departments",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_group_page_permissions_branches_branch_id",
                table: "group_page_permissions",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_inventories_branches_branch_id",
                table: "inventories",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_inventories_warehouse_bins_bin_id",
                table: "inventories",
                column: "bin_id",
                principalTable: "warehouse_bins",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_branches_branch_id",
                table: "notifications",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_order_details_branches_branch_id",
                table: "order_details",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_orders_branches_branch_id",
                table: "orders",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_product_entry_request_items_branches_branch_id",
                table: "product_entry_request_items",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_product_entry_request_items_warehouse_bins_bin_id",
                table: "product_entry_request_items",
                column: "bin_id",
                principalTable: "warehouse_bins",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_product_entry_requests_branches_branch_id",
                table: "product_entry_requests",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_product_exit_request_items_branches_branch_id",
                table: "product_exit_request_items",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_product_exit_requests_branches_branch_id",
                table: "product_exit_requests",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_product_items_branches_branch_id",
                table: "product_items",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_product_items_warehouse_bins_bin_id",
                table: "product_items",
                column: "bin_id",
                principalTable: "warehouse_bins",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_product_states_branches_branch_id",
                table: "product_states",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_products_branches_branch_id",
                table: "products",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_scan_transactions_branches_branch_id",
                table: "scan_transactions",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_suppliers_branches_branch_id",
                table: "suppliers",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_groups_branches_branch_id",
                table: "user_groups",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_page_permissions_branches_branch_id",
                table: "user_page_permissions",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_users_branches_branch_id",
                table: "users",
                column: "branch_id",
                principalTable: "branches",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_approval_configs_branches_branch_id",
                table: "approval_configs");

            migrationBuilder.DropForeignKey(
                name: "fk_categories_branches_branch_id",
                table: "categories");

            migrationBuilder.DropForeignKey(
                name: "fk_compasses_branches_branch_id",
                table: "compasses");

            migrationBuilder.DropForeignKey(
                name: "fk_departments_branches_branch_id",
                table: "departments");

            migrationBuilder.DropForeignKey(
                name: "fk_group_page_permissions_branches_branch_id",
                table: "group_page_permissions");

            migrationBuilder.DropForeignKey(
                name: "fk_inventories_branches_branch_id",
                table: "inventories");

            migrationBuilder.DropForeignKey(
                name: "fk_inventories_warehouse_bins_bin_id",
                table: "inventories");

            migrationBuilder.DropForeignKey(
                name: "fk_notifications_branches_branch_id",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "fk_order_details_branches_branch_id",
                table: "order_details");

            migrationBuilder.DropForeignKey(
                name: "fk_orders_branches_branch_id",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "fk_product_entry_request_items_branches_branch_id",
                table: "product_entry_request_items");

            migrationBuilder.DropForeignKey(
                name: "fk_product_entry_request_items_warehouse_bins_bin_id",
                table: "product_entry_request_items");

            migrationBuilder.DropForeignKey(
                name: "fk_product_entry_requests_branches_branch_id",
                table: "product_entry_requests");

            migrationBuilder.DropForeignKey(
                name: "fk_product_exit_request_items_branches_branch_id",
                table: "product_exit_request_items");

            migrationBuilder.DropForeignKey(
                name: "fk_product_exit_requests_branches_branch_id",
                table: "product_exit_requests");

            migrationBuilder.DropForeignKey(
                name: "fk_product_items_branches_branch_id",
                table: "product_items");

            migrationBuilder.DropForeignKey(
                name: "fk_product_items_warehouse_bins_bin_id",
                table: "product_items");

            migrationBuilder.DropForeignKey(
                name: "fk_product_states_branches_branch_id",
                table: "product_states");

            migrationBuilder.DropForeignKey(
                name: "fk_products_branches_branch_id",
                table: "products");

            migrationBuilder.DropForeignKey(
                name: "fk_scan_transactions_branches_branch_id",
                table: "scan_transactions");

            migrationBuilder.DropForeignKey(
                name: "fk_suppliers_branches_branch_id",
                table: "suppliers");

            migrationBuilder.DropForeignKey(
                name: "fk_user_groups_branches_branch_id",
                table: "user_groups");

            migrationBuilder.DropForeignKey(
                name: "fk_user_page_permissions_branches_branch_id",
                table: "user_page_permissions");

            migrationBuilder.DropForeignKey(
                name: "fk_users_branches_branch_id",
                table: "users");

            migrationBuilder.DropTable(
                name: "platform_audit_logs");

            migrationBuilder.DropTable(
                name: "warehouse_bins");

            migrationBuilder.DropTable(
                name: "branches");

            migrationBuilder.DropIndex(
                name: "ix_users_branch_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_user_page_permissions_branch_id",
                table: "user_page_permissions");

            migrationBuilder.DropIndex(
                name: "ix_user_groups_branch_id",
                table: "user_groups");

            migrationBuilder.DropIndex(
                name: "ix_suppliers_branch_id",
                table: "suppliers");

            migrationBuilder.DropIndex(
                name: "ix_scan_transactions_branch_id",
                table: "scan_transactions");

            migrationBuilder.DropIndex(
                name: "ix_products_branch_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_products_branch_id_barcode",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_products_branch_id_sku",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_product_states_branch_id",
                table: "product_states");

            migrationBuilder.DropIndex(
                name: "ix_product_items_bin_id",
                table: "product_items");

            migrationBuilder.DropIndex(
                name: "ix_product_items_branch_id",
                table: "product_items");

            migrationBuilder.DropIndex(
                name: "ix_product_items_branch_id_serial_number",
                table: "product_items");

            migrationBuilder.DropIndex(
                name: "ix_product_items_is_deleted",
                table: "product_items");

            migrationBuilder.DropIndex(
                name: "ix_product_items_product_id_status",
                table: "product_items");

            migrationBuilder.DropIndex(
                name: "ix_product_items_status",
                table: "product_items");

            migrationBuilder.DropIndex(
                name: "ix_product_exit_requests_branch_id",
                table: "product_exit_requests");

            migrationBuilder.DropIndex(
                name: "ix_product_exit_request_items_branch_id",
                table: "product_exit_request_items");

            migrationBuilder.DropIndex(
                name: "ix_product_entry_requests_branch_id",
                table: "product_entry_requests");

            migrationBuilder.DropIndex(
                name: "ix_product_entry_request_items_bin_id",
                table: "product_entry_request_items");

            migrationBuilder.DropIndex(
                name: "ix_product_entry_request_items_branch_id",
                table: "product_entry_request_items");

            migrationBuilder.DropIndex(
                name: "ix_orders_branch_id",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_order_details_branch_id",
                table: "order_details");

            migrationBuilder.DropIndex(
                name: "ix_notifications_branch_id",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_inventories_bin_id",
                table: "inventories");

            migrationBuilder.DropIndex(
                name: "ix_inventories_branch_id",
                table: "inventories");

            migrationBuilder.DropIndex(
                name: "ix_group_page_permissions_branch_id",
                table: "group_page_permissions");

            migrationBuilder.DropIndex(
                name: "ix_departments_branch_id",
                table: "departments");

            migrationBuilder.DropIndex(
                name: "ix_compasses_branch_id",
                table: "compasses");

            migrationBuilder.DropIndex(
                name: "ix_categories_branch_id",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "ix_approval_configs_branch_id",
                table: "approval_configs");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "must_change_password",
                table: "users");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "user_page_permissions");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "user_groups");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "scan_transactions");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "product_states");

            migrationBuilder.DropColumn(
                name: "bin_id",
                table: "product_items");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "product_items");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "product_exit_requests");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "product_exit_request_items");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "product_entry_requests");

            migrationBuilder.DropColumn(
                name: "bin_id",
                table: "product_entry_request_items");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "product_entry_request_items");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "order_details");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "bin_id",
                table: "inventories");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "inventories");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "group_page_permissions");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "departments");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "compasses");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "approval_configs");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 1,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 10, 36, 22, 283, DateTimeKind.Utc).AddTicks(4976));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 2,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 10, 36, 22, 283, DateTimeKind.Utc).AddTicks(8690));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 3,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 10, 36, 22, 283, DateTimeKind.Utc).AddTicks(8692));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 4,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 10, 36, 22, 283, DateTimeKind.Utc).AddTicks(8693));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 5,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 10, 36, 22, 283, DateTimeKind.Utc).AddTicks(8694));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 6,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 10, 36, 22, 283, DateTimeKind.Utc).AddTicks(8695));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 7,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 10, 36, 22, 283, DateTimeKind.Utc).AddTicks(8696));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 8,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 10, 36, 22, 283, DateTimeKind.Utc).AddTicks(8697));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 9,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 10, 36, 22, 283, DateTimeKind.Utc).AddTicks(8698));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 11,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 10, 36, 22, 283, DateTimeKind.Utc).AddTicks(8698));

            migrationBuilder.CreateIndex(
                name: "ix_products_barcode",
                table: "products",
                column: "barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_sku",
                table: "products",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_items_serial_number",
                table: "product_items",
                column: "serial_number",
                unique: true);
        }
    }
}
