using HotelManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Data;

/// <summary>
/// Service to handle automatic database migrations in development
/// </summary>
public static class DatabaseMigrationService
{
    /// <summary>
    /// Applies pending migrations and seeds initial data if needed
    /// </summary>
    public static async Task MigrateDatabaseAsync(IServiceProvider services, IHostEnvironment environment)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            logger.LogInformation("Checking database migration status...");

            // Check if there are pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            var pendingCount = pendingMigrations.Count();

            if (pendingCount > 0)
            {
                logger.LogWarning("Found {Count} pending migration(s): {Migrations}",
                    pendingCount, string.Join(", ", pendingMigrations));

                if (environment.IsDevelopment())
                {
                    logger.LogInformation("Applying migrations automatically (Development environment)...");
                    await context.Database.MigrateAsync();
                    logger.LogInformation("✓ Migrations applied successfully");
                }
                else
                {
                    logger.LogWarning(
                        "⚠️ Pending migrations detected in {Environment} environment. " +
                        "Automatic migration is disabled. Please run migrations manually using: " +
                        "dotnet ef database update",
                        environment.EnvironmentName);
                }
            }
            else
            {
                logger.LogInformation("✓ Database is up to date");
            }

            // Verify database connection
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                logger.LogError("❌ Cannot connect to database");
                throw new InvalidOperationException("Database connection failed");
            }

            logger.LogInformation("✓ Database connection verified");

            // Get applied migrations
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            logger.LogInformation("Applied migrations: {Count}", appliedMigrations.Count());

            // Seed initial data
            if (environment.IsDevelopment())
            {
                logger.LogInformation("Seeding database with initial data...");
                var seedLogger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContextSeed>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var seeder = new ApplicationDbContextSeed(context, seedLogger, userManager);
                await seeder.SeedAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ An error occurred while migrating the database");

            if (environment.IsDevelopment())
            {
                // In development, we want to see the error
                throw;
            }
            else
            {
                // In production, log but don't crash the application
                // The application can still start, but database operations might fail
                logger.LogCritical(
                    "Database migration failed. Application starting without database connectivity. " +
                    "Please resolve database issues immediately.");
            }
        }
    }

    /// <summary>
    /// Ensures the database is created (for testing/development)
    /// </summary>
    public static async Task EnsureDatabaseCreatedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            var created = await context.Database.EnsureCreatedAsync();
            if (created)
            {
                logger.LogInformation("✓ Database created successfully");
            }
            else
            {
                logger.LogInformation("Database already exists");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error ensuring database exists");
            throw;
        }
    }

    /// <summary>
    /// Drops and recreates the database (DANGEROUS - only for development/testing)
    /// </summary>
    public static async Task ResetDatabaseAsync(IServiceProvider services, IHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Database reset is only allowed in Development environment");
        }

        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            logger.LogWarning("⚠️ Dropping database...");
            await context.Database.EnsureDeletedAsync();

            logger.LogInformation("Creating database...");
            await context.Database.MigrateAsync();

            logger.LogInformation("✓ Database reset complete");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error resetting database");
            throw;
        }
    }

    /// <summary>
    /// Gets detailed database information for diagnostics
    /// </summary>
    public static async Task<DatabaseInfo> GetDatabaseInfoAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        var canConnect = await context.Database.CanConnectAsync();

        return new DatabaseInfo
        {
            CanConnect = canConnect,
            Provider = context.Database.ProviderName ?? "Unknown",
            ConnectionString = context.Database.GetConnectionString() ?? "Not configured",
            AppliedMigrations = appliedMigrations.ToList(),
            PendingMigrations = pendingMigrations.ToList()
        };
    }
}

/// <summary>
/// Database information for diagnostics
/// </summary>
public class DatabaseInfo
{
    public bool CanConnect { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public List<string> AppliedMigrations { get; set; } = new();
    public List<string> PendingMigrations { get; set; } = new();

    public bool HasPendingMigrations => PendingMigrations.Count > 0;
    public int TotalMigrations => AppliedMigrations.Count + PendingMigrations.Count;
}

