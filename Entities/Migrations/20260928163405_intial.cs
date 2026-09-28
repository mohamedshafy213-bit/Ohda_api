using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class intial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 1,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 37, 10, 740, DateTimeKind.Utc).AddTicks(2537));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 2,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 37, 10, 740, DateTimeKind.Utc).AddTicks(8793));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 3,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 37, 10, 740, DateTimeKind.Utc).AddTicks(8797));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 4,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 37, 10, 740, DateTimeKind.Utc).AddTicks(8799));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 5,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 37, 10, 740, DateTimeKind.Utc).AddTicks(8800));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 6,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 37, 10, 740, DateTimeKind.Utc).AddTicks(8827));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 7,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 37, 10, 740, DateTimeKind.Utc).AddTicks(8828));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 8,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 37, 10, 740, DateTimeKind.Utc).AddTicks(8830));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 9,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 37, 10, 740, DateTimeKind.Utc).AddTicks(8831));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 11,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 37, 10, 740, DateTimeKind.Utc).AddTicks(8832));
        }
    }
}
