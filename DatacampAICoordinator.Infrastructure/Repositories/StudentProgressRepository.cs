using DatacampAICoordinator.Infrastructure.Data;
using DatacampAICoordinator.Infrastructure.Models;
using DatacampAICoordinator.Infrastructure.Repositories.Interfaces;

namespace DatacampAICoordinator.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for StudentProgress entity operations
/// </summary>
public class StudentProgressRepository : IStudentProgressRepository
{
    private readonly DatacampDbContext _context;

    public StudentProgressRepository(DatacampDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<int> BulkCreateAsync(IEnumerable<StudentProgress> progressRecords, CancellationToken cancellationToken = default)
    {
        if (progressRecords == null)
            throw new ArgumentNullException(nameof(progressRecords));

        var recordsList = progressRecords.ToList();
        if (!recordsList.Any())
            return 0;

        await _context.StudentProgress.AddRangeAsync(recordsList, cancellationToken);
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

