using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeIndexesAndPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseBins_IsDeleted",
                table: "WarehouseBins",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_UserPagePermissions_IsDeleted",
                table: "UserPagePermissions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_UserPagePermissions_UserId_PageId_IsDeleted",
                table: "UserPagePermissions",
                columns: new[] { "UserId", "PageId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_IsDeleted",
                table: "UserGroups",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_IsDeleted",
                table: "Suppliers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStates_IsDeleted",
                table: "ProductStates",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsDeleted",
                table: "Notifications",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_IsDeleted",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_BranchId_ProductId",
                table: "Inventories",
                columns: new[] { "BranchId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_IsDeleted",
                table: "Inventories",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPagePermissions_IsDeleted",
                table: "GroupPagePermissions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_IsDeleted",
                table: "Departments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IsDeleted",
                table: "Categories",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WarehouseBins_IsDeleted",
                table: "WarehouseBins");

            migrationBuilder.DropIndex(
                name: "IX_UserPagePermissions_IsDeleted",
                table: "UserPagePermissions");

            migrationBuilder.DropIndex(
                name: "IX_UserPagePermissions_UserId_PageId_IsDeleted",
                table: "UserPagePermissions");

            migrationBuilder.DropIndex(
                name: "IX_UserGroups_IsDeleted",
                table: "UserGroups");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_IsDeleted",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_ProductStates_IsDeleted",
                table: "ProductStates");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_IsDeleted",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead_IsDeleted",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_BranchId_ProductId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_IsDeleted",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_GroupPagePermissions_IsDeleted",
                table: "GroupPagePermissions");

            migrationBuilder.DropIndex(
                name: "IX_Departments_IsDeleted",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Categories_IsDeleted",
                table: "Categories");

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 1,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 8, 50, 948, DateTimeKind.Utc).AddTicks(4134));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 2,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 8, 50, 949, DateTimeKind.Utc).AddTicks(356));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 3,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 8, 50, 949, DateTimeKind.Utc).AddTicks(358));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 4,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 8, 50, 949, DateTimeKind.Utc).AddTicks(360));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 5,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 8, 50, 949, DateTimeKind.Utc).AddTicks(361));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 6,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 8, 50, 949, DateTimeKind.Utc).AddTicks(362));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 7,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 8, 50, 949, DateTimeKind.Utc).AddTicks(363));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 8,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 8, 50, 949, DateTimeKind.Utc).AddTicks(364));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 9,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 8, 50, 949, DateTimeKind.Utc).AddTicks(365));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 11,
                column: "InsertDate",
                value: new DateTime(2026, 9, 27, 9, 8, 50, 949, DateTimeKind.Utc).AddTicks(367));
        }
    }
}
