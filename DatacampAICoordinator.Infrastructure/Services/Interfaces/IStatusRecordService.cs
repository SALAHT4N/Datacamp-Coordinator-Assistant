using DatacampAICoordinator.Infrastructure.DTOs;
using DatacampAICoordinator.Infrastructure.Models;

namespace DatacampAICoordinator.Infrastructure.Services.Interfaces;

/// <summary>
/// Service for creating and storing student daily status records
/// </summary>
public interface IStatusRecordService
{
    /// <summary>
    /// Creates status records from leaderboard entries and stores them in the database
    /// </summary>
    /// <param name="leaderboardEntries">Leaderboard entries from DataCamp</param>
    /// <param name="datacampIdToSystemIdMap">Mapping of DataCamp IDs to system Student IDs</param>
    /// <param name="previousProcess">The previous process (if any) to track progress from</param>
    /// <returns>Tuple containing the created process and list of status records</returns>
    Task<(Process Process, List<StudentDailyStatus> StatusRecords)> CreateAndStoreStatusRecordsAsync(
        List<LeaderboardEntry> leaderboardEntries,
        Dictionary<int, int> datacampIdToSystemIdMap,
        Process? previousProcess);
}

