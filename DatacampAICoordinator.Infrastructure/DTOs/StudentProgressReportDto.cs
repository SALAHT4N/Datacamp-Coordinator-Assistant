namespace DatacampAICoordinator.Infrastructure.DTOs;

/// <summary>
/// DTO for a single student progress entry with student details
/// </summary>
public class StudentProgressEntryDto
{
    // Student Progress fields
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int DifferenceOfCourses { get; set; }
    public int DifferenceOfChapters { get; set; }
    public int DifferenceOfXp { get; set; }
    public string? Notes { get; set; }
    
    // Student fields
    public int DatacampId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastXpDate { get; set; }
}

/// <summary>
/// DTO for students who haven't earned XP recently
/// </summary>
public class InactiveStudentDto
{
    public int StudentId { get; set; }
    public int DatacampId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public DateTime? LastXpDate { get; set; }
    public int DaysSinceLastXp { get; set; }
}

/// <summary>
/// DTO for student progress reports containing process information and a list of progress entries
/// </summary>
public class StudentProgressReportDto
{
    // Process information at the top level
    public int ProcessId { get; set; }
    public DateTime ProcessRunDate { get; set; }
    public DateTime ProcessInitialDate { get; set; }
    public DateTime ProcessFinalDate { get; set; }
    
    // List of student progress entries with student details
    public List<StudentProgressEntryDto> ProgressEntries { get; set; } = new();
    
    // List of students who haven't earned XP in a specified number of days
    public List<InactiveStudentDto> InactiveStudents { get; set; } = new();
    
    // Threshold for inactive students (default: 4 days)
    public int InactiveThresholdDays { get; set; } = 4;
}
