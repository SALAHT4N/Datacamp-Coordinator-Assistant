using DatacampAICoordinator.Infrastructure.Models;

namespace DatacampAICoordinator.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Repository interface for Process entity operations
/// </summary>
public interface IProcessRepository
{
    /// <summary>
    /// Updates an existing process
    /// </summary>
    /// <param name="process">Process entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities updated</returns>
    Task<int> UpdateAsync(Process process, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves the latest process ordered by DateOfRun
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The most recent Process object, or null if no processes exist</returns>
    Task<Process?> GetLatestProcessAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finds a process by date (ignoring time component). If multiple processes exist for the same date,
    /// returns the latest one based on time.
    /// </summary>
    /// <param name="date">The date to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The matching Process object, or null if not found</returns>
    Task<Process?> GetProcessByDateAsync(DateTime date, CancellationToken cancellationToken = default);
}