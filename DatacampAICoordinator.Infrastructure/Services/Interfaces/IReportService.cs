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
    
    /// <summary>
    /// Generates and publishes a report from pre-calculated progress records for a date range
    /// </summary>
    /// <param name="progressRecords">The progress records to include in the report</param>
    /// <param name="startDate">The start date of the reporting period</param>
    /// <param name="endDate">The end date of the reporting period</param>
    Task GenerateAndPublishDateRangeReportAsync(List<Models.StudentProgress> progressRecords, DateTime startDate, DateTime endDate);
}
