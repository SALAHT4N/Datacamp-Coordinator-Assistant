using DatacampAICoordinator.Infrastructure.Models;

namespace DatacampAICoordinator.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Repository interface for StudentDailyStatus entity operations
/// </summary>
public interface IStudentDailyStatusRepository
{
    /// <summary>
    /// Creates multiple student daily status records in a single operation
    /// </summary>
    /// <param name="dailyStatuses">Collection of daily status records to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of records created</returns>
    Task<int> BulkCreateAsync(IEnumerable<StudentDailyStatus> dailyStatuses, CancellationToken cancellationToken = default);
}

