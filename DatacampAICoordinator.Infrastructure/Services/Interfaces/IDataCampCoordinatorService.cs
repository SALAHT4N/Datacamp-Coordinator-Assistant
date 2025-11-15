namespace DatacampAICoordinator.Infrastructure.Services.Interfaces;

/// <summary>
/// High-level orchestrator service that coordinates the full DataCamp sync workflow
/// </summary>
public interface IDataCampCoordinatorService
{
    /// <summary>
    /// Executes the complete workflow:
    /// 1. Fetches leaderboard data from DataCamp
    /// 2. Syncs students to database
    /// 3. Creates and stores status records
    /// 4. Calculates and stores progress (if previous data exists)
    /// </summary>
    /// <param name="cookieValue">DataCamp authentication cookie</param>
    /// <param name="group">The group name (default: gaza-sky-geeks-25-26)</param>
    /// <param name="team">The team name (default: nnu-team)</param>
    /// <param name="days">Number of days to filter (default: 30)</param>
    /// <returns>Number of progress records created (0 if this is the first run)</returns>
    Task<int> ExecuteFullSyncAsync(
        string cookieValue,
        string group = "gaza-sky-geeks-25-26",
        string team = "nnu-team",
        int days = 36500);
    
    /// <summary>
    /// Generates a report for a specific date range by comparing data between two processes
    /// </summary>
    /// <param name="startDate">The start date of the reporting period</param>
    /// <param name="endDate">The end date of the reporting period</param>
    /// <returns>True if report was generated successfully, false if insufficient data</returns>
    Task<bool> GenerateDateRangeReportAsync(DateTime startDate, DateTime endDate);
}
