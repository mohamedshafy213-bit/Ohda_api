using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAllowedRolesColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedRoles",
                table: "Pages");

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 1,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 20, 59, 5, 317, DateTimeKind.Utc).AddTicks(6619));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 2,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 20, 59, 5, 317, DateTimeKind.Utc).AddTicks(8688));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 3,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 20, 59, 5, 317, DateTimeKind.Utc).AddTicks(8691));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 4,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 20, 59, 5, 317, DateTimeKind.Utc).AddTicks(8693));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 5,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 20, 59, 5, 317, DateTimeKind.Utc).AddTicks(8695));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 6,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 20, 59, 5, 317, DateTimeKind.Utc).AddTicks(8696));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 7,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 20, 59, 5, 317, DateTimeKind.Utc).AddTicks(8698));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 8,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 20, 59, 5, 317, DateTimeKind.Utc).AddTicks(8699));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 9,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 20, 59, 5, 317, DateTimeKind.Utc).AddTicks(8700));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AllowedRoles",
                table: "Pages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AllowedRoles", "InsertDate" },
                values: new object[] { "Admin,Employee,Supervisor,Manager", new DateTime(2026, 7, 23, 20, 26, 43, 528, DateTimeKind.Utc).AddTicks(3175) });

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AllowedRoles", "InsertDate" },
                values: new object[] { "Admin,Employee,Supervisor,Manager", new DateTime(2026, 7, 23, 20, 26, 43, 528, DateTimeKind.Utc).AddTicks(5629) });

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AllowedRoles", "InsertDate" },
                values: new object[] { "Admin,Supervisor,Manager", new DateTime(2026, 7, 23, 20, 26, 43, 528, DateTimeKind.Utc).AddTicks(5633) });

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AllowedRoles", "InsertDate" },
                values: new object[] { "Admin,Employee", new DateTime(2026, 7, 23, 20, 26, 43, 528, DateTimeKind.Utc).AddTicks(5635) });

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AllowedRoles", "InsertDate" },
                values: new object[] { "Admin,Employee,Supervisor,Manager", new DateTime(2026, 7, 23, 20, 26, 43, 528, DateTimeKind.Utc).AddTicks(5637) });

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AllowedRoles", "InsertDate" },
                values: new object[] { "Admin,Employee", new DateTime(2026, 7, 23, 20, 26, 43, 528, DateTimeKind.Utc).AddTicks(5638) });

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AllowedRoles", "InsertDate" },
                values: new object[] { "Admin,Manager", new DateTime(2026, 7, 23, 20, 26, 43, 528, DateTimeKind.Utc).AddTicks(5640) });

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AllowedRoles", "InsertDate" },
                values: new object[] { "Admin,Manager", new DateTime(2026, 7, 23, 20, 26, 43, 528, DateTimeKind.Utc).AddTicks(5642) });

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "AllowedRoles", "InsertDate" },
                values: new object[] { "Admin", new DateTime(2026, 7, 23, 20, 26, 43, 528, DateTimeKind.Utc).AddTicks(5643) });
        }
    }
}
