# Tenant Query Filters Documentation

## Overview

Tenant query filters provide automatic, database-level data isolation for multi-tenant entities. This ensures that users can only access data belonging to their tenant, preventing accidental or malicious cross-tenant data access.

## How It Works

### Automatic Filtering

All entities implementing `ITenantEntity` are automatically filtered at the EF Core level. When you query these entities, EF Core automatically adds a `WHERE TenantId = @currentTenantId` clause to the SQL query.

**Example**:
```csharp
// Your code:
var branches = await _context.Branches.ToListAsync();

// Generated SQL:
SELECT * FROM Branches WHERE TenantId = '...' 
```

### Filter Logic

The query filter implements the following logic:

```csharp
entity => 
    // Bypass if SuperAdmin
    (_superAdminContext != null && _superAdminContext.IsSuperAdmin) ||
    // Bypass if no tenant resolved
    (_tenantContext == null || !_tenantContext.IsResolved) ||
    // Filter by current tenant
    entity.TenantId == _tenantContext.TenantId
```

## SuperAdmin Bypass

SuperAdmins can access data across all tenants without being restricted by tenant filters.

### How SuperAdmin Bypass Works

1. **Automatic Detection**: When `ISuperAdminContext.IsSuperAdmin` is `true`, filters are automatically bypassed
2. **No Code Changes**: SuperAdmin controllers/services work normally without modification
3. **Audit Trail**: All SuperAdmin operations are logged separately

**Example**:
```csharp
// SuperAdmin context
public class TenantManagementController : ControllerBase
{
    // SuperAdmin can see all tenants across the system
    public async Task<ActionResult> GetAllTenants()
    {
        // Query filters are automatically bypassed for SuperAdmin
        var allTenants = await _context.Tenants.ToListAsync();
        return Ok(allTenants);
    }
}
```

## Manual Filter Control

### Ignoring Filters in Code

If you need to explicitly bypass filters in code (rare cases):

```csharp
// Bypass all query filters
var allBranchesAcrossTenants = await _context.Branches
    .IgnoreQueryFilters()
    .ToListAsync();

// Bypass and manually filter by specific tenant
var tenant1Branches = await _context.Branches
    .IgnoreQueryFilters()
    .Where(b => b.TenantId == specificTenantId)
    .ToListAsync();
```

**⚠️ Warning**: Only use `IgnoreQueryFilters()` when absolutely necessary and with proper authorization checks.

### Temporarily Disabling Filters

For admin operations that need cross-tenant access:

```csharp
public async Task<List<Branch>> GetBranchesForTenant(Guid tenantId)
{
    // Verify current user is authorized for cross-tenant access
    if (!_currentUser.IsInRole("SuperAdmin"))
    {
        throw new ForbiddenAccessException();
    }

    // Explicitly query specific tenant's data
    return await _context.Branches
        .IgnoreQueryFilters()
        .Where(b => b.TenantId == tenantId)
        .ToListAsync();
}
```

## Entities with Query Filters

Query filters are applied to all entities implementing `ITenantEntity`:

- ✅ `Branch`
- ✅ `Room`
- ✅ `Reservation`
- ✅ `AuditLog`
- ✅ `AuditLogDetail`
- ✅ `Permission`
- ✅ `RolePermission`
- ✅ Any custom tenant-aware entities

**Not Filtered** (system-wide entities):
- ❌ `Tenant` (registry of all tenants)
- ❌ `Plan` (subscription plans)
- ❌ `Module` (system modules)
- ❌ `License` (not tenant-specific)
- ❌ `SuperAdminAuditLog` (cross-tenant audit)

## Testing Data Isolation

### Unit Testing

```csharp
[Test]
public async Task GetBranches_OnlyReturnCurrentTenantData()
{
    // Arrange
    var tenant1Id = Guid.NewGuid();
    var tenant2Id = Guid.NewGuid();
    
    await _context.Branches.AddAsync(new Branch { TenantId = tenant1Id, Name = "T1 Branch" });
    await _context.Branches.AddAsync(new Branch { TenantId = tenant2Id, Name = "T2 Branch" });
    await _context.SaveChangesAsync();

    // Set current tenant to tenant1
    _tenantContext.SetTenant(tenant1Id, "tenant1");

    // Act
    var branches = await _context.Branches.ToListAsync();

    // Assert
    Assert.AreEqual(1, branches.Count);
    Assert.AreEqual(tenant1Id, branches[0].TenantId);
}
```

### Integration Testing

```csharp
[Test]
public async Task API_OnlyReturnsCurrentTenantData()
{
    // Create test data for multiple tenants
    var tenant1 = await CreateTenantAsync("tenant1");
    var tenant2 = await CreateTenantAsync("tenant2");

    // Make request as tenant1
    var response = await _client.GetAsync("/api/v1/administrator/branch", 
        headers: new { "X-Tenant-Identifier" = "tenant1" });

    // Verify only tenant1 data is returned
    var branches = await response.Content.ReadAsAsync<List<BranchDto>>();
    Assert.All(branches, b => Assert.Equal(tenant1.Id, b.TenantId));
}
```

## Performance Considerations

### Indexing

Query filters add WHERE clauses to all queries. Ensure `TenantId` columns are indexed:

```csharp
// In entity configuration
builder.HasIndex(e => e.TenantId)
    .HasDatabaseName("IX_EntityName_TenantId");

// Composite indexes for common queries
builder.HasIndex(e => new { e.TenantId, e.CreatedDate })
    .HasDatabaseName("IX_EntityName_TenantId_CreatedDate");
```

### Query Planning

EF Core includes the tenant filter in query plans:

```sql
-- Efficient: Index is used
SELECT * FROM Branches WHERE TenantId = @p0

-- Efficient: Composite index used
SELECT * FROM Branches WHERE TenantId = @p0 AND CreatedDate > @p1
```

### Caching Considerations

When caching tenant data:

```csharp
// ❌ BAD: Could cache data from wrong tenant
var cacheKey = "branches_all";

// ✅ GOOD: Include tenant ID in cache key
var cacheKey = $"branches_tenant_{_tenantContext.TenantId}";
```

## Security Best Practices

### 1. Trust but Verify

Even with automatic filters, validate tenant ownership in sensitive operations:

```csharp
public async Task<Branch> GetBranchAsync(Guid branchId)
{
    var branch = await _context.Branches.FindAsync(branchId);
    
    if (branch == null)
    {
        throw new NotFoundException();
    }

    // Redundant check (filter already applied) but good for defense-in-depth
    if (branch.TenantId != _tenantContext.TenantId)
    {
        throw new ForbiddenAccessException();
    }

    return branch;
}
```

### 2. Audit Cross-Tenant Access

Log all operations that bypass filters:

```csharp
public async Task<List<Branch>> GetAllTenantsDataAsync()
{
    _logger.LogWarning("Cross-tenant query executed by {User}", _currentUser.UserId);
    
    return await _context.Branches
        .IgnoreQueryFilters()
        .ToListAsync();
}
```

### 3. SuperAdmin Authorization

Verify SuperAdmin access before bypassing filters:

```csharp
[Authorize(Roles = "SuperAdmin")]
[Authorize(Policy = "RequireStepUpAuth")]
public class TenantManagementController : ControllerBase
{
    // SuperAdmin endpoints
}
```

## Troubleshooting

### Issue: No Data Returned

**Symptom**: Queries return empty results even though data exists

**Possible Causes**:
1. Tenant context not resolved
2. Wrong tenant ID in context
3. Data actually belongs to different tenant

**Solution**:
```csharp
// Check if tenant is resolved
if (!_tenantContext.IsResolved)
{
    _logger.LogWarning("Tenant context not resolved for query");
}

// Log current tenant
_logger.LogInformation("Querying as tenant {TenantId}", _tenantContext.TenantId);

// Temporarily bypass to debug
var allData = await _context.Branches.IgnoreQueryFilters().ToListAsync();
_logger.LogInformation("Total branches across all tenants: {Count}", allData.Count);
```

### Issue: SuperAdmin Seeing Filtered Data

**Symptom**: SuperAdmin can't see all tenant data

**Possible Causes**:
1. SuperAdmin context not set correctly
2. `IsSuperAdmin` is false

**Solution**:
```csharp
// Verify SuperAdmin context
_logger.LogInformation("IsSuperAdmin: {IsSuperAdmin}", 
    _superAdminContext.IsSuperAdmin);

// Check middleware execution
// Ensure SuperAdminMiddleware runs before queries
```

### Issue: Performance Degradation

**Symptom**: Queries slower after implementing filters

**Solution**:
```csharp
// Add indexes
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.Entity<Branch>()
        .HasIndex(e => e.TenantId);
        
    builder.Entity<Room>()
        .HasIndex(e => new { e.TenantId, e.BranchId });
}
```

## Migration Guide

### Adding Filters to Existing System

If adding tenant filters to an existing system:

1. **Backup Database**: Always backup before major changes
2. **Add TenantId**: Ensure all entities have TenantId populated
3. **Test Extensively**: Test all queries with filters active
4. **Monitor Performance**: Watch for slow queries after deployment

```sql
-- Verify all records have TenantId
SELECT COUNT(*) FROM Branches WHERE TenantId IS NULL;
SELECT COUNT(*) FROM Rooms WHERE TenantId IS NULL;

-- Add indexes
CREATE INDEX IX_Branches_TenantId ON Branches(TenantId);
CREATE INDEX IX_Rooms_TenantId ON Rooms(TenantId);
```

## Advanced Scenarios

### Multi-Level Tenant Hierarchies

For organizations with sub-tenants:

```csharp
// Custom filter for hierarchical tenants
builder.Entity<Branch>().HasQueryFilter(e => 
    _superAdminContext.IsSuperAdmin ||
    e.TenantId == _tenantContext.TenantId ||
    _tenantContext.ChildTenantIds.Contains(e.TenantId)
);
```

### Tenant-Specific Schemas

For complete database isolation:

```csharp
// Use separate schemas per tenant
protected override void OnModelCreating(ModelBuilder builder)
{
    var schema = _tenantContext.IsResolved 
        ? $"tenant_{_tenantContext.TenantIdentifier}"
        : "dbo";
        
    builder.HasDefaultSchema(schema);
}
```

## Monitoring and Auditing

### Track Filter Effectiveness

```csharp
public class QueryFilterMonitor
{
    public async Task LogQueryStats()
    {
        var stats = new
        {
            TotalQueries = _metrics.QueriesExecuted,
            FilteredQueries = _metrics.QueriesWithFilters,
            BypassedQueries = _metrics.QueriesWithIgnoreFilters,
            SuperAdminQueries = _metrics.SuperAdminQueries
        };
        
        _logger.LogInformation("Query Filter Stats: {@Stats}", stats);
    }
}
```

## Related Documentation

- [Multi-Tenant Architecture](./MULTI_TENANT_ARCHITECTURE.md)
- [Security Best Practices](./SECURITY_HEADERS.md)
- [Database Indexing](./DATABASE_OPTIMIZATION.md)

## References

- [EF Core Global Query Filters](https://docs.microsoft.com/ef/core/querying/filters)
- [Multi-Tenant Data Architecture](https://docs.microsoft.com/azure/architecture/guide/multitenant/approaches/data-considerations)

