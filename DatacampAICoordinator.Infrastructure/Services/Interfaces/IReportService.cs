namespace DatacampAICoordinator.Infrastructure.Services.Interfaces;

/// <summary>
/// Service interface for generating and publishing reports
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Generates and publishes a report based on the current data for a specific process.
    /// The implementation determines both the format and delivery method.
    /// </summary>
    /// <param name="processId">The ID of the process to generate the report for</param>
    Task GenerateAndPublishReportAsync(int processId);
}
