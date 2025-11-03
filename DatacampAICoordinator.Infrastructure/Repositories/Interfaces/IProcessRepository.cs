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
}