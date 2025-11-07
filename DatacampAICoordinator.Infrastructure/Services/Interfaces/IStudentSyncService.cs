using DatacampAICoordinator.Infrastructure.DTOs;
using DatacampAICoordinator.Infrastructure.Models;

namespace DatacampAICoordinator.Infrastructure.Services.Interfaces;

/// <summary>
/// Service for synchronizing students from DataCamp leaderboard entries to the database
/// </summary>
public interface IStudentSyncService
{
    /// <summary>
    /// Syncs students from leaderboard entries to the database
    /// Creates new students if they don't exist
    /// </summary>
    /// <param name="leaderboardEntries">Leaderboard entries from DataCamp</param>
    /// <returns>Dictionary mapping DatacampId to system StudentId</returns>
    Task<Dictionary<int, int>> SyncStudentsAsync(List<LeaderboardEntry> leaderboardEntries);
}