using HotelManagement.Domain.Constants;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Data;

/// <summary>
/// Seeds initial data into the database
/// All seed operations are idempotent - they can be run multiple times safely
/// </summary>
public class ApplicationDbContextSeed
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApplicationDbContextSeed> _logger;

    public ApplicationDbContextSeed(ApplicationDbContext context, ILogger<ApplicationDbContextSeed> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seeds all initial data
    /// </summary>
    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting database seed...");

        try
        {
            await SeedRolesAsync();
            await SeedPermissionsAsync();
            await SeedModulesAsync();
            // Note: Plan seeding requires manual setup due to complex relationships
            // await SeedPlansAsync();

            _logger.LogInformation("✓ Database seed completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error occurred while seeding database");
            throw;
        }
    }

    /// <summary>
    /// Seeds default roles
    /// </summary>
    private async Task SeedRolesAsync()
    {
        _logger.LogInformation("Seeding roles...");

        var rolesToSeed = new[]
        {
            new { Name = Roles.SuperAdmin, Description = "Super Administrator with full system access" },
            new { Name = Roles.Administrator, Description = "Tenant Administrator with full tenant access" },
            new { Name = "Manager", Description = "Hotel Manager" },
            new { Name = "Receptionist", Description = "Front Desk Receptionist" },
            new { Name = "Housekeeping", Description = "Housekeeping Staff" },
            new { Name = "Maintenance", Description = "Maintenance Staff" },
            new { Name = "User", Description = "Standard User" }
        };

        foreach (var roleData in rolesToSeed)
        {
            var existingRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == roleData.Name);

            if (existingRole == null)
            {
                var role = new ApplicationRole
                {
                    Id = Guid.NewGuid(),
                    Name = roleData.Name,
                    NormalizedName = roleData.Name.ToUpperInvariant(),
                    Description = roleData.Description,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };

                await _context.Roles.AddAsync(role);
                _logger.LogInformation("  + Created role: {Role}", roleData.Name);
            }
            else
            {
                _logger.LogDebug("  - Role already exists: {Role}", roleData.Name);
            }
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds default permissions
    /// </summary>
    private async Task SeedPermissionsAsync()
    {
        _logger.LogInformation("Seeding permissions...");

        var permissionsToSeed = new[]
        {
            // User Management
            new { Name = "users.view", Description = "View users" },
            new { Name = "users.create", Description = "Create users" },
            new { Name = "users.edit", Description = "Edit users" },
            new { Name = "users.delete", Description = "Delete users" },
            
            // Branch Management
            new { Name = "branches.view", Description = "View branches" },
            new { Name = "branches.create", Description = "Create branches" },
            new { Name = "branches.edit", Description = "Edit branches" },
            new { Name = "branches.delete", Description = "Delete branches" },
            
            // Room Management
            new { Name = "rooms.view", Description = "View rooms" },
            new { Name = "rooms.create", Description = "Create rooms" },
            new { Name = "rooms.edit", Description = "Edit rooms" },
            new { Name = "rooms.delete", Description = "Delete rooms" },
            
            // Reservation Management
            new { Name = "reservations.view", Description = "View reservations" },
            new { Name = "reservations.create", Description = "Create reservations" },
            new { Name = "reservations.edit", Description = "Edit reservations" },
            new { Name = "reservations.cancel", Description = "Cancel reservations" },
            
            // Reports
            new { Name = "reports.view", Description = "View reports" },
            new { Name = "reports.generate", Description = "Generate reports" },
            new { Name = "reports.export", Description = "Export reports" },
            
            // Settings
            new { Name = "settings.view", Description = "View settings" },
            new { Name = "settings.edit", Description = "Edit settings" }
        };

        foreach (var permData in permissionsToSeed)
        {
            var existingPerm = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Name == permData.Name);

            if (existingPerm == null)
            {
                var permission = new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = permData.Name,
                    Description = permData.Description
                };

                await _context.Permissions.AddAsync(permission);
                _logger.LogInformation("  + Created permission: {Permission}", permData.Name);
            }
            else
            {
                _logger.LogDebug("  - Permission already exists: {Permission}", permData.Name);
            }
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds default modules
    /// </summary>
    private async Task SeedModulesAsync()
    {
        _logger.LogInformation("Seeding modules...");

        var modulesToSeed = new[]
        {
            new
            {
                Name = "Core",
                Category = "CORE",
                Description = "Core hotel management features",
                IsCore = true
            },
            new
            {
                Name = "Reservations",
                Category = "RESERVATIONS",
                Description = "Room reservation and booking management",
                IsCore = true
            },
            new
            {
                Name = "Point of Sale",
                Category = "POS",
                Description = "Restaurant and bar POS system",
                IsCore = false
            },
            new
            {
                Name = "Housekeeping",
                Category = "HOUSEKEEPING",
                Description = "Housekeeping task management",
                IsCore = false
            },
            new
            {
                Name = "Maintenance",
                Category = "MAINTENANCE",
                Description = "Maintenance request tracking",
                IsCore = false
            },
            new
            {
                Name = "Reports & Analytics",
                Category = "REPORTS",
                Description = "Advanced reporting and analytics",
                IsCore = false
            },
            new
            {
                Name = "Channel Manager",
                Category = "CHANNEL_MGR",
                Description = "Multi-channel distribution management",
                IsCore = false
            },
            new
            {
                Name = "Guest Portal",
                Category = "GUEST_PORTAL",
                Description = "Self-service guest portal",
                IsCore = false
            }
        };

        foreach (var moduleData in modulesToSeed)
        {
            var existingModule = await _context.Modules
                .FirstOrDefaultAsync(m => m.Category == moduleData.Category);

            if (existingModule == null)
            {
                var module = new Domain.Entities.Module
                {
                    Id = Guid.NewGuid(),
                    Name = moduleData.Name,
                    Category = moduleData.Category,
                    Description = moduleData.Description,
                    IsActive = true,
                    IsCore = moduleData.IsCore,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Modules.AddAsync(module);
                _logger.LogInformation("  + Created module: {Module}", moduleData.Name);
            }
            else
            {
                _logger.LogDebug("  - Module already exists: {Module}", moduleData.Name);
            }
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds default subscription plans
    /// TODO: Implement plan seeding after entity structure is finalized
    /// </summary>
    private async Task SeedPlansAsync()
    {
        _logger.LogInformation("Plan seeding is not yet implemented");
        await Task.CompletedTask;

        /* TODO: Fix and uncomment after entity alignment
        _logger.LogInformation("Seeding plans...");

        // Get modules for plan associations
        var coreModule = await _context.Modules.FirstOrDefaultAsync(m => m.Category == "CORE");
        var reservationsModule = await _context.Modules.FirstOrDefaultAsync(m => m.Category == "RESERVATIONS");
        var reportsModule = await _context.Modules.FirstOrDefaultAsync(m => m.Category == "REPORTS");
        var channelMgrModule = await _context.Modules.FirstOrDefaultAsync(m => m.Category == "CHANNEL_MGR");

        var plansToSeed = new[]
        {
            new 
            { 
                Name = "Starter",
                Description = "Perfect for small hotels and guesthouses",
                Currency = "USD",
                BillingCycle = BillingCycle.Monthly,
                IsPopular = false,
                Limits = new
                {
                    MaxUsers = 5,
                    MaxBranches = 1,
                    MaxRooms = 25,
                    MaxReservationsPerMonth = 100,
                    StorageGB = 5
                },
                ModuleCodes = new[] { "CORE", "RESERVATIONS" }
            },
            new 
            { 
                Name = "Professional",
                Description = "Ideal for mid-size hotels",
                Currency = "USD",
                BillingCycle = BillingCycle.Monthly,
                IsPopular = true,
                Limits = new
                {
                    MaxUsers = 25,
                    MaxBranches = 3,
                    MaxRooms = 100,
                    MaxReservationsPerMonth = 1000,
                    StorageGB = 25
                },
                ModuleCodes = new[] { "CORE", "RESERVATIONS", "REPORTS", "HOUSEKEEPING" }
            },
            new 
            { 
                Name = "Enterprise",
                Description = "Full-featured solution for large hotels and chains",
                Currency = "USD",
                BillingCycle = BillingCycle.Monthly,
                IsPopular = false,
                Limits = new
                {
                    MaxUsers = -1, // Unlimited
                    MaxBranches = -1,
                    MaxRooms = -1,
                    MaxReservationsPerMonth = -1,
                    StorageGB = 100
                },
                ModuleCodes = new[] { "CORE", "RESERVATIONS", "REPORTS", "HOUSEKEEPING", "MAINTENANCE", "CHANNEL_MGR", "GUEST_PORTAL" }
            }
        };

        foreach (var planData in plansToSeed)
        {
            var existingPlan = await _context.Plans
                .FirstOrDefaultAsync(p => p.Name == planData.Name);

            if (existingPlan == null)
            {
                // Create limits
                var limits = new Limits
                {
                    Id = Guid.NewGuid(),
                    MaxUsers = planData.Limits.MaxUsers,
                    MaxBranches = planData.Limits.MaxBranches,
                    MaxRooms = planData.Limits.MaxRooms,
                    MaxReservationsPerMonth = planData.Limits.MaxReservationsPerMonth,
                    StorageGB = planData.Limits.StorageGB,
                    ApiCallsPerDay = 10000,
                    CanExportData = true,
                    CanUseApi = true
                };

                await _context.Limits.AddAsync(limits);

                // Create plan
                var plan = new Plan
                {
                    Id = Guid.NewGuid(),
                    Name = planData.Name,
                    Description = planData.Description,
                    Currency = planData.Currency,
                    BillingCycle = planData.BillingCycle,
                    IsActive = true,
                    IsPopular = planData.IsPopular,
                    LimitsId = limits.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Plans.AddAsync(plan);
                await _context.SaveChangesAsync(); // Save to get plan ID

                // Associate modules with plan
                foreach (var moduleCode in planData.ModuleCodes)
                {
                    var module = await _context.Modules.FirstOrDefaultAsync(m => m.Code == moduleCode);
                    if (module != null)
                    {
                        var planModule = new PlanModule
                        {
                            Id = Guid.NewGuid(),
                            PlanId = plan.Id,
                            ModuleId = module.Id,
                            IsIncluded = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _context.PlanModules.AddAsync(planModule);
                    }
                }

                _logger.LogInformation("  + Created plan: {Plan}", planData.Name);
            }
            else
            {
                _logger.LogDebug("  - Plan already exists: {Plan}", planData.Name);
            }
        }

        await _context.SaveChangesAsync();
        */
    }

    /// <summary>
    /// Seeds a super admin user (for initial setup only)
    /// </summary>
    public async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager, string email, string password)
    {
        _logger.LogInformation("Checking for super admin user...");

        var existingSuperAdmin = await userManager.FindByEmailAsync(email);

        if (existingSuperAdmin == null)
        {
            var superAdmin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = "Super",
                LastName = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(superAdmin, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdmin, Roles.SuperAdmin);
                _logger.LogInformation("✓ Super admin user created: {Email}", email);
            }
            else
            {
                _logger.LogError("Failed to create super admin: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            _logger.LogInformation("Super admin user already exists");
        }
    }
}

