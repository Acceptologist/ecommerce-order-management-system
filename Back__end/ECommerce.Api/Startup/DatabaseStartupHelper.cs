using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Startup;

/// <summary>
/// Runs database migrations and optional schema safeguards at application startup.
/// Keeps Program.cs as a thin composition root.
/// </summary>
public static class DatabaseStartupHelper
{
    public static async Task EnsureDatabaseAsync(AppDbContext context, IWebHostEnvironment environment)
    {
        var appliedMigrations = context.Database.GetAppliedMigrations();
        var hasMigrationHistory = appliedMigrations.Any();

        if (environment.IsDevelopment())
        {
            var resetDb = Environment.GetEnvironmentVariable("RESET_DB") == "true";
            if (resetDb || !hasMigrationHistory)
            {
                await context.Database.EnsureDeletedAsync();
            }
        }

        await context.Database.MigrateAsync();
        await EnsureSoftDeleteColumnsAsync(context);
        await EnsureNotificationOrderIdColumnAsync(context);
    }

    /// <summary>
    /// Ensures Products and Categories have IsDeleted and DeletedAtUtc columns (soft delete).
    /// Idempotent; safe when EF migration was not applied (e.g. dotnet ef from Api without --project).
    /// </summary>
    private static async Task EnsureSoftDeleteColumnsAsync(AppDbContext context)
    {
        const string migrationId = "20260315000000_AddSoftDeleteToProductAndCategory";
        var applied = await context.Database.GetAppliedMigrationsAsync();
        if (applied.Contains(migrationId))
            return;

        var conn = context.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [Products] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    ALTER TABLE [Products] ADD [DeletedAtUtc] datetime2 NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Categories') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [Categories] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    ALTER TABLE [Categories] ADD [DeletedAtUtc] datetime2 NULL;
END";
            await cmd.ExecuteNonQueryAsync();

            using var insertCmd = conn.CreateCommand();
            insertCmd.CommandText = @"INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
SELECT @p0, @p1 WHERE NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = @p0)";
            var p0 = insertCmd.CreateParameter();
            p0.ParameterName = "@p0";
            p0.Value = migrationId;
            var p1 = insertCmd.CreateParameter();
            p1.ParameterName = "@p1";
            p1.Value = "8.0.14";
            insertCmd.Parameters.Add(p0);
            insertCmd.Parameters.Add(p1);
            await insertCmd.ExecuteNonQueryAsync();
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    /// <summary>
    /// Ensures Notifications table has OrderId column (nullable) for order-related notifications.
    /// Idempotent; safe when migration was not applied.
    /// </summary>
    private static async Task EnsureNotificationOrderIdColumnAsync(AppDbContext context)
    {
        const string migrationId = "20260315100000_AddOrderIdToNotifications";
        var applied = await context.Database.GetAppliedMigrationsAsync();
        if (applied.Contains(migrationId))
            return;

        var conn = context.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Notifications') AND name = 'OrderId')
BEGIN
    ALTER TABLE [Notifications] ADD [OrderId] int NULL;
END";
            await cmd.ExecuteNonQueryAsync();

            using var insertCmd = conn.CreateCommand();
            insertCmd.CommandText = @"INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
SELECT @p0, @p1 WHERE NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = @p0)";
            var p0 = insertCmd.CreateParameter();
            p0.ParameterName = "@p0";
            p0.Value = migrationId;
            var p1 = insertCmd.CreateParameter();
            p1.ParameterName = "@p1";
            p1.Value = "8.0.14";
            insertCmd.Parameters.Add(p0);
            insertCmd.Parameters.Add(p1);
            await insertCmd.ExecuteNonQueryAsync();
        }
        finally
        {
            await conn.CloseAsync();
        }
    }
}
