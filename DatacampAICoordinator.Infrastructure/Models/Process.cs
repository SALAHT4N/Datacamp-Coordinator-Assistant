namespace DatacampAICoordinator.Infrastructure.Models;

/// <summary>
/// Represents a scraping process run that collects data from DataCamp
/// </summary>
public class Process
{
    /// <summary>
    /// Unique identifier for the process
    /// </summary>
    public int ProcessId { get; set; }
    
    /// <summary>
    /// Date and time when the process was executed
    /// </summary>
    public DateTime DateOfRun { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Start date of the data range being processed
    /// </summary>
    public DateTime InitialDate { get; set; }
    
    /// <summary>
    /// End date of the data range being processed
    /// </summary>
    public DateTime FinalDate { get; set; }
    
    /// <summary>
    /// Status of the process execution
    /// </summary>
    public ProcessStatus Status { get; set; } = ProcessStatus.Pending;
    
    /// <summary>
    /// Number of student records processed
    /// </summary>
    public int RecordsProcessed { get; set; }
    
    /// <summary>
    /// Error message if the process failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Date and time when the process started
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    public ICollection<StudentProgress> ProgressRecords { get; set; } = new List<StudentProgress>();
}
/// <summary>
/// Enum representing the status of a process
/// </summary>
public enum ProcessStatus
{
    Pending,
    Completed,
    Failed
}

