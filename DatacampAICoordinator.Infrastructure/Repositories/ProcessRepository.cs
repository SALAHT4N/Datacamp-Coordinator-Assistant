using DatacampAICoordinator.Infrastructure.Data;
using DatacampAICoordinator.Infrastructure.Models;
using DatacampAICoordinator.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatacampAICoordinator.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Process entity operations
/// </summary>
public class ProcessRepository : IProcessRepository
{
    private readonly DatacampDbContext _context;

    public ProcessRepository(DatacampDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<int> UpdateAsync(Process process, CancellationToken cancellationToken = default)
    {
        if (process == null)
            throw new ArgumentNullException(nameof(process));

        _context.Process.Update(process);
        return await _context.SaveChangesAsync(cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task<Process?> GetLatestProcessAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Process
            .OrderByDescending(p => p.DateOfRun)
            .FirstOrDefaultAsync(cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task<Process?> GetProcessByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var dateOnly = date.Date;
        return await _context.Process
            .Where(p => p.DateOfRun.Date == dateOnly)
            .OrderByDescending(p => p.DateOfRun)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
