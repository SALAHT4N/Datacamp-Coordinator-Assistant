using DatacampAICoordinator.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DatacampAICoordinator.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DatacampDbContext>
{
    public DatacampDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatacampDbContext>();
        
        optionsBuilder.UseSqlite($"Data Source={SolutionPathHelper.GetDatabasePath()}");
        return new DatacampDbContext(optionsBuilder.Options);
    }
}