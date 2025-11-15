namespace DatacampAICoordinator.Infrastructure.DTOs;

/// <summary>
/// DTO for date range report that includes progress data with student information
/// </summary>
public class DateRangeReportDto
{
    /// <summary>
    /// Start date of the reporting period
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// End date of the reporting period
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// When the report was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; }
    
    /// <summary>
    /// List of student progress entries for the date range
    /// </summary>
    public List<StudentProgressEntryDto> ProgressEntries { get; set; } = new();
}