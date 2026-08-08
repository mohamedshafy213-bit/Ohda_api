using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class Add_Compass_And_ProductItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Compasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RecipientName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Place = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ExitDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductExitRequestId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsertUserCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsertDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateUserCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleteUserCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compasses_ProductExitRequests_ProductExitRequestId",
                        column: x => x.ProductExitRequestId,
                        principalTable: "ProductExitRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProductItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QRCode = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RecipientName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Place = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExitDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProductExitRequestId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsertUserCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsertDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateUserCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleteUserCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductItems_ProductExitRequests_ProductExitRequestId",
                        column: x => x.ProductExitRequestId,
                        principalTable: "ProductExitRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 1,
                column: "InsertDate",
                value: new DateTime(2026, 7, 27, 16, 22, 53, 526, DateTimeKind.Utc).AddTicks(1468));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 2,
                column: "InsertDate",
                value: new DateTime(2026, 7, 27, 16, 22, 53, 526, DateTimeKind.Utc).AddTicks(3428));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 3,
                column: "InsertDate",
                value: new DateTime(2026, 7, 27, 16, 22, 53, 526, DateTimeKind.Utc).AddTicks(3432));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 4,
                column: "InsertDate",
                value: new DateTime(2026, 7, 27, 16, 22, 53, 526, DateTimeKind.Utc).AddTicks(3434));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 5,
                column: "InsertDate",
                value: new DateTime(2026, 7, 27, 16, 22, 53, 526, DateTimeKind.Utc).AddTicks(3436));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 6,
                column: "InsertDate",
                value: new DateTime(2026, 7, 27, 16, 22, 53, 526, DateTimeKind.Utc).AddTicks(3438));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 7,
                column: "InsertDate",
                value: new DateTime(2026, 7, 27, 16, 22, 53, 526, DateTimeKind.Utc).AddTicks(3439));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 8,
                column: "InsertDate",
                value: new DateTime(2026, 7, 27, 16, 22, 53, 526, DateTimeKind.Utc).AddTicks(3441));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 9,
                column: "InsertDate",
                value: new DateTime(2026, 7, 27, 16, 22, 53, 526, DateTimeKind.Utc).AddTicks(3458));

            migrationBuilder.CreateIndex(
                name: "IX_Compasses_ProductExitRequestId",
                table: "Compasses",
                column: "ProductExitRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductItems_ProductExitRequestId",
                table: "ProductItems",
                column: "ProductExitRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductItems_ProductId",
                table: "ProductItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductItems_SerialNumber",
                table: "ProductItems",
                column: "SerialNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Compasses");

            migrationBuilder.DropTable(
                name: "ProductItems");

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
    }
}
