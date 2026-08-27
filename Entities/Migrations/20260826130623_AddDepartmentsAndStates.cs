using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentsAndStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "ProductExitRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "ProductEntryRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductStateId",
                table: "ProductEntryRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Compasses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductEntryRequestId",
                table: "Compasses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductStateId",
                table: "Compasses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Compasses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_ProductStates", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 1,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 13, 6, 22, 197, DateTimeKind.Utc).AddTicks(7685));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 2,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 13, 6, 22, 197, DateTimeKind.Utc).AddTicks(9613));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 3,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 13, 6, 22, 197, DateTimeKind.Utc).AddTicks(9617));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 4,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 13, 6, 22, 197, DateTimeKind.Utc).AddTicks(9619));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 5,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 13, 6, 22, 197, DateTimeKind.Utc).AddTicks(9620));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 6,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 13, 6, 22, 197, DateTimeKind.Utc).AddTicks(9622));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 7,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 13, 6, 22, 197, DateTimeKind.Utc).AddTicks(9624));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 8,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 13, 6, 22, 197, DateTimeKind.Utc).AddTicks(9625));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 9,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 13, 6, 22, 197, DateTimeKind.Utc).AddTicks(9626));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 11,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 13, 6, 22, 197, DateTimeKind.Utc).AddTicks(9629));

            migrationBuilder.CreateIndex(
                name: "IX_ProductExitRequests_DepartmentId",
                table: "ProductExitRequests",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductEntryRequests_DepartmentId",
                table: "ProductEntryRequests",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductEntryRequests_ProductStateId",
                table: "ProductEntryRequests",
                column: "ProductStateId");

            migrationBuilder.CreateIndex(
                name: "IX_Compasses_DepartmentId",
                table: "Compasses",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Compasses_ProductEntryRequestId",
                table: "Compasses",
                column: "ProductEntryRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Compasses_ProductStateId",
                table: "Compasses",
                column: "ProductStateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Compasses_Departments_DepartmentId",
                table: "Compasses",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Compasses_ProductEntryRequests_ProductEntryRequestId",
                table: "Compasses",
                column: "ProductEntryRequestId",
                principalTable: "ProductEntryRequests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Compasses_ProductStates_ProductStateId",
                table: "Compasses",
                column: "ProductStateId",
                principalTable: "ProductStates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductEntryRequests_Departments_DepartmentId",
                table: "ProductEntryRequests",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductEntryRequests_ProductStates_ProductStateId",
                table: "ProductEntryRequests",
                column: "ProductStateId",
                principalTable: "ProductStates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductExitRequests_Departments_DepartmentId",
                table: "ProductExitRequests",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Compasses_Departments_DepartmentId",
                table: "Compasses");

            migrationBuilder.DropForeignKey(
                name: "FK_Compasses_ProductEntryRequests_ProductEntryRequestId",
                table: "Compasses");

            migrationBuilder.DropForeignKey(
                name: "FK_Compasses_ProductStates_ProductStateId",
                table: "Compasses");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductEntryRequests_Departments_DepartmentId",
                table: "ProductEntryRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductEntryRequests_ProductStates_ProductStateId",
                table: "ProductEntryRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductExitRequests_Departments_DepartmentId",
                table: "ProductExitRequests");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "ProductStates");

            migrationBuilder.DropIndex(
                name: "IX_ProductExitRequests_DepartmentId",
                table: "ProductExitRequests");

            migrationBuilder.DropIndex(
                name: "IX_ProductEntryRequests_DepartmentId",
                table: "ProductEntryRequests");

            migrationBuilder.DropIndex(
                name: "IX_ProductEntryRequests_ProductStateId",
                table: "ProductEntryRequests");

            migrationBuilder.DropIndex(
                name: "IX_Compasses_DepartmentId",
                table: "Compasses");

            migrationBuilder.DropIndex(
                name: "IX_Compasses_ProductEntryRequestId",
                table: "Compasses");

            migrationBuilder.DropIndex(
                name: "IX_Compasses_ProductStateId",
                table: "Compasses");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "ProductExitRequests");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "ProductEntryRequests");

            migrationBuilder.DropColumn(
                name: "ProductStateId",
                table: "ProductEntryRequests");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Compasses");

            migrationBuilder.DropColumn(
                name: "ProductEntryRequestId",
                table: "Compasses");

            migrationBuilder.DropColumn(
                name: "ProductStateId",
                table: "Compasses");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Compasses");

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 1,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 12, 2, 37, 223, DateTimeKind.Utc).AddTicks(7103));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 2,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 12, 2, 37, 224, DateTimeKind.Utc).AddTicks(4449));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 3,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 12, 2, 37, 224, DateTimeKind.Utc).AddTicks(4461));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 4,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 12, 2, 37, 224, DateTimeKind.Utc).AddTicks(4467));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 5,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 12, 2, 37, 224, DateTimeKind.Utc).AddTicks(4472));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 6,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 12, 2, 37, 224, DateTimeKind.Utc).AddTicks(4477));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 7,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 12, 2, 37, 224, DateTimeKind.Utc).AddTicks(4481));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 8,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 12, 2, 37, 224, DateTimeKind.Utc).AddTicks(4487));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 9,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 12, 2, 37, 224, DateTimeKind.Utc).AddTicks(4492));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 11,
                column: "InsertDate",
                value: new DateTime(2026, 8, 26, 12, 2, 37, 224, DateTimeKind.Utc).AddTicks(4496));
        }
    }
}
