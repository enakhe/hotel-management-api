using HotelManagement.Domain.Constants;
using HotelManagement.Domain.Entities.Configuration;
using HotelManagement.Domain.Entities.Data;
using HotelManagement.Domain.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Data;
public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default tenant
        var defaultTenant = new Tenant
        {
            Name = "Esmart Systems",
            Identifier = "esmart",
            Description = "Main Tenant for superadmin operations",
            Address = "Ikeja, Lagos, Nigeria",
            ContactNumber = "+2349069477106",
            Email = "contact@eitiltech.com",
            IsActive = true,
            TimeZone = "WAT",
            CurrencyCode = "NGN",
            LanguageCode = "en",
            Industry = "Technology",
            Country = "Nigeria",
            Region = "Lagos"
        };

        if (!_context.Tenants.Any(t => t.Identifier == defaultTenant.Identifier))
        {
            _context.Tenants.Add(defaultTenant);
            await _context.SaveChangesAsync();
        }
        else
        {
            defaultTenant = _context.Tenants.First(t => t.Identifier == defaultTenant.Identifier);
        }

        // Default roles
        var administratorRole = new ApplicationRole
        {
            Name = Roles.Administrator.ToString()
        };

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
        }

        var superAdminRole = new ApplicationRole
        {
            Name = Roles.SuperAdmin.ToString()
        };

        if (_roleManager.Roles.All(r => r.Name != superAdminRole.Name))
        {
            await _roleManager.CreateAsync(superAdminRole);
        }

        // Default branch
        var defaultBranch = new Branch
        {
            TenantId = defaultTenant.Id,
            Name = "Main Branch",
            Address = "Ikeja, Lagos, Nigeria",
            ContactNumber = "+2349069477106",
            Email = "contact@eitiltech.com",
            IsActive = true,
        };

        if (!_context.Branches.Any(b => b.Name == defaultBranch.Name))
        {
            _context.Branches.Add(defaultBranch);
            await _context.SaveChangesAsync();
        }
        else
        {
            defaultBranch = _context.Branches.First(b => b.Name == defaultBranch.Name);
        }

        // Default users
        var administrator = new ApplicationUser
        {
            TenantId = defaultTenant.Id,
            UserName = "admin@eitiltech.com",
            Email = "admin@eitiltech.com",
            FirstName = "Admin",
            LastName = "User",
            MiddleName = "A",
            FullName = "Admin User A",
            EmailConfirmed = true,
            IsActive = true,
            PhoneNumberConfirmed = true,
            PhoneNumber = "+2349069477106",
            BranchId = defaultBranch.Id,
            Branch = defaultBranch
        };

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, "Administrator1!");
            if (!string.IsNullOrWhiteSpace(administratorRole.Name))
            {
                await _userManager.AddToRolesAsync(administrator, new[] { administratorRole.Name });
            }
        }

        // SuperAdmin user (not tied to any tenant or branch)
        var superAdmin = new ApplicationUser
        {
            UserName = "superadmin@eitiltech.com",
            Email = "superadmin@eitiltech.com",
            FirstName = "Super",
            LastName = "Admin",
            MiddleName = "A",
            FullName = "Super Admin A",
            EmailConfirmed = true,
            IsActive = true,
            PhoneNumberConfirmed = true,
            PhoneNumber = "+2349069477106",
            TenantId = defaultTenant.Id,
            BranchId = defaultBranch.Id,
            Branch = defaultBranch
        };

        if (_userManager.Users.All(u => u.UserName != superAdmin.UserName))
        {
            await _userManager.CreateAsync(superAdmin, "SuperAdmin1!");
            if (!string.IsNullOrWhiteSpace(superAdminRole.Name))
            {
                await _userManager.AddToRolesAsync(superAdmin, [superAdminRole.Name]);
            }
        }
    }
}
