using DatacampAICoordinator.Infrastructure.Models;

namespace DatacampAICoordinator.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Repository interface for Student entity operations
/// </summary>
public interface IStudentRepository
{
    /// <summary>
    /// Creates multiple students in a single operation
    /// </summary>
    /// <param name="students">Collection of students to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of students created</returns>
    Task<int> BulkCreateAsync(IEnumerable<Student> students, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves multiple students by their DataCamp IDs
    /// </summary>
    /// <param name="datacampIds">Collection of DataCamp IDs to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of students</returns>
    Task<IEnumerable<Student>> BulkGetAsync(IEnumerable<int> datacampIds, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves all students
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all students</returns>
    Task<IEnumerable<Student>> GetAllAsync(CancellationToken cancellationToken = default);
}