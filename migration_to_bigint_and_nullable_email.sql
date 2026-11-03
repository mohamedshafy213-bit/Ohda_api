-- ==============================================================================
-- Migration Script: Convert MilitaryNumber / User Id to BIGINT & Make Email Nullable
-- Database: OhdaDb3 (SQL Server)
-- ==============================================================================

USE [OhdaDb3];
GO

PRINT 'Starting migration for BIGINT military numbers, nullable email, and acceptance page...';
GO

BEGIN TRANSACTION;
BEGIN TRY

    -- 1. Drop all Foreign Keys referencing [Users]([Id])
    PRINT 'Dropping Foreign Key constraints referencing Users...';

    IF OBJECT_ID(N'[FK_ProductExitRequests_Users_ManagerId]', 'F') IS NOT NULL
        ALTER TABLE [ProductExitRequests] DROP CONSTRAINT [FK_ProductExitRequests_Users_ManagerId];

    IF OBJECT_ID(N'[FK_ProductExitRequests_Users_RequestedByUserId]', 'F') IS NOT NULL
        ALTER TABLE [ProductExitRequests] DROP CONSTRAINT [FK_ProductExitRequests_Users_RequestedByUserId];

    IF OBJECT_ID(N'[FK_ProductExitRequests_Users_SupervisorId]', 'F') IS NOT NULL
        ALTER TABLE [ProductExitRequests] DROP CONSTRAINT [FK_ProductExitRequests_Users_SupervisorId];

    IF OBJECT_ID(N'[FK_ProductEntryRequests_Users_ManagerId]', 'F') IS NOT NULL
        ALTER TABLE [ProductEntryRequests] DROP CONSTRAINT [FK_ProductEntryRequests_Users_ManagerId];

    IF OBJECT_ID(N'[FK_ProductEntryRequests_Users_ReceivedByUserId]', 'F') IS NOT NULL
        ALTER TABLE [ProductEntryRequests] DROP CONSTRAINT [FK_ProductEntryRequests_Users_ReceivedByUserId];

    IF OBJECT_ID(N'[FK_ProductEntryRequests_Users_SupervisorId]', 'F') IS NOT NULL
        ALTER TABLE [ProductEntryRequests] DROP CONSTRAINT [FK_ProductEntryRequests_Users_SupervisorId];

    IF OBJECT_ID(N'[FK_Notifications_Users_UserId]', 'F') IS NOT NULL
        ALTER TABLE [Notifications] DROP CONSTRAINT [FK_Notifications_Users_UserId];

    IF OBJECT_ID(N'[FK_UserPagePermissions_Users_GrantedByUserId]', 'F') IS NOT NULL
        ALTER TABLE [UserPagePermissions] DROP CONSTRAINT [FK_UserPagePermissions_Users_GrantedByUserId];

    IF OBJECT_ID(N'[FK_UserPagePermissions_Users_UserId]', 'F') IS NOT NULL
        ALTER TABLE [UserPagePermissions] DROP CONSTRAINT [FK_UserPagePermissions_Users_UserId];

    IF OBJECT_ID(N'[FK_GroupPagePermissions_Users_GrantedByUserId]', 'F') IS NOT NULL
        ALTER TABLE [GroupPagePermissions] DROP CONSTRAINT [FK_GroupPagePermissions_Users_GrantedByUserId];

    IF OBJECT_ID(N'[FK_Orders_Users_UserId]', 'F') IS NOT NULL
        ALTER TABLE [Orders] DROP CONSTRAINT [FK_Orders_Users_UserId];

    -- 2. Drop Primary Key on [Users]
    PRINT 'Dropping Primary Key constraint on Users...';
    DECLARE @PkName NVARCHAR(128);
    SELECT @PkName = name FROM sys.key_constraints WHERE [type] = 'PK' AND [parent_object_id] = OBJECT_ID(N'[Users]');
    IF @PkName IS NOT NULL
    BEGIN
        EXEC('ALTER TABLE [Users] DROP CONSTRAINT [' + @PkName + '];');
    END

    -- 3. Drop Email unique index/constraint if exists
    PRINT 'Updating Email column to NULL and creating filtered unique index...';
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Users_Email' AND object_id = OBJECT_ID(N'[Users]'))
        DROP INDEX [IX_Users_Email] ON [Users];

    ALTER TABLE [Users] ALTER COLUMN [Email] NVARCHAR(150) NULL;

    CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email]
    ON [Users]([Email])
    WHERE [Email] IS NOT NULL;

    -- 4. Alter [Users].[Id] to BIGINT
    PRINT 'Altering Users.Id to BIGINT...';
    ALTER TABLE [Users] ALTER COLUMN [Id] BIGINT NOT NULL;

    -- Re-create Primary Key on [Users]
    ALTER TABLE [Users] ADD CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC);

    -- 5. Alter referencing columns to BIGINT
    PRINT 'Altering referencing columns in related tables to BIGINT...';

    IF OBJECT_ID(N'[ProductExitRequests]', 'U') IS NOT NULL
    BEGIN
        ALTER TABLE [ProductExitRequests] ALTER COLUMN [RequestedByUserId] BIGINT NOT NULL;
        ALTER TABLE [ProductExitRequests] ALTER COLUMN [SupervisorId] BIGINT NULL;
        ALTER TABLE [ProductExitRequests] ALTER COLUMN [ManagerId] BIGINT NULL;
    END

    IF OBJECT_ID(N'[ProductEntryRequests]', 'U') IS NOT NULL
    BEGIN
        ALTER TABLE [ProductEntryRequests] ALTER COLUMN [ReceivedByUserId] BIGINT NOT NULL;
        ALTER TABLE [ProductEntryRequests] ALTER COLUMN [SupervisorId] BIGINT NULL;
        ALTER TABLE [ProductEntryRequests] ALTER COLUMN [ManagerId] BIGINT NULL;
    END

    IF OBJECT_ID(N'[Notifications]', 'U') IS NOT NULL
    BEGIN
        ALTER TABLE [Notifications] ALTER COLUMN [UserId] BIGINT NOT NULL;
    END

    IF OBJECT_ID(N'[UserPagePermissions]', 'U') IS NOT NULL
    BEGIN
        ALTER TABLE [UserPagePermissions] ALTER COLUMN [UserId] BIGINT NOT NULL;
        ALTER TABLE [UserPagePermissions] ALTER COLUMN [GrantedByUserId] BIGINT NULL;
    END

    IF OBJECT_ID(N'[GroupPagePermissions]', 'U') IS NOT NULL
    BEGIN
        ALTER TABLE [GroupPagePermissions] ALTER COLUMN [GrantedByUserId] BIGINT NULL;
    END

    IF OBJECT_ID(N'[Orders]', 'U') IS NOT NULL
    BEGIN
        ALTER TABLE [Orders] ALTER COLUMN [UserId] BIGINT NOT NULL;
    END

    -- 6. Recreate Foreign Key Constraints
    PRINT 'Re-creating Foreign Key constraints with BIGINT...';

    IF OBJECT_ID(N'[ProductExitRequests]', 'U') IS NOT NULL
    BEGIN
        ALTER TABLE [ProductExitRequests] WITH CHECK ADD CONSTRAINT [FK_ProductExitRequests_Users_ManagerId]
            FOREIGN KEY ([ManagerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;

        ALTER TABLE [ProductExitRequests] WITH CHECK ADD CONSTRAINT [FK_ProductExitRequests_Users_RequestedByUserId]
            FOREIGN KEY ([RequestedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;

        ALTER TABLE [ProductExitRequests] WITH CHECK ADD CONSTRAINT [FK_ProductExitRequests_Users_SupervisorId]
            FOREIGN KEY ([SupervisorId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
    END

    IF OBJECT_ID(N'[ProductEntryRequests]', 'U') IS NOT NULL
    BEGIN
        ALTER TABLE [ProductEntryRequests] WITH CHECK ADD CONSTRAINT [FK_ProductEntryRequests_Users_ManagerId]
            FOREIGN KEY ([ManagerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;

        ALTER TABLE [ProductEntryRequests] WITH CHECK ADD CONSTRAINT [FK_ProductEntryRequests_Users_ReceivedByUserId]
            FOREIGN KEY ([ReceivedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;

        ALTER TABLE [ProductEntryRequests] WITH CHECK ADD CONSTRAINT [FK_ProductEntryRequests_Users_SupervisorId]
            FOREIGN KEY ([SupervisorId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
    END

    IF OBJECT_ID(N'[Notifications]', 'U') IS NOT NULL
    BEGIN
        ALTER TABLE [Notifications] WITH CHECK ADD CONSTRAINT [FK_Notifications_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
    END

    IF OBJECT_ID(N'[UserPagePermissions]', 'U') IS NOT NULL
    BEGIN
        ALTER TABLE [UserPagePermissions] WITH CHECK ADD CONSTRAINT [FK_UserPagePermissions_Users_GrantedByUserId]
            FOREIGN KEY ([GrantedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;

        ALTER TABLE [UserPagePermissions] WITH CHECK ADD CONSTRAINT [FK_UserPagePermissions_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
    END

    IF OBJECT_ID(N'[GroupPagePermissions]', 'U') IS NOT NULL
    BEGIN
        ALTER TABLE [GroupPagePermissions] WITH CHECK ADD CONSTRAINT [FK_GroupPagePermissions_Users_GrantedByUserId]
            FOREIGN KEY ([GrantedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
    END

    IF OBJECT_ID(N'[Orders]', 'U') IS NOT NULL
    BEGIN
        ALTER TABLE [Orders] WITH CHECK ADD CONSTRAINT [FK_Orders_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
    END

    -- 7. Seed Acceptance Page into [Pages] table if not exists
    PRINT 'Seeding Acceptance page into [Pages]...';
    IF NOT EXISTS (SELECT 1 FROM [Pages] WHERE [Path] = '/acceptance' OR [Path] = '/ohda/acceptance')
    BEGIN
        INSERT INTO [Pages] ([Title], [Path], [Icon], [SortOrder], [AllowedRoles], [IsDeleted])
        VALUES (N'مركز التصديقات والاعتمادات', N'/acceptance', N'CheckCheck', 18, N'Admin,Supervisor,Manager', 0);
    END

    COMMIT TRANSACTION;
    PRINT 'Migration completed successfully!';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT 'Error occurred during migration:';
    PRINT ERROR_MESSAGE();
    THROW;
END CATCH;
GO
