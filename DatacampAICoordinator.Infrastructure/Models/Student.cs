namespace DatacampAICoordinator.Infrastructure.Models;

/// <summary>
/// Represents a student entity from the DataCamp leaderboard
/// </summary>
public class Student
{
    /// <summary>
    /// Unique identifier for the student
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Student's full name
    /// </summary>
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// Student's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// DataCamp username or profile identifier
    /// </summary>
    public string? Slug { get; set; }
    
    /// <summary>
    /// Date when the student record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Date when the student record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Indicates if the student is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<StudentDailyStatus> DailyStatuses { get; set; } = new List<StudentDailyStatus>();
    public ICollection<StudentProgress> ProgressRecords { get; set; } = new List<StudentProgress>();
}