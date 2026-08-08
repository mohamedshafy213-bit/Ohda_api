using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class Phase2_Extensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "ProductExitRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientDepartment",
                table: "ProductExitRequests",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                table: "ProductExitRequests",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ReferenceId = table.Column<int>(type: "int", nullable: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductEntryRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    EnteredQuantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FromSource = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReceivedByUserId = table.Column<int>(type: "int", nullable: false),
                    SupervisorId = table.Column<int>(type: "int", nullable: true),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_ProductEntryRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductEntryRequests_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductEntryRequests_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductEntryRequests_Users_ReceivedByUserId",
                        column: x => x.ReceivedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductEntryRequests_Users_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPagePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PageId = table.Column<int>(type: "int", nullable: false),
                    GrantedByUserId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_UserPagePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPagePermissions_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPagePermissions_Users_GrantedByUserId",
                        column: x => x.GrantedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPagePermissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 1,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 14, 22, 53, 247, DateTimeKind.Utc).AddTicks(7497));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 2,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 14, 22, 53, 248, DateTimeKind.Utc).AddTicks(1432));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 3,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 14, 22, 53, 248, DateTimeKind.Utc).AddTicks(1441));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 4,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 14, 22, 53, 248, DateTimeKind.Utc).AddTicks(1444));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 5,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 14, 22, 53, 248, DateTimeKind.Utc).AddTicks(1446));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 6,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 14, 22, 53, 248, DateTimeKind.Utc).AddTicks(1449));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 7,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 14, 22, 53, 248, DateTimeKind.Utc).AddTicks(1451));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 8,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 14, 22, 53, 248, DateTimeKind.Utc).AddTicks(1454));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 9,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 14, 22, 53, 248, DateTimeKind.Utc).AddTicks(1457));

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductEntryRequests_ManagerId",
                table: "ProductEntryRequests",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductEntryRequests_ProductId",
                table: "ProductEntryRequests",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductEntryRequests_ReceivedByUserId",
                table: "ProductEntryRequests",
                column: "ReceivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductEntryRequests_SupervisorId",
                table: "ProductEntryRequests",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPagePermissions_GrantedByUserId",
                table: "UserPagePermissions",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPagePermissions_PageId",
                table: "UserPagePermissions",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPagePermissions_UserId",
                table: "UserPagePermissions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "ProductEntryRequests");

            migrationBuilder.DropTable(
                name: "UserPagePermissions");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "ProductExitRequests");

            migrationBuilder.DropColumn(
                name: "RecipientDepartment",
                table: "ProductExitRequests");

            migrationBuilder.DropColumn(
                name: "RecipientName",
                table: "ProductExitRequests");

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 1,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 12, 0, 34, 651, DateTimeKind.Utc).AddTicks(5664));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 2,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 12, 0, 34, 651, DateTimeKind.Utc).AddTicks(7979));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 3,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 12, 0, 34, 651, DateTimeKind.Utc).AddTicks(7984));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 4,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 12, 0, 34, 651, DateTimeKind.Utc).AddTicks(8048));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 5,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 12, 0, 34, 651, DateTimeKind.Utc).AddTicks(8051));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 6,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 12, 0, 34, 651, DateTimeKind.Utc).AddTicks(8053));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 7,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 12, 0, 34, 651, DateTimeKind.Utc).AddTicks(8055));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 8,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 12, 0, 34, 651, DateTimeKind.Utc).AddTicks(8057));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 9,
                column: "InsertDate",
                value: new DateTime(2026, 7, 23, 12, 0, 34, 651, DateTimeKind.Utc).AddTicks(8058));
        }
    }
}
