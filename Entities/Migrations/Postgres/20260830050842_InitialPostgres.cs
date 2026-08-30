using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Entities.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_departments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    path = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    icon = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_states",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: true),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_states", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "SERLOGS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SERTIMESTAMP = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SERLEVEL = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    SERTEMPLATE = table.Column<string>(type: "text", nullable: true),
                    SERMESSAGE = table.Column<string>(type: "text", nullable: true),
                    SEREXCEPTION = table.Column<string>(type: "text", nullable: true),
                    SERPROPERTIES = table.Column<string>(type: "text", nullable: true),
                    SERTS = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "NOW()"),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_serlogs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    company_name = table.Column<string>(type: "text", nullable: false),
                    contact_name = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_suppliers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_groups",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    barcode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    supplier_id = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    inventory_type = table.Column<int>(type: "integer", nullable: false),
                    purchase_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    asset_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                    table.ForeignKey(
                        name: "fk_products_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_products_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "approval_configs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    request_type = table.Column<int>(type: "integer", nullable: false),
                    user_group_id = table.Column<int>(type: "integer", nullable: false),
                    workflow_role = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_approval_configs", x => x.id);
                    table.ForeignKey(
                        name: "fk_approval_configs_user_groups_user_group_id",
                        column: x => x.user_group_id,
                        principalTable: "user_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    person_name = table.Column<string>(type: "text", nullable: true),
                    user_group_id = table.Column<int>(type: "integer", nullable: true),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_user_groups_user_group_id",
                        column: x => x.user_group_id,
                        principalTable: "user_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "inventories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    min_stock = table.Column<int>(type: "integer", nullable: false),
                    max_stock = table.Column<int>(type: "integer", nullable: false),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventories", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventories_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scan_transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    barcode_scanned = table.Column<string>(type: "text", nullable: false),
                    transaction_type = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    scanner_device_id = table.Column<string>(type: "text", nullable: true),
                    product_id = table.Column<int>(type: "integer", nullable: true),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scan_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_scan_transactions_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "group_page_permissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_group_id = table.Column<int>(type: "integer", nullable: false),
                    page_id = table.Column<int>(type: "integer", nullable: false),
                    granted_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_page_permissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_group_page_permissions_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_group_page_permissions_user_groups_user_group_id",
                        column: x => x.user_group_id,
                        principalTable: "user_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_group_page_permissions_users_granted_by_user_id",
                        column: x => x.granted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    reference_id = table.Column<int>(type: "integer", nullable: true),
                    reference_type = table.Column<string>(type: "text", nullable: true),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    order_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_orders_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_entry_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    entered_quantity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    from_source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: true),
                    product_state_id = table.Column<int>(type: "integer", nullable: true),
                    invoice_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    received_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    supervisor_id = table.Column<int>(type: "integer", nullable: true),
                    manager_id = table.Column<int>(type: "integer", nullable: true),
                    rejection_reason = table.Column<string>(type: "text", nullable: true),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_entry_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_entry_requests_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_product_entry_requests_product_states_product_state_id",
                        column: x => x.product_state_id,
                        principalTable: "product_states",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_product_entry_requests_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_entry_requests_users_manager_id",
                        column: x => x.manager_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_entry_requests_users_received_by_user_id",
                        column: x => x.received_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_entry_requests_users_supervisor_id",
                        column: x => x.supervisor_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_exit_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    requested_quantity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    recipient_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    recipient_department = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    department_id = table.Column<int>(type: "integer", nullable: true),
                    purpose = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    requested_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    supervisor_id = table.Column<int>(type: "integer", nullable: true),
                    manager_id = table.Column<int>(type: "integer", nullable: true),
                    rejection_reason = table.Column<string>(type: "text", nullable: true),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_exit_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_exit_requests_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_product_exit_requests_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_exit_requests_users_manager_id",
                        column: x => x.manager_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_exit_requests_users_requested_by_user_id",
                        column: x => x.requested_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_exit_requests_users_supervisor_id",
                        column: x => x.supervisor_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_page_permissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    page_id = table.Column<int>(type: "integer", nullable: false),
                    granted_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_page_permissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_page_permissions_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_page_permissions_users_granted_by_user_id",
                        column: x => x.granted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_page_permissions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_details",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_details_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_order_details_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compasses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    recipient_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    place = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    exit_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: true),
                    product_state_id = table.Column<int>(type: "integer", nullable: true),
                    product_exit_request_id = table.Column<int>(type: "integer", nullable: true),
                    product_entry_request_id = table.Column<int>(type: "integer", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compasses", x => x.id);
                    table.ForeignKey(
                        name: "fk_compasses_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_compasses_product_entry_requests_product_entry_request_id",
                        column: x => x.product_entry_request_id,
                        principalTable: "product_entry_requests",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_compasses_product_exit_requests_product_exit_request_id",
                        column: x => x.product_exit_request_id,
                        principalTable: "product_exit_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_compasses_product_states_product_state_id",
                        column: x => x.product_state_id,
                        principalTable: "product_states",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "product_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    qr_code = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    recipient_name = table.Column<string>(type: "text", nullable: true),
                    place = table.Column<string>(type: "text", nullable: true),
                    exit_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    product_exit_request_id = table.Column<int>(type: "integer", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    insert_user_code = table.Column<string>(type: "text", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    update_user_code = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    delete_user_code = table.Column<string>(type: "text", nullable: true),
                    delete_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_items_product_exit_requests_product_exit_request_id",
                        column: x => x.product_exit_request_id,
                        principalTable: "product_exit_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_product_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "pages",
                columns: new[] { "id", "delete_date", "delete_user_code", "icon", "insert_date", "insert_user_code", "is_deleted", "last_update", "path", "sort_order", "title", "update_user_code" },
                values: new object[,]
                {
                    { 1, null, null, "dashboard", new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(3760), null, false, null, "/dashboard", 1, "Dashboard", null },
                    { 2, null, null, "inventory_2", new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(7685), null, false, null, "/products", 2, "Products", null },
                    { 3, null, null, "warehouse", new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(7687), null, false, null, "/inventory", 3, "Inventory Stock", null },
                    { 4, null, null, "qr_code_scanner", new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(7688), null, false, null, "/scan", 4, "Scan Barcode", null },
                    { 5, null, null, "assignment_return", new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(7689), null, false, null, "/exit-requests", 5, "Product Exit Requests", null },
                    { 6, null, null, "shopping_cart", new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(7690), null, false, null, "/orders", 6, "Orders", null },
                    { 7, null, null, "category", new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(7691), null, false, null, "/categories", 7, "Categories", null },
                    { 8, null, null, "local_shipping", new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(7692), null, false, null, "/suppliers", 8, "Suppliers", null },
                    { 9, null, null, "group", new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(7693), null, false, null, "/users", 9, "User Management", null },
                    { 11, null, null, "explore", new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(7694), null, false, null, "/compass", 10, "Compass Log", null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_approval_configs_user_group_id",
                table: "approval_configs",
                column: "user_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_compasses_department_id",
                table: "compasses",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_compasses_product_entry_request_id",
                table: "compasses",
                column: "product_entry_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_compasses_product_exit_request_id",
                table: "compasses",
                column: "product_exit_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_compasses_product_state_id",
                table: "compasses",
                column: "product_state_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_page_permissions_granted_by_user_id",
                table: "group_page_permissions",
                column: "granted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_page_permissions_page_id",
                table: "group_page_permissions",
                column: "page_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_page_permissions_user_group_id",
                table: "group_page_permissions",
                column: "user_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventories_product_id",
                table: "inventories",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_details_order_id",
                table: "order_details",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_details_product_id",
                table: "order_details",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_user_id",
                table: "orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_entry_requests_department_id",
                table: "product_entry_requests",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_entry_requests_manager_id",
                table: "product_entry_requests",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_entry_requests_product_id",
                table: "product_entry_requests",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_entry_requests_product_state_id",
                table: "product_entry_requests",
                column: "product_state_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_entry_requests_received_by_user_id",
                table: "product_entry_requests",
                column: "received_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_entry_requests_supervisor_id",
                table: "product_entry_requests",
                column: "supervisor_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_exit_requests_department_id",
                table: "product_exit_requests",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_exit_requests_manager_id",
                table: "product_exit_requests",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_exit_requests_product_id",
                table: "product_exit_requests",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_exit_requests_requested_by_user_id",
                table: "product_exit_requests",
                column: "requested_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_exit_requests_supervisor_id",
                table: "product_exit_requests",
                column: "supervisor_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_items_product_exit_request_id",
                table: "product_items",
                column: "product_exit_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_items_product_id",
                table: "product_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_items_serial_number",
                table: "product_items",
                column: "serial_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_barcode",
                table: "products",
                column: "barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_sku",
                table: "products",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_supplier_id",
                table: "products",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_scan_transactions_product_id",
                table: "scan_transactions",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_page_permissions_granted_by_user_id",
                table: "user_page_permissions",
                column: "granted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_page_permissions_page_id",
                table: "user_page_permissions",
                column: "page_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_page_permissions_user_id",
                table: "user_page_permissions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_user_group_id",
                table: "users",
                column: "user_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "approval_configs");

            migrationBuilder.DropTable(
                name: "compasses");

            migrationBuilder.DropTable(
                name: "group_page_permissions");

            migrationBuilder.DropTable(
                name: "inventories");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "order_details");

            migrationBuilder.DropTable(
                name: "product_items");

            migrationBuilder.DropTable(
                name: "scan_transactions");

            migrationBuilder.DropTable(
                name: "SERLOGS");

            migrationBuilder.DropTable(
                name: "user_page_permissions");

            migrationBuilder.DropTable(
                name: "product_entry_requests");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "product_exit_requests");

            migrationBuilder.DropTable(
                name: "pages");

            migrationBuilder.DropTable(
                name: "product_states");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DropTable(
                name: "user_groups");
        }
    }
}
