using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalConfigRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestType = table.Column<int>(type: "int", nullable: false),
                    UserGroupId = table.Column<int>(type: "int", nullable: false),
                    WorkflowRole = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ApprovalConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalConfigs_UserGroups_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.InsertData(
                table: "Pages",
                columns: new[] { "Id", "DeleteDate", "DeleteUserCode", "Icon", "InsertDate", "InsertUserCode", "IsDeleted", "LastUpdate", "Path", "SortOrder", "Title", "UpdateUserCode" },
                values: new object[] { 11, null, null, "explore", new DateTime(2026, 8, 26, 12, 2, 37, 224, DateTimeKind.Utc).AddTicks(4496), null, false, null, "/compass", 10, "Compass Log", null });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalConfigs_UserGroupId",
                table: "ApprovalConfigs",
                column: "UserGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalConfigs");

            migrationBuilder.DeleteData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 11);

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
        }
    }
}
