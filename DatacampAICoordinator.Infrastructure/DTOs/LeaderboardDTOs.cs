namespace DatacampAICoordinator.Infrastructure.DTOs;

/// <summary>
/// Represents the complete API response from the DataCamp leaderboard endpoint
/// </summary>
public class LeaderboardResponse
{
    public List<LeaderboardEntry> Entries { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
}

/// <summary>
/// Represents a single entry in the leaderboard
/// </summary>
public class LeaderboardEntry
{
    public int Chapters { get; set; }
    public int Courses { get; set; }
    public string LastXp { get; set; } = string.Empty;
    public int Rank { get; set; }
    public UserInfo User { get; set; } = new();
    public int Xp { get; set; }
}

/// <summary>
/// Represents user information within a leaderboard entry
/// </summary>
public class UserInfo
{
    public string AvatarUrl { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
}

/// <summary>
/// Represents pagination metadata
/// </summary>
public class PaginationInfo
{
    public int CurrentPage { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}