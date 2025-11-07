using DatacampAICoordinator.Infrastructure.Models;

namespace DatacampAICoordinator.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Repository interface for StudentProgress entity operations
/// </summary>
public interface IStudentProgressRepository
{
    /// <summary>
    /// Creates multiple student progress records in a single operation
    /// </summary>
    /// <param name="progressRecords">Collection of progress records to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of records created</returns>
    Task<int> BulkCreateAsync(IEnumerable<StudentProgress> progressRecords, CancellationToken cancellationToken = default);
}