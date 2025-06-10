using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HotelManagement.Infrastructure.Data.Persistence;
public class AppDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Traverse up to the startup project directory (AppHost)
        var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Web"));

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.Development.json", optional: false)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("HotelManagementDb"));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
