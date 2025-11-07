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
    
    /// <summary>
    /// Retrieves all student daily status records for a specific process
    /// </summary>
    /// <param name="processId">The process identifier to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of student daily status records associated with the process</returns>
    Task<IEnumerable<StudentDailyStatus>> GetByProcessIdAsync(int processId, CancellationToken cancellationToken = default);
}
