using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowStepColorsAndAuditTrail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalTrail",
                table: "ProductExitRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentStep",
                table: "ProductExitRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequesterConfirmed",
                table: "ProductExitRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequesterConfirmedDate",
                table: "ProductExitRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalTrail",
                table: "ProductEntryRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentStep",
                table: "ProductEntryRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequesterConfirmed",
                table: "ProductEntryRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequesterConfirmedDate",
                table: "ProductEntryRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorHex",
                table: "ApprovalConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StepName",
                table: "ApprovalConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StepOrder",
                table: "ApprovalConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 1,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 18, 23, 11, 733, DateTimeKind.Utc).AddTicks(1687));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 2,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 18, 23, 11, 733, DateTimeKind.Utc).AddTicks(3667));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 3,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 18, 23, 11, 733, DateTimeKind.Utc).AddTicks(3672));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 4,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 18, 23, 11, 733, DateTimeKind.Utc).AddTicks(3675));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 5,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 18, 23, 11, 733, DateTimeKind.Utc).AddTicks(3677));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 6,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 18, 23, 11, 733, DateTimeKind.Utc).AddTicks(3678));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 7,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 18, 23, 11, 733, DateTimeKind.Utc).AddTicks(3680));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 8,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 18, 23, 11, 733, DateTimeKind.Utc).AddTicks(3681));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 9,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 18, 23, 11, 733, DateTimeKind.Utc).AddTicks(3683));

            migrationBuilder.UpdateData(
                table: "Pages",
                keyColumn: "Id",
                keyValue: 11,
                column: "InsertDate",
                value: new DateTime(2026, 9, 28, 18, 23, 11, 733, DateTimeKind.Utc).AddTicks(3684));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalTrail",
                table: "ProductExitRequests");

            migrationBuilder.DropColumn(
                name: "CurrentStep",
                table: "ProductExitRequests");

            migrationBuilder.DropColumn(
                name: "IsRequesterConfirmed",
                table: "ProductExitRequests");

            migrationBuilder.DropColumn(
                name: "RequesterConfirmedDate",
                table: "ProductExitRequests");

            migrationBuilder.DropColumn(
                name: "ApprovalTrail",
                table: "ProductEntryRequests");

            migrationBuilder.DropColumn(
                name: "CurrentStep",
                table: "ProductEntryRequests");

            migrationBuilder.DropColumn(
                name: "IsRequesterConfirmed",
                table: "ProductEntryRequests");

            migrationBuilder.DropColumn(
                name: "RequesterConfirmedDate",
                table: "ProductEntryRequests");

            migrationBuilder.DropColumn(
                name: "ColorHex",
                table: "ApprovalConfigs");

            migrationBuilder.DropColumn(
                name: "StepName",
                table: "ApprovalConfigs");

            migrationBuilder.DropColumn(
                name: "StepOrder",
                table: "ApprovalConfigs");

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
    }
}
