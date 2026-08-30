using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Entities.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class UpdateRequestToMultiItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_product_entry_requests_products_product_id",
                table: "product_entry_requests");

            migrationBuilder.DropForeignKey(
                name: "fk_product_exit_requests_products_product_id",
                table: "product_exit_requests");

            migrationBuilder.DropIndex(
                name: "ix_product_exit_requests_product_id",
                table: "product_exit_requests");

            migrationBuilder.DropIndex(
                name: "ix_product_entry_requests_product_id",
                table: "product_entry_requests");

            migrationBuilder.DropColumn(
                name: "product_id",
                table: "product_exit_requests");

            migrationBuilder.DropColumn(
                name: "requested_quantity",
                table: "product_exit_requests");

            migrationBuilder.DropColumn(
                name: "entered_quantity",
                table: "product_entry_requests");

            migrationBuilder.DropColumn(
                name: "product_id",
                table: "product_entry_requests");

            migrationBuilder.CreateTable(
                name: "product_entry_request_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_entry_request_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    product_state_id = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_product_entry_request_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_entry_request_items_product_entry_requests_product_",
                        column: x => x.product_entry_request_id,
                        principalTable: "product_entry_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_product_entry_request_items_product_states_product_state_id",
                        column: x => x.product_state_id,
                        principalTable: "product_states",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_entry_request_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_exit_request_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_exit_request_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_product_exit_request_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_exit_request_items_product_exit_requests_product_ex",
                        column: x => x.product_exit_request_id,
                        principalTable: "product_exit_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_product_exit_request_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 1,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 6, 3, 17, 520, DateTimeKind.Utc).AddTicks(349));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 2,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 6, 3, 17, 520, DateTimeKind.Utc).AddTicks(934));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 3,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 6, 3, 17, 520, DateTimeKind.Utc).AddTicks(936));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 4,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 6, 3, 17, 520, DateTimeKind.Utc).AddTicks(937));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 5,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 6, 3, 17, 520, DateTimeKind.Utc).AddTicks(938));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 6,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 6, 3, 17, 520, DateTimeKind.Utc).AddTicks(939));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 7,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 6, 3, 17, 520, DateTimeKind.Utc).AddTicks(939));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 8,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 6, 3, 17, 520, DateTimeKind.Utc).AddTicks(940));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 9,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 6, 3, 17, 520, DateTimeKind.Utc).AddTicks(941));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 11,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 6, 3, 17, 520, DateTimeKind.Utc).AddTicks(942));

            migrationBuilder.CreateIndex(
                name: "ix_product_entry_request_items_product_entry_request_id",
                table: "product_entry_request_items",
                column: "product_entry_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_entry_request_items_product_id",
                table: "product_entry_request_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_entry_request_items_product_state_id",
                table: "product_entry_request_items",
                column: "product_state_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_exit_request_items_product_exit_request_id",
                table: "product_exit_request_items",
                column: "product_exit_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_exit_request_items_product_id",
                table: "product_exit_request_items",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_entry_request_items");

            migrationBuilder.DropTable(
                name: "product_exit_request_items");

            migrationBuilder.AddColumn<int>(
                name: "product_id",
                table: "product_exit_requests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "requested_quantity",
                table: "product_exit_requests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "entered_quantity",
                table: "product_entry_requests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "product_id",
                table: "product_entry_requests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 1,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(3760));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 2,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(7685));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 3,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(7687));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 4,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(7688));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 5,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(7689));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 6,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(7690));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 7,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(7691));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 8,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(7692));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 9,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(7693));

            migrationBuilder.UpdateData(
                table: "pages",
                keyColumn: "id",
                keyValue: 11,
                column: "insert_date",
                value: new DateTime(2026, 8, 30, 5, 8, 42, 363, DateTimeKind.Utc).AddTicks(7694));

            migrationBuilder.CreateIndex(
                name: "ix_product_exit_requests_product_id",
                table: "product_exit_requests",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_entry_requests_product_id",
                table: "product_entry_requests",
                column: "product_id");

            migrationBuilder.AddForeignKey(
                name: "fk_product_entry_requests_products_product_id",
                table: "product_entry_requests",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_product_exit_requests_products_product_id",
                table: "product_exit_requests",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
