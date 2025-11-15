using DatacampAICoordinator.Infrastructure.Models;

namespace DatacampAICoordinator.Infrastructure.Services.Interfaces;

/// <summary>
/// Service for calculating and storing student progress by comparing current and previous status
/// </summary>
public interface IProgressCalculationService
{
    /// <summary>
    /// Calculates progress by comparing current status with previous status and stores the results
    /// </summary>
    /// <param name="currentProcess">The current process that was just completed</param>
    /// <param name="currentStatusRecords">Current status records</param>
    /// <param name="previousProcess">Previous process to compare against</param>
    /// <returns>Number of progress records created</returns>
    Task<int> CalculateAndStoreProgressAsync(
        Process currentProcess,
        List<StudentDailyStatus> currentStatusRecords,
        Process previousProcess);

    /// <summary>
    /// Calculates progress between two processes for a date range without storing to database
    /// </summary>
    /// <param name="startProcess">The earlier process (start date)</param>
    /// <param name="endProcess">The later process (end date)</param>
    /// <returns>List of calculated progress records</returns>
    Task<List<StudentProgress>> CalculateProgressBetweenProcessesAsync(
        Process startProcess,
        Process endProcess);
}
