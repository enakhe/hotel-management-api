# Database Seed Data Documentation

## Overview

The application includes an idempotent seeding system that populates the database with essential default data. All seed operations can be run multiple times without creating duplicates.

## What Gets Seeded

### 1. Default Roles

**Standard Roles**:
- `SuperAdmin` - Full system access across all tenants
- `Administrator` - Full access within a tenant
- `Manager` - Hotel management capabilities
- `Receptionist` - Front desk operations
- `Housekeeping` - Housekeeping operations
- `Maintenance` - Maintenance operations
- `User` - Basic user access

### 2. Default Permissions

Permissions are organized by category:

**User Management**:
- `users.view` - View users
- `users.create` - Create users
- `users.edit` - Edit users
- `users.delete` - Delete users

**Branch Management**:
- `branches.view` - View branches
- `branches.create` - Create branches
- `branches.edit` - Edit branches
- `branches.delete` - Delete branches

**Room Management**:
- `rooms.view` - View rooms
- `rooms.create` - Create rooms
- `rooms.edit` - Edit rooms
- `rooms.delete` - Delete rooms

**Reservation Management**:
- `reservations.view` - View reservations
- `reservations.create` - Create reservations
- `reservations.edit` - Edit reservations
- `reservations.cancel` - Cancel reservations

**Reports**:
- `reports.view` - View reports
- `reports.generate` - Generate reports
- `reports.export` - Export reports

**Settings**:
- `settings.view` - View settings
- `settings.edit` - Edit settings

### 3. System Modules

**Required Modules**:
- `CORE` - Core hotel management features
- `RESERVATIONS` - Room reservation and booking management

**Optional Modules**:
- `POS` - Point of Sale system
- `HOUSEKEEPING` - Housekeeping task management
- `MAINTENANCE` - Maintenance request tracking
- `REPORTS` - Advanced reporting and analytics
- `CHANNEL_MGR` - Multi-channel distribution
- `GUEST_PORTAL` - Self-service guest portal

### 4. Subscription Plans

#### Starter Plan
**Target**: Small hotels and guesthouses

**Limits**:
- Max Users: 5
- Max Branches: 1
- Max Rooms: 25
- Max Reservations/Month: 100
- Storage: 5 GB

**Modules**: Core, Reservations

#### Professional Plan (Popular)
**Target**: Mid-size hotels

**Limits**:
- Max Users: 25
- Max Branches: 3
- Max Rooms: 100
- Max Reservations/Month: 1,000
- Storage: 25 GB

**Modules**: Core, Reservations, Reports, Housekeeping

#### Enterprise Plan
**Target**: Large hotels and chains

**Limits**:
- Max Users: Unlimited
- Max Branches: Unlimited
- Max Rooms: Unlimited
- Max Reservations/Month: Unlimited
- Storage: 100 GB

**Modules**: All modules included

## When Seeding Occurs

### Automatic Seeding

**Development Environment**:
- ✅ Runs automatically on application startup
- ✅ Executes after database migrations
- ✅ Logs all seeding operations

**Production Environment**:
- ❌ Does NOT run automatically
- ✅ Must be triggered manually
- ✅ Recommended during initial deployment only

### Manual Seeding

You can trigger seeding manually using:

```bash
# Using EF Core CLI (development)
dotnet ef database update -p src/Infrastructure -s src/Web

# The seed will run automatically in development after migrations
```

## Idempotent Design

All seed operations check for existing data before inserting:

```csharp
// Example: Role seeding
var existingRole = await _context.Roles
    .FirstOrDefaultAsync(r => r.Name == "Administrator");

if (existingRole == null)
{
    // Only create if it doesn't exist
    await _context.Roles.AddAsync(newRole);
}
```

**Benefits**:
- ✅ Safe to run multiple times
- ✅ No duplicate data created
- ✅ Updates can be added without breaking existing data
- ✅ Supports database resets in development

## Customizing Seed Data

### Adding New Roles

Edit `ApplicationDbContextSeed.cs`:

```csharp
private async Task SeedRolesAsync()
{
    var rolesToSeed = new[]
    {
        // ... existing roles
        new { Name = "YourNewRole", Description = "Description" }
    };
    
    // ... seeding logic
}
```

### Adding New Permissions

```csharp
private async Task SeedPermissionsAsync()
{
    var permissionsToSeed = new[]
    {
        // ... existing permissions
        new { 
            Name = "newfeature.view", 
            Description = "View new feature", 
            Category = "New Feature" 
        }
    };
    
    // ... seeding logic
}
```

### Adding New Plans

```csharp
private async Task SeedPlansAsync()
{
    var plansToSeed = new[]
    {
        // ... existing plans
        new 
        { 
            Name = "Custom Plan",
            Description = "Custom plan description",
            // ... other properties
        }
    };
    
    // ... seeding logic
}
```

## Super Admin Setup

### Creating Initial Super Admin

For first-time setup, you can create a super admin user:

```csharp
// In Program.cs or setup script
using var scope = app.Services.CreateScope();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContextSeed>>();

var seeder = new ApplicationDbContextSeed(context, logger);
await seeder.SeedSuperAdminAsync(
    userManager, 
    "admin@hotelmanagement.com", 
    "SecurePassword123!"
);
```

**Security Note**: Change the default password immediately after first login!

### Environment-Specific Super Admin

**appsettings.Development.json**:
```json
{
  "SeedData": {
    "SuperAdmin": {
      "Email": "dev@localhost.com",
      "Password": "Dev123!"
    }
  }
}
```

**⚠️ Never commit production credentials to source control!**

## Production Deployment

### Initial Deployment Checklist

1. **Deploy application** with database migrations
2. **Verify database** is created and migrations applied
3. **Trigger seed manually** (one-time):
   ```bash
   # Option 1: Using seeding endpoint (if implemented)
   POST /api/admin/seed
   
   # Option 2: Using custom CLI command
   dotnet run --project src/Web seed-database
   
   # Option 3: Using SQL script
   # Run generated seed script
   ```

4. **Create super admin user** manually
5. **Verify seeded data**:
   ```sql
   SELECT COUNT(*) FROM AspNetRoles;          -- Should have 7 roles
   SELECT COUNT(*) FROM Permissions;          -- Should have ~20 permissions
   SELECT COUNT(*) FROM Modules;              -- Should have 8 modules
   SELECT COUNT(*) FROM Plans;                -- Should have 3 plans
   ```

6. **Create first tenant** using super admin account

### Subsequent Deployments

- ✅ Seed runs only if data doesn't exist
- ✅ New roles/permissions are added automatically
- ✅ Existing data is not modified
- ✅ Safe to include in deployment pipeline

## Verifying Seed Data

### Using SQL Queries

```sql
-- Check roles
SELECT * FROM AspNetRoles ORDER BY Name;

-- Check permissions by category
SELECT Category, COUNT(*) as PermissionCount 
FROM Permissions 
GROUP BY Category 
ORDER BY Category;

-- Check modules
SELECT Code, Name, IsRequired, IsActive 
FROM Modules 
ORDER BY IsRequired DESC, Name;

-- Check plans with limits
SELECT p.Name, p.IsPopular, l.MaxUsers, l.MaxBranches, l.MaxRooms
FROM Plans p
INNER JOIN Limits l ON p.LimitsId = l.Id
ORDER BY p.Name;

-- Check plan-module associations
SELECT p.Name as PlanName, m.Name as ModuleName
FROM PlanModules pm
INNER JOIN Plans p ON pm.PlanId = p.Id
INNER JOIN Modules m ON pm.ModuleId = m.Id
ORDER BY p.Name, m.Name;
```

### Using Application Endpoints

```bash
# Get roles
GET /api/v1/administrator/role

# Get permissions (SuperAdmin only)
GET /cp/permissions

# Get plans
GET /cp/plan

# Get modules
GET /cp/module
```

## Troubleshooting

### Issue: Seed Not Running

**Symptoms**: Database has no default roles/permissions

**Solutions**:
1. Check environment: `ASPNETCORE_ENVIRONMENT=Development`
2. Check logs for seed execution
3. Manually trigger seed
4. Verify migrations have been applied

### Issue: Duplicate Key Errors

**Symptoms**: Error when seeding existing data

**Cause**: Seed logic not properly checking for existing data

**Solution**:
```csharp
// Ensure this pattern is used:
var existing = await _context.SomeTable
    .FirstOrDefaultAsync(x => x.UniqueField == value);

if (existing == null)
{
    // Create new
}
```

### Issue: Foreign Key Violations

**Symptoms**: Error creating related entities

**Cause**: Related entities not created in correct order

**Solution**: Ensure proper seeding order:
1. Independent entities first (Roles, Permissions, Modules)
2. Entities with simple FK (Limits)
3. Entities with complex relationships (Plans → PlanModules)

## Best Practices

### 1. Keep Seeds Minimal

Only seed data that is:
- ✅ Required for application to function
- ✅ Common across all installations
- ✅ Rarely changes

Don't seed:
- ❌ Tenant-specific data
- ❌ User-generated content
- ❌ Test data (use separate fixtures)

### 2. Use Meaningful Identifiers

```csharp
// Good: Use code/name for lookups
var module = await _context.Modules
    .FirstOrDefaultAsync(m => m.Code == "CORE");

// Bad: Hardcoded GUIDs
var module = await _context.Modules
    .FindAsync(Guid.Parse("..."));
```

### 3. Version Your Seed Data

```csharp
// Track seed version
public class SeedVersion
{
    public string Version { get; set; } = "1.0.0";
    public DateTime AppliedAt { get; set; }
}

// Only run new seeds if version changed
```

### 4. Log Everything

```csharp
_logger.LogInformation("+ Created role: {RoleName}", role.Name);
_logger.LogDebug("- Role already exists: {RoleName}", role.Name);
```

## Extending the Seed System

### Adding New Seed Methods

```csharp
public class ApplicationDbContextSeed
{
    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedPermissionsAsync();
        await SeedModulesAsync();
        await SeedPlansAsync();
        
        // Add your custom seed
        await SeedCustomDataAsync();
    }
    
    private async Task SeedCustomDataAsync()
    {
        // Your seeding logic
    }
}
```

### Environment-Specific Seeds

```csharp
public async Task SeedAsync(IHostEnvironment environment)
{
    // Common seeds
    await SeedRolesAsync();
    
    // Environment-specific
    if (environment.IsDevelopment())
    {
        await SeedTestDataAsync();
    }
    
    if (environment.IsProduction())
    {
        await SeedProductionSpecificAsync();
    }
}
```

## Related Documentation

- [Database Migrations](./DATABASE_MIGRATIONS.md)
- [Multi-Tenant Architecture](./MULTI_TENANT_ARCHITECTURE.md)
- [Role-Based Access Control](./RBAC.md)

