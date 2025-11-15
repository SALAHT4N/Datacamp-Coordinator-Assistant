using DatacampAICoordinator.Infrastructure.Repositories.Interfaces;
using DatacampAICoordinator.Infrastructure.Services.Interfaces;

namespace DatacampAICoordinator.Infrastructure.Services;

/// <summary>
/// High-level orchestrator service that coordinates the full DataCamp sync workflow
/// </summary>
public class DataCampCoordinatorService : IDataCampCoordinatorService
{
    private readonly IDataCampService _dataCampService;
    private readonly IStudentSyncService _studentSyncService;
    private readonly IStatusRecordService _statusRecordService;
    private readonly IProgressCalculationService _progressCalculationService;
    private readonly IProcessRepository _processRepository;
    private readonly IReportService _reportService;

    public DataCampCoordinatorService(
        IDataCampService dataCampService,
        IStudentSyncService studentSyncService,
        IStatusRecordService statusRecordService,
        IProgressCalculationService progressCalculationService,
        IProcessRepository processRepository,
        IReportService reportService)
    {
        _dataCampService = dataCampService;
        _studentSyncService = studentSyncService;
        _statusRecordService = statusRecordService;
        _progressCalculationService = progressCalculationService;
        _processRepository = processRepository;
        _reportService = reportService;
    }

    /// <summary>
    /// Executes the complete workflow:
    /// 1. Fetches leaderboard data from DataCamp
    /// 2. Syncs students to database
    /// 3. Creates and stores status records
    /// 4. Calculates and stores progress (if previous data exists)
    /// 5. Generates and publishes report
    /// </summary>
    public async Task<int> ExecuteFullSyncAsync(
        string cookieValue,
        string group = "gaza-sky-geeks-25-26",
        string team = "nnu-team",
        int days = 36500)
    {
        // Step 1: Fetch all leaderboard entries from DataCamp
        var allEntries = await _dataCampService.GetAllLeaderboardEntriesAsync(
            cookieValue, group, team, days);

        Console.WriteLine($"\nTotal entries fetched: {allEntries.Count}");

        // Step 2: Sync students to database
        var datacampIdToSystemIdMap = await _studentSyncService.SyncStudentsAsync(allEntries);
        
        Console.WriteLine($"Synced {datacampIdToSystemIdMap.Count} students to database");

        // Step 3: Get the latest process before creating new records
        var previousProcess = await _processRepository.GetLatestProcessAsync();

        // Step 4: Create and store status records
        var (currentProcess, statusRecords) = await _statusRecordService.CreateAndStoreStatusRecordsAsync(
            allEntries, datacampIdToSystemIdMap, previousProcess);

        Console.WriteLine($"Created {statusRecords.Count} status records");

        // Step 5: Calculate and store progress if previous data exists
        var progressCount = 0;
        if (previousProcess != null)
        {
            progressCount = await _progressCalculationService.CalculateAndStoreProgressAsync(
                currentProcess, statusRecords, previousProcess);
            
            Console.WriteLine($"Calculated and stored {progressCount} progress records");
        }
        else
        {
            Console.WriteLine("No previous process found - skipping progress calculation");
        }

        // Step 6: Generate and publish report for the current process
        await _reportService.GenerateAndPublishReportAsync(currentProcess.ProcessId);
        Console.WriteLine("Report generated and published");

        return progressCount;
    }

    /// <summary>
    /// Generates a report for a specific date range by comparing data between two processes
    /// </summary>
    public async Task<bool> GenerateDateRangeReportAsync(DateTime startDate, DateTime endDate)
    {
        Console.WriteLine($"\n=== Generating Date Range Report ===");
        Console.WriteLine($"Start Date: {startDate:yyyy-MM-dd}");
        Console.WriteLine($"End Date: {endDate:yyyy-MM-dd}");

        // Step 1: Find process for start date
        var startProcess = await _processRepository.GetProcessByDateAsync(startDate);
        if (startProcess == null)
        {
            Console.WriteLine($"ERROR: No process found for start date {startDate:yyyy-MM-dd}");
            Console.WriteLine("Cannot generate report - insufficient data.");
            return false;
        }
        Console.WriteLine($"Found start process: ProcessId={startProcess.ProcessId}, RunDate={startProcess.DateOfRun:yyyy-MM-dd HH:mm:ss}");

        // Step 2: Find process for end date
        var endProcess = await _processRepository.GetProcessByDateAsync(endDate);
        if (endProcess == null)
        {
            Console.WriteLine($"ERROR: No process found for end date {endDate:yyyy-MM-dd}");
            Console.WriteLine("Cannot generate report - insufficient data.");
            return false;
        }
        Console.WriteLine($"Found end process: ProcessId={endProcess.ProcessId}, RunDate={endProcess.DateOfRun:yyyy-MM-dd HH:mm:ss}");

        // Step 3: Calculate progress between the two processes
        var progressRecords = await _progressCalculationService.CalculateProgressBetweenProcessesAsync(
            startProcess, endProcess);
        Console.WriteLine($"Calculated {progressRecords.Count} progress records for date range");

        // Step 4: Generate and publish the report
        await _reportService.GenerateAndPublishDateRangeReportAsync(progressRecords, startDate, endDate);
        Console.WriteLine("Date range report generated and published");

        return true;
    }
}
