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

    public DataCampCoordinatorService(
        IDataCampService dataCampService,
        IStudentSyncService studentSyncService,
        IStatusRecordService statusRecordService,
        IProgressCalculationService progressCalculationService,
        IProcessRepository processRepository)
    {
        _dataCampService = dataCampService;
        _studentSyncService = studentSyncService;
        _statusRecordService = statusRecordService;
        _progressCalculationService = progressCalculationService;
        _processRepository = processRepository;
    }

    /// <summary>
    /// Executes the complete workflow:
    /// 1. Fetches leaderboard data from DataCamp
    /// 2. Syncs students to database
    /// 3. Creates and stores status records
    /// 4. Calculates and stores progress (if previous data exists)
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

        return progressCount;
    }
}

