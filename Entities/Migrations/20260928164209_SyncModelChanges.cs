using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 1,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 42, 7, 804, DateTimeKind.Utc).AddTicks(1378));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 2,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 42, 7, 804, DateTimeKind.Utc).AddTicks(3299));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 3,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 42, 7, 804, DateTimeKind.Utc).AddTicks(3302));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 4,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 42, 7, 804, DateTimeKind.Utc).AddTicks(3304));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 5,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 42, 7, 804, DateTimeKind.Utc).AddTicks(3305));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 6,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 42, 7, 804, DateTimeKind.Utc).AddTicks(3307));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 7,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 42, 7, 804, DateTimeKind.Utc).AddTicks(3325));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 8,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 42, 7, 804, DateTimeKind.Utc).AddTicks(3326));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 9,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 42, 7, 804, DateTimeKind.Utc).AddTicks(3328));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 11,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 42, 7, 804, DateTimeKind.Utc).AddTicks(3330));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 1,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 34, 4, 135, DateTimeKind.Utc).AddTicks(7672));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 2,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 34, 4, 135, DateTimeKind.Utc).AddTicks(9696));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 3,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 34, 4, 135, DateTimeKind.Utc).AddTicks(9699));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 4,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 34, 4, 135, DateTimeKind.Utc).AddTicks(9701));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 5,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 34, 4, 135, DateTimeKind.Utc).AddTicks(9703));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 6,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 34, 4, 135, DateTimeKind.Utc).AddTicks(9704));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 7,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 34, 4, 135, DateTimeKind.Utc).AddTicks(9706));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 8,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 34, 4, 135, DateTimeKind.Utc).AddTicks(9707));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 9,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 34, 4, 135, DateTimeKind.Utc).AddTicks(9709));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 11,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 16, 34, 4, 135, DateTimeKind.Utc).AddTicks(9710));
        }
    }
}
