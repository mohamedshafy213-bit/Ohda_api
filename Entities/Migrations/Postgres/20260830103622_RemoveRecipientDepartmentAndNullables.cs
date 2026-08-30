using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class RemoveRecipientDepartmentAndNullables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill DepartmentId from RecipientDepartment
            migrationBuilder.Sql(@"
                CREATE TEMP TABLE temp_missing_deps AS
                SELECT DISTINCT recipient_department 
                FROM product_exit_requests 
                WHERE department_id IS NULL AND recipient_department IS NOT NULL AND recipient_department <> '';

                INSERT INTO departments (name, description, insert_date, is_deleted)
                SELECT recipient_department, 'Auto-created during migration backfill', NOW(), false
                FROM temp_missing_deps
                WHERE recipient_department NOT IN (SELECT name FROM departments);

                UPDATE product_exit_requests r
                SET department_id = d.id
                FROM departments d
                WHERE r.department_id IS NULL AND r.recipient_department = d.name;

                DROP TABLE temp_missing_deps;
            ");

            migrationBuilder.DropColumn(
                name: "recipient_department",
                table: "product_exit_requests");

            migrationBuilder.AlterColumn<decimal>(
                name: "unit_price",
                table: "products",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "supplier_id",
                table: "products",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

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
                name: "ix_product_exit_requests_insert_date",
                table: "product_exit_requests",
                column: "insert_date");

            migrationBuilder.CreateIndex(
                name: "ix_product_exit_requests_status",
                table: "product_exit_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_product_entry_requests_insert_date",
                table: "product_entry_requests",
                column: "insert_date");

            migrationBuilder.CreateIndex(
                name: "ix_product_entry_requests_status",
                table: "product_entry_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_compasses_insert_date",
                table: "compasses",
                column: "insert_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_product_exit_requests_insert_date",
                table: "product_exit_requests");

            migrationBuilder.DropIndex(
                name: "ix_product_exit_requests_status",
                table: "product_exit_requests");

            migrationBuilder.DropIndex(
                name: "ix_product_entry_requests_insert_date",
                table: "product_entry_requests");

            migrationBuilder.DropIndex(
                name: "ix_product_entry_requests_status",
                table: "product_entry_requests");

            migrationBuilder.DropIndex(
                name: "ix_compasses_insert_date",
                table: "compasses");

            migrationBuilder.AlterColumn<decimal>(
                name: "unit_price",
                table: "products",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "supplier_id",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "recipient_department",
                table: "product_exit_requests",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

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
        }
    }
}
