# Database Migrations Guide

## Overview

This application uses Entity Framework Core Migrations for database schema management. Migrations are applied automatically in development and manually in production environments.

## Migration Strategy by Environment

### Development Environment

**Strategy**: Automatic migrations on application startup

**Behavior**:
- ✅ Automatically applies pending migrations
- ✅ Creates database if it doesn't exist
- ✅ Logs detailed migration information
- ✅ Stops application if migration fails

**Configuration**: No configuration needed - automatic in Development

### Staging/Production Environments

**Strategy**: Manual migration with verification

**Behavior**:
- ⚠️ Warns about pending migrations on startup
- ❌ Does NOT apply migrations automatically
- ✅ Application starts normally
- ✅ Logs pending migration count

**Why Manual**: 
- Prevents accidental schema changes
- Allows migration review before applying
- Enables rollback planning
- Supports blue-green deployments

## Creating Migrations

### Prerequisites

Install EF Core tools:

```bash
dotnet tool install --global dotnet-ef
# Or update existing
dotnet tool update --global dotnet-ef
```

### Creating a New Migration

```bash
# Navigate to the solution root
cd path/to/api

# Create migration
dotnet ef migrations add MigrationName -p src/Infrastructure -s src/Web

# Example: Adding a new feature
dotnet ef migrations add AddCustomerPreferences -p src/Infrastructure -s src/Web
```

**Naming Conventions**:
- Use PascalCase
- Be descriptive: `AddEmailVerificationToUsers` not `Update1`
- Include context: `CreateReservationTables`, `AddIndexToTenants`

### Reviewing Generated Migration

```bash
# View migration file
code src/Infrastructure/Migrations/20231102_MigrationName.cs
```

**Check**:
- ✅ SQL operations are correct
- ✅ Indexes are created where needed
- ✅ No data loss operations (dropping columns with data)
- ✅ Default values for NOT NULL columns

## Applying Migrations

### Development

**Automatic**: Migrations apply on application startup

**Manual** (if needed):
```bash
dotnet ef database update -p src/Infrastructure -s src/Web
```

### Staging/Production

#### Option 1: Using dotnet ef (Recommended for small teams)

```bash
# Set environment variables
$env:ASPNETCORE_ENVIRONMENT="Production"

# Apply migrations
dotnet ef database update -p src/Infrastructure -s src/Web
```

#### Option 2: Using SQL Scripts (Recommended for enterprise)

```bash
# Generate SQL script
dotnet ef migrations script -p src/Infrastructure -s src/Web -o migration.sql

# Review script
code migration.sql

# Apply using SQL Server Management Studio or sqlcmd
sqlcmd -S server -d database -U username -P password -i migration.sql
```

#### Option 3: Using Azure DevOps/GitHub Actions

```yaml
# Azure DevOps Pipeline
- task: DotNetCoreCLI@2
  displayName: 'Apply EF Migrations'
  inputs:
    command: 'custom'
    custom: 'ef'
    arguments: 'database update --project src/Infrastructure --startup-project src/Web'
    workingDirectory: '$(Build.SourcesDirectory)'
```

### Rolling Back Migrations

```bash
# List all migrations
dotnet ef migrations list -p src/Infrastructure -s src/Web

# Roll back to specific migration
dotnet ef database update PreviousMigrationName -p src/Infrastructure -s src/Web

# Example: Roll back to InitialCreate
dotnet ef database update InitialCreate -p src/Infrastructure -s src/Web
```

## Migration Scripts

### Generating Idempotent Scripts

For repeatable deployments:

```bash
# Generate script that can be run multiple times
dotnet ef migrations script -i -p src/Infrastructure -s src/Web -o migration_idempotent.sql
```

### Generating Scripts for Specific Range

```bash
# From one migration to another
dotnet ef migrations script FromMigration ToMigration -p src/Infrastructure -s src/Web -o partial_migration.sql

# From specific migration to latest
dotnet ef migrations script 20231101_InitialCreate -p src/Infrastructure -s src/Web -o latest.sql
```

## Database Reset (Development Only)

### Reset Database

```bash
# Drop and recreate database
dotnet ef database drop -p src/Infrastructure -s src/Web -f
dotnet ef database update -p src/Infrastructure -s src/Web
```

### Using Code

```csharp
// In Program.cs or startup code (ONLY in Development)
if (app.Environment.IsDevelopment() && 
    app.Configuration.GetValue<bool>("ResetDatabase"))
{
    await DatabaseMigrationService.ResetDatabaseAsync(app.Services, app.Environment);
}
```

**appsettings.Development.json**:
```json
{
  "ResetDatabase": false  // Set to true to reset on startup
}
```

## Migration Best Practices

### 1. Always Test Migrations

```bash
# Create test database
dotnet ef database update -p src/Infrastructure -s src/Web --connection "Server=localhost;Database=Test_HotelManagement;..."

# Verify changes
sqlcmd -S localhost -d Test_HotelManagement -Q "SELECT * FROM sys.tables"
```

### 2. Review Before Commit

**Checklist**:
- [ ] Migration Up method is correct
- [ ] Migration Down method can rollback
- [ ] No data loss operations
- [ ] Indexes created for foreign keys
- [ ] Default values provided for new NOT NULL columns
- [ ] Migration tested locally

### 3. Handle Data Migrations Carefully

**Bad** (might lose data):
```csharp
migrationBuilder.DropColumn(
    name: "OldColumn",
    table: "Users");
```

**Good** (preserve data):
```csharp
// Step 1: Add new column
migrationBuilder.AddColumn<string>(
    name: "NewColumn",
    table: "Users",
    nullable: true);

// Step 2: Copy data
migrationBuilder.Sql(
    "UPDATE Users SET NewColumn = OldColumn");

// Step 3: Drop old column (in next migration after verification)
```

### 4. Use Transactions

EF Core migrations run in transactions by default. For custom SQL:

```csharp
migrationBuilder.Sql(
    "UPDATE LargeTable SET Status = 'Active' WHERE Status IS NULL",
    suppressTransaction: false);  // Use transaction
```

### 5. Add Indexes Concurrently (SQL Server 2014+)

```csharp
migrationBuilder.Sql(
    "CREATE INDEX IX_Users_Email ON Users(Email) WITH (ONLINE = ON)");
```

## Common Migration Scenarios

### Adding a New Table

```bash
# 1. Add entity to Domain layer
# 2. Add DbSet to ApplicationDbContext
# 3. Create migration
dotnet ef migrations add CreateRoomAmenities -p src/Infrastructure -s src/Web

# 4. Review and apply
dotnet ef database update -p src/Infrastructure -s src/Web
```

### Adding a Column

```bash
dotnet ef migrations add AddPhoneToUsers -p src/Infrastructure -s src/Web
```

**Generated**:
```csharp
migrationBuilder.AddColumn<string>(
    name: "Phone",
    table: "Users",
    type: "nvarchar(20)",
    maxLength: 20,
    nullable: true);
```

### Renaming a Column

**Don't** let EF generate a drop + add:

```csharp
// Manual migration
migrationBuilder.RenameColumn(
    name: "OldName",
    table: "Users",
    newName: "NewName");
```

### Adding an Index

```csharp
migrationBuilder.CreateIndex(
    name: "IX_Branches_TenantId",
    table: "Branches",
    column: "TenantId");
```

### Adding Foreign Key

```csharp
migrationBuilder.CreateIndex(
    name: "IX_Rooms_BranchId",
    table: "Rooms",
    column: "BranchId");

migrationBuilder.AddForeignKey(
    name: "FK_Rooms_Branches_BranchId",
    table: "Rooms",
    column: "BranchId",
    principalTable: "Branches",
    principalColumn: "Id",
    onDelete: ReferentialAction.Restrict);
```

## Continuous Integration/Deployment

### CI Pipeline

```yaml
name: Database Migration CI

on:
  pull_request:
    branches: [ main, develop ]

jobs:
  migration-check:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: '9.0.x'
    
    - name: Install EF Core tools
      run: dotnet tool install --global dotnet-ef
    
    - name: Check for pending migrations
      run: |
        dotnet ef migrations has-pending-model-changes -p src/Infrastructure -s src/Web
        if [ $? -eq 0 ]; then
          echo "⚠️ Model changes detected without migration"
          exit 1
        fi
    
    - name: Generate migration script
      run: dotnet ef migrations script -p src/Infrastructure -s src/Web -o migration.sql
    
    - name: Upload migration script
      uses: actions/upload-artifact@v2
      with:
        name: migration-script
        path: migration.sql
```

### CD Pipeline

```yaml
- name: Apply Database Migrations
  run: |
    dotnet ef database update -p src/Infrastructure -s src/Web --connection "${{ secrets.DB_CONNECTION_STRING }}"
  env:
    ASPNETCORE_ENVIRONMENT: Production
```

## Troubleshooting

### Issue: "Cannot connect to database"

**Solution**:
```bash
# Test connection
sqlcmd -S server -U username -P password -Q "SELECT 1"

# Check connection string in secrets
dotnet user-secrets list -p src/Web
```

### Issue: "A migration is pending"

**Solution**:
```bash
# Apply pending migrations
dotnet ef database update -p src/Infrastructure -s src/Web

# Or generate script for manual application
dotnet ef migrations script -p src/Infrastructure -s src/Web
```

### Issue: "The migration has already been applied"

**Solution**:
```bash
# Check migration history
SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId DESC

# Remove from history if needed (careful!)
DELETE FROM __EFMigrationsHistory WHERE MigrationId = '20231102_MigrationName'
```

### Issue: "Model incompatible with database"

**Solution**:
```bash
# Create new migration to sync
dotnet ef migrations add SyncModel -p src/Infrastructure -s src/Web

# Or reset database (dev only)
dotnet ef database drop -f -p src/Infrastructure -s src/Web
dotnet ef database update -p src/Infrastructure -s src/Web
```

## Monitoring

### Database Version Endpoint

Create a health check endpoint:

```csharp
app.MapGet("/api/health/database", async (ApplicationDbContext context) =>
{
    var info = await DatabaseMigrationService.GetDatabaseInfoAsync(context);
    return Results.Ok(new
    {
        CanConnect = info.CanConnect,
        AppliedMigrations = info.AppliedMigrations.Count,
        PendingMigrations = info.PendingMigrations.Count,
        Status = info.HasPendingMigrations ? "Warning" : "Healthy"
    });
});
```

### Application Insights

Log migration events:

```csharp
logger.LogInformation(
    "Database Migration Status: Applied={Applied}, Pending={Pending}",
    appliedCount,
    pendingCount);
```

## Production Deployment Checklist

- [ ] Generate migration script: `dotnet ef migrations script`
- [ ] Review script for data loss operations
- [ ] Backup production database
- [ ] Test migration on staging environment
- [ ] Schedule maintenance window (if needed)
- [ ] Apply migration to production
- [ ] Verify application functionality
- [ ] Monitor for errors
- [ ] Keep rollback script ready

## Rollback Strategy

### Automated Rollback Script

```sql
-- Rollback template
BEGIN TRANSACTION;

-- Remove migration from history
DELETE FROM __EFMigrationsHistory 
WHERE MigrationId = 'YourMigrationId';

-- Reverse migration operations
-- (Copy from Down() method)

-- Verify rollback
SELECT * FROM __EFMigrationsHistory;

ROLLBACK TRANSACTION;  -- or COMMIT after verification
```

## Additional Resources

- [EF Core Migrations Documentation](https://docs.microsoft.com/ef/core/managing-schemas/migrations/)
- [Database Providers](https://docs.microsoft.com/ef/core/providers/)
- [SQL Server Migration Best Practices](https://docs.microsoft.com/sql/relational-databases/policy-based-management/database-migration-best-practices)

