using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class Packageuntracked9112026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.PackageItems', 'U') IS NOT NULL DROP TABLE dbo.PackageItems;
                IF OBJECT_ID('dbo.Packages', 'U') IS NOT NULL DROP TABLE dbo.Packages;
                IF OBJECT_ID('dbo.UntrackedProducts', 'U') IS NOT NULL DROP TABLE dbo.UntrackedProducts;
                IF OBJECT_ID('dbo.UntrackedItems', 'U') IS NOT NULL DROP TABLE dbo.UntrackedItems;

                IF COL_LENGTH('dbo.ProductExitRequestItems', 'IsUntracked') IS NOT NULL
                    ALTER TABLE dbo.ProductExitRequestItems DROP COLUMN [IsUntracked];
                IF COL_LENGTH('dbo.ProductExitRequestItems', 'PackageId') IS NOT NULL
                    ALTER TABLE dbo.ProductExitRequestItems DROP COLUMN [PackageId];
                IF COL_LENGTH('dbo.ProductExitRequestItems', 'PackageName') IS NOT NULL
                    ALTER TABLE dbo.ProductExitRequestItems DROP COLUMN [PackageName];
                IF COL_LENGTH('dbo.ProductExitRequestItems', 'UntrackedItemName') IS NOT NULL
                    ALTER TABLE dbo.ProductExitRequestItems DROP COLUMN [UntrackedItemName];
            ");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "ProductExitRequestItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsUntracked",
                table: "ProductExitRequestItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PackageId",
                table: "ProductExitRequestItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackageName",
                table: "ProductExitRequestItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UntrackedItemName",
                table: "ProductExitRequestItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    InsertUserCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsertDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateUserCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleteUserCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Packages_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UntrackedItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    InsertUserCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsertDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateUserCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleteUserCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UntrackedItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UntrackedItems_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackageItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    TrackedProductId = table.Column<int>(type: "int", nullable: true),
                    UntrackedItemId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    InsertUserCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsertDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateUserCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleteUserCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageItems_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageItems_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackageItems_Products_TrackedProductId",
                        column: x => x.TrackedProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageItems_UntrackedItems_UntrackedItemId",
                        column: x => x.UntrackedItemId,
                        principalTable: "UntrackedItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 1,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 8, 29, 0, 68, DateTimeKind.Utc).AddTicks(7011));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 2,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 8, 29, 0, 69, DateTimeKind.Utc).AddTicks(3985));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 3,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 8, 29, 0, 69, DateTimeKind.Utc).AddTicks(3988));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 4,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 8, 29, 0, 69, DateTimeKind.Utc).AddTicks(3990));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 5,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 8, 29, 0, 69, DateTimeKind.Utc).AddTicks(3991));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 6,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 8, 29, 0, 69, DateTimeKind.Utc).AddTicks(3992));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 7,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 8, 29, 0, 69, DateTimeKind.Utc).AddTicks(3993));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 8,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 8, 29, 0, 69, DateTimeKind.Utc).AddTicks(3994));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 9,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 8, 29, 0, 69, DateTimeKind.Utc).AddTicks(3995));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 11,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 8, 29, 0, 69, DateTimeKind.Utc).AddTicks(3997));

            migrationBuilder.CreateIndex(
                name: "IX_PackageItems_BranchId",
                table: "PackageItems",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageItems_CategoryId",
                table: "PackageItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageItems_IsDeleted",
                table: "PackageItems",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PackageItems_PackageId",
                table: "PackageItems",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageItems_TrackedProductId",
                table: "PackageItems",
                column: "TrackedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageItems_UntrackedItemId",
                table: "PackageItems",
                column: "UntrackedItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_BranchId",
                table: "Packages",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_IsDeleted",
                table: "Packages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_UntrackedItems_BranchId",
                table: "UntrackedItems",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_UntrackedItems_IsDeleted",
                table: "UntrackedItems",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageItems");

            migrationBuilder.DropTable(
                name: "Packages");

            migrationBuilder.DropTable(
                name: "UntrackedItems");

            migrationBuilder.DropColumn(
                name: "IsUntracked",
                table: "ProductExitRequestItems");

            migrationBuilder.DropColumn(
                name: "PackageId",
                table: "ProductExitRequestItems");

            migrationBuilder.DropColumn(
                name: "PackageName",
                table: "ProductExitRequestItems");

            migrationBuilder.DropColumn(
                name: "UntrackedItemName",
                table: "ProductExitRequestItems");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "ProductExitRequestItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 1,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 7, 50, 0, 82, DateTimeKind.Utc).AddTicks(1905));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 2,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 7, 50, 0, 82, DateTimeKind.Utc).AddTicks(5115));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 3,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 7, 50, 0, 82, DateTimeKind.Utc).AddTicks(5117));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 4,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 7, 50, 0, 82, DateTimeKind.Utc).AddTicks(5119));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 5,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 7, 50, 0, 82, DateTimeKind.Utc).AddTicks(5120));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 6,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 7, 50, 0, 82, DateTimeKind.Utc).AddTicks(5121));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 7,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 7, 50, 0, 82, DateTimeKind.Utc).AddTicks(5122));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 8,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 7, 50, 0, 82, DateTimeKind.Utc).AddTicks(5123));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 9,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 7, 50, 0, 82, DateTimeKind.Utc).AddTicks(5124));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 11,
                column: "InsertDate",
                value: new DateTime(2026, 11, 9, 7, 50, 0, 82, DateTimeKind.Utc).AddTicks(5125));
        }
    }
}
