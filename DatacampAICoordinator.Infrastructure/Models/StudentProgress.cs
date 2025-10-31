namespace DatacampAICoordinator.Infrastructure.Models;

/// <summary>
/// Represents the progress difference between two time periods for a student
/// </summary>
public class StudentProgress
{
    /// <summary>
    /// Unique identifier for the progress record
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Foreign key to the Student
    /// </summary>
    public int StudentId { get; set; }
    
    /// <summary>
    /// Foreign key to the Process that calculated this progress
    /// </summary>
    public int ProcessId { get; set; }
    
    /// <summary>
    /// Difference in the number of courses compared to the previous period
    /// </summary>
    public int DifferenceOfCourses { get; set; }
    
    /// <summary>
    /// Difference in the number of chapters compared to the previous period
    /// </summary>
    public int DifferenceOfChapters { get; set; }
    
    /// <summary>
    /// Difference in XP compared to the previous period
    /// </summary>
    public int DifferenceOfXp { get; set; }
    
    /// <summary>
    /// Optional notes or remarks about this progress
    /// </summary>
    public string? Notes { get; set; }
    
    // Navigation properties
    public Student Student { get; set; } = null!;
    public Process Process { get; set; } = null!;
}

