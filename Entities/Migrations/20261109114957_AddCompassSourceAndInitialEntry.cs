using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddCompassSourceAndInitialEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[PackageItems]') AND name = 'CategoryId')
BEGIN
    ALTER TABLE [PackageItems] ADD [CategoryId] int NULL;
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Compasses]') AND name = 'Source')
BEGIN
    ALTER TABLE [Compasses] ADD [Source] nvarchar(50) NOT NULL DEFAULT N'Normal';
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_PackageItems_CategoryId' AND object_id = OBJECT_ID(N'[PackageItems]'))
BEGIN
    CREATE INDEX [IX_PackageItems_CategoryId] ON [PackageItems] ([CategoryId]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_PackageItems_Categories_CategoryId')
BEGIN
    ALTER TABLE [PackageItems] ADD CONSTRAINT [FK_PackageItems_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_PackageItems_Categories_CategoryId')
BEGIN
    ALTER TABLE [PackageItems] DROP CONSTRAINT [FK_PackageItems_Categories_CategoryId];
END
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_PackageItems_CategoryId' AND object_id = OBJECT_ID(N'[PackageItems]'))
BEGIN
    DROP INDEX [IX_PackageItems_CategoryId] ON [PackageItems];
END
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[PackageItems]') AND name = 'CategoryId')
BEGIN
    ALTER TABLE [PackageItems] DROP COLUMN [CategoryId];
END
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Compasses]') AND name = 'Source')
BEGIN
    ALTER TABLE [Compasses] DROP COLUMN [Source];
END
");
        }
    }
}
