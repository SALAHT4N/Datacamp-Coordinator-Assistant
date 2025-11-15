using DatacampAICoordinator.Infrastructure.Models;
using DatacampAICoordinator.Infrastructure.Repositories.Interfaces;
using DatacampAICoordinator.Infrastructure.Services.Interfaces;

namespace DatacampAICoordinator.Infrastructure.Services;

/// <summary>
/// Service for calculating and storing student progress by comparing current and previous status
/// </summary>
public class ProgressCalculationService : IProgressCalculationService
{
    private readonly IStudentDailyStatusRepository _studentDailyStatusRepository;
    private readonly IStudentProgressRepository _studentProgressRepository;

    public ProgressCalculationService(
        IStudentDailyStatusRepository studentDailyStatusRepository,
        IStudentProgressRepository studentProgressRepository)
    {
        _studentDailyStatusRepository = studentDailyStatusRepository;
        _studentProgressRepository = studentProgressRepository;
    }

    /// <summary>
    /// Calculates progress by comparing current status with previous status and stores the results
    /// </summary>
    public async Task<int> CalculateAndStoreProgressAsync(
        Process currentProcess,
        List<StudentDailyStatus> currentStatusRecords,
        Process previousProcess)
    {
        var previousStatusRecords = await _studentDailyStatusRepository.GetByProcessIdAsync(previousProcess.ProcessId);
        
        var previousRecordsDict = previousStatusRecords.ToDictionary(x => x.StudentId);
        
        var emptyStatus = new StudentDailyStatus
        {
            Courses = 0,
            Chapters = 0,
            Xp = 0,
            Date = DateTime.MinValue
        };
        
        var progressRecords = currentStatusRecords
            .Select(current =>
            {
                var previous = previousRecordsDict.GetValueOrDefault(current.StudentId, emptyStatus);
                
                return new StudentProgress
                {
                    StudentId = current.StudentId,
                    ProcessId = currentProcess.ProcessId,
                    DifferenceOfCourses = current.Courses - previous.Courses,
                    DifferenceOfChapters = current.Chapters - previous.Chapters,
                    DifferenceOfXp = current.Xp - previous.Xp,
                    Notes = previous.Date == DateTime.MinValue 
                        ? "First time tracking - no previous data" 
                        : $"Progress from {previous.Date:yyyy-MM-dd} to {current.Date:yyyy-MM-dd}"
                };
            })
            .ToList();
        
        var savedProgressCount = await _studentProgressRepository.BulkCreateAsync(progressRecords);
        return savedProgressCount;
    }

    /// <summary>
    /// Calculates progress between two processes for a date range without storing to database
    /// </summary>
    public async Task<List<StudentProgress>> CalculateProgressBetweenProcessesAsync(
        Process startProcess,
        Process endProcess)
    {
        var startStatusRecords = await _studentDailyStatusRepository.GetByProcessIdAsync(startProcess.ProcessId);
        var endStatusRecords = await _studentDailyStatusRepository.GetByProcessIdAsync(endProcess.ProcessId);
        
        var startRecordsDict = startStatusRecords.ToDictionary(x => x.StudentId);
        
        var emptyStatus = new StudentDailyStatus
        {
            Courses = 0,
            Chapters = 0,
            Xp = 0,
            Date = DateTime.MinValue
        };
        
        var progressRecords = endStatusRecords
            .Select(endStatus =>
            {
                var startStatus = startRecordsDict.GetValueOrDefault(endStatus.StudentId, emptyStatus);
                
                return new StudentProgress
                {
                    StudentId = endStatus.StudentId,
                    ProcessId = endProcess.ProcessId,
                    DifferenceOfCourses = endStatus.Courses - startStatus.Courses,
                    DifferenceOfChapters = endStatus.Chapters - startStatus.Chapters,
                    DifferenceOfXp = endStatus.Xp - startStatus.Xp,
                    Notes = startStatus.Date == DateTime.MinValue 
                        ? "First time tracking - no previous data" 
                        : $"Progress from {startStatus.Date:yyyy-MM-dd} to {endStatus.Date:yyyy-MM-dd}"
                };
            })
            .ToList();
        
        return progressRecords;
    }
}
