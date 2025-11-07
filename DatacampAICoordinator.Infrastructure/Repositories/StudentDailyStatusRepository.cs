using DatacampAICoordinator.Infrastructure.Data;
using DatacampAICoordinator.Infrastructure.Models;
using DatacampAICoordinator.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatacampAICoordinator.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for StudentDailyStatus entity operations
/// </summary>
public class StudentDailyStatusRepository : IStudentDailyStatusRepository
{
    private readonly DatacampDbContext _context;

    public StudentDailyStatusRepository(DatacampDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<int> BulkCreateAsync(IEnumerable<StudentDailyStatus> dailyStatuses, CancellationToken cancellationToken = default)
    {
        if (dailyStatuses == null)
            throw new ArgumentNullException(nameof(dailyStatuses));

        var statusList = dailyStatuses.ToList();
        if (!statusList.Any())
            return 0;

        await _context.StudentDailyStatus.AddRangeAsync(statusList, cancellationToken);
        return await _context.SaveChangesAsync(cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<StudentDailyStatus>> GetByProcessIdAsync(int processId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentDailyStatus
            .Where(sds => sds.ProcessId == processId)
            .OrderBy(sds => sds.Date)
            .ToListAsync(cancellationToken);
    }
}
