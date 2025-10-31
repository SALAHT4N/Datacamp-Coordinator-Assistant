namespace DatacampAICoordinator.Infrastructure.Models;

/// <summary>
/// Represents a daily snapshot of a student's status on DataCamp
/// </summary>
public class StudentDailyStatus
{
    /// <summary>
    /// Unique identifier for the daily status record
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Foreign key to the Student
    /// </summary>
    public int StudentId { get; set; }
    
    /// <summary>
    /// Total XP (experience points) the student has earned
    /// </summary>
    public int Xp { get; set; }
    
    /// <summary>
    /// Number of courses completed or in progress
    /// </summary>
    public int Courses { get; set; }
    
    /// <summary>
    /// Number of chapters completed
    /// </summary>
    public int Chapters { get; set; }
    
    /// <summary>
    /// Date when the student last earned XP
    /// </summary>
    public DateTime? LastXpDate { get; set; }
    
    /// <summary>
    /// Date when this status was recorded
    /// </summary>
    public DateTime Date { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Foreign key to the Process that created this record
    /// </summary>
    public int? ProcessId { get; set; }
    
    // Navigation properties
    public Student Student { get; set; } = null!;
}

