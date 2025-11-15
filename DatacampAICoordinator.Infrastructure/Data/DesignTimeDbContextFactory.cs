using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DatacampAICoordinator.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DatacampDbContext>
{
    public DatacampDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var databasePath = configuration["GeneralSettings:DatabasePath"] ?? "datacamp.db";
        
        var dbPath = Path.IsPathRooted(databasePath)
            ? databasePath
            : Path.Combine(AppContext.BaseDirectory, databasePath);

        var optionsBuilder = new DbContextOptionsBuilder<DatacampDbContext>();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        
        return new DatacampDbContext(optionsBuilder.Options);
    }
}