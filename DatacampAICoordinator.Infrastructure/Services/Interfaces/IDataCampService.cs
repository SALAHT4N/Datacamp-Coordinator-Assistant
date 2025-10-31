using DatacampAICoordinator.Infrastructure.DTOs;

namespace DatacampAICoordinator.Infrastructure.Services.Interfaces;

/// <summary>
/// Interface for DataCamp API service
/// </summary>
public interface IDataCampService
{
    /// <summary>
    /// Fetches all leaderboard entries across all pages
    /// </summary>
    /// <param name="cookieValue">The cookie value to include in the request header</param>
    /// <param name="group">The group name (default: gaza-sky-geeks-25-26)</param>
    /// <param name="team">The team name (default: nnu-team)</param>
    /// <param name="days">Number of days to filter (default: 30)</param>
    /// <param name="sortField">Field to sort by (default: xp)</param>
    /// <param name="sortOrder">Sort order (default: desc)</param>
    /// <returns>List of all leaderboard entries from all pages</returns>
    Task<List<LeaderboardEntry>> GetAllLeaderboardEntriesAsync(
        string cookieValue,
        string group = "gaza-sky-geeks-25-26",
        string team = "nnu-team",
        int days = 30,
        string sortField = "xp",
        string sortOrder = "desc");
}

