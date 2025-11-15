using DatacampAICoordinator.Infrastructure.Data;
using DatacampAICoordinator.Infrastructure.DTOs;
using DatacampAICoordinator.Infrastructure.Models;
using DatacampAICoordinator.Infrastructure.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using RazorLight;

namespace DatacampAICoordinator.Infrastructure.Services;

/// <summary>
/// Service implementation for generating and publishing reports using database queries
/// </summary>
public class ReportService : IReportService
{
    private readonly DatacampDbContext _dbContext;
    private readonly RazorLightEngine _razorLightEngine;
    private readonly IReportPublisher _reportPublisher;
    
    private readonly string ViewName = "StudentProgressReportV1.cshtml";
    private readonly string DateRangeViewName = "StudentProgressReportV2.cshtml";
    
    /// <summary>
    /// CSV file name containing student emails to filter the report
    /// </summary>
    private const string StudentFilterFileName = "StudentFilter.csv";
    
    /// <summary>
    /// Number of days since last XP to consider a student inactive
    /// </summary>
    private const int InactiveThresholdDays = 4;
    
    public ReportService(DatacampDbContext dbContext, RazorLightEngine razorLightEngine, IReportPublisher reportPublisher)
    {
        _dbContext = dbContext;
        _razorLightEngine = razorLightEngine;
        _reportPublisher = reportPublisher;
    }

    /// <summary>
    /// Generates and publishes a report based on the current data for a specific process.
    /// Uses LINQ queries to retrieve data from the database.
    /// </summary>
    /// <param name="processId">The ID of the process to generate the report for</param>
    public async Task GenerateAndPublishReportAsync(int processId)
    {
        var reportData = await GenerateReportData(processId);
        
        var renderedReport = await _razorLightEngine.CompileRenderAsync(ViewName, reportData);
        
        await SaveHtmlAsync(processId, renderedReport);
        
        await _reportPublisher.PublishAsync(renderedReport);
    }

    /// <summary>
    /// Generates and publishes a report from pre-calculated progress records for a date range
    /// </summary>
    /// <param name="progressRecords">The progress records to include in the report</param>
    /// <param name="startDate">The start date of the reporting period</param>
    /// <param name="endDate">The end date of the reporting period</param>
    public async Task GenerateAndPublishDateRangeReportAsync(List<StudentProgress> progressRecords, DateTime startDate, DateTime endDate)
    {
        var reportData = await GenerateDateRangeReportData(progressRecords, startDate, endDate);
        
        var renderedReport = await _razorLightEngine.CompileRenderAsync(DateRangeViewName, reportData);
        
        await SaveHtmlAsync($"DateRange_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}", renderedReport);
        
        await _reportPublisher.PublishAsync(renderedReport);
    }

    private async Task<StudentProgressReportDto> GenerateReportData(int processId)
    {
        var process = await _dbContext.Process
            .FirstOrDefaultAsync(p => p.ProcessId == processId);

        if (process == null)
        {
            throw new InvalidOperationException($"Process with ID {processId} not found");
        }

        var studentEmailFilter = await GetStudentEmailFilterAsync();
        
        LogFilteringStatus(studentEmailFilter);

        var latestDailyStatuses = await _dbContext.StudentDailyStatus
            .GroupBy(sds => sds.StudentId)
            .Select(g => g.OrderByDescending(sds => sds.Date).FirstOrDefault())
            .ToDictionaryAsync(sds => sds.StudentId, sds => sds);

        var progressEntries = await GetProgressEntriesAsync(processId, studentEmailFilter, latestDailyStatuses);
        var inactiveStudents = await GetInactiveStudentsAsync(process, studentEmailFilter, latestDailyStatuses);

        return new StudentProgressReportDto
        {
            ProcessId = process.ProcessId,
            ProcessRunDate = process.DateOfRun,
            ProcessInitialDate = process.InitialDate,
            ProcessFinalDate = process.FinalDate,
            ProgressEntries = progressEntries,
            InactiveStudents = inactiveStudents,
            InactiveThresholdDays = InactiveThresholdDays
        };
    }
    
    /// <summary>
    /// Generates report data from pre-calculated progress records for a date range
    /// </summary>
    private async Task<DateRangeReportDto> GenerateDateRangeReportData(List<StudentProgress> progressRecords, DateTime startDate, DateTime endDate)
    {
        var studentEmailFilter = await GetStudentEmailFilterAsync();
        
        LogFilteringStatus(studentEmailFilter);
        
        var filteredProgressRecords = progressRecords;
        if (studentEmailFilter != null && studentEmailFilter.Count > 0)
        {
            filteredProgressRecords = progressRecords
                .Where(pr => studentEmailFilter.Contains(pr.Student.Email))
                .ToList();
        }
        
        var progressEntries = filteredProgressRecords
            .Select(pr => new StudentProgressEntryDto
            {
                Id = pr.Id,
                StudentId = pr.StudentId,
                DifferenceOfCourses = pr.DifferenceOfCourses,
                DifferenceOfChapters = pr.DifferenceOfChapters,
                DifferenceOfXp = pr.DifferenceOfXp,
                Notes = pr.Notes,
                DatacampId = pr.Student.DatacampId,
                FullName = pr.Student.FullName,
                Email = pr.Student.Email,
                Slug = pr.Student.Slug,
                IsActive = pr.Student.IsActive,
            })
            .OrderByDescending(x => x.DifferenceOfXp)
            .ToList();

        return new DateRangeReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            GeneratedAt = DateTime.Now,
            ProgressEntries = progressEntries
        };
    }
    
    /// <summary>
    /// Logs the filtering status based on the student email filter
    /// </summary>
    private void LogFilteringStatus(HashSet<string>? studentEmailFilter)
    {
        if (studentEmailFilter != null && studentEmailFilter.Count > 0)
        {
            Console.WriteLine($"Filtering report to {studentEmailFilter.Count} student(s) from {StudentFilterFileName}");
        }
        else
        {
            Console.WriteLine("Generating report for all students");
        }
    }

    /// <summary>
    /// Reads student emails from the CSV filter file if it exists
    /// </summary>
    /// <returns>List of student emails to filter by, or null if no filter should be applied</returns>
    private async Task<HashSet<string>?> GetStudentEmailFilterAsync()
    {
        var filterFilePath = Path.Combine(AppContext.BaseDirectory, StudentFilterFileName);
        
        if (!File.Exists(filterFilePath))
        {
            return null;
        }

        var emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        try
        {
            var lines = await File.ReadAllLinesAsync(filterFilePath);
            
            foreach (var line in lines)
            {
                var email = line.Split(',')[0].Trim();
                
                if (!string.IsNullOrWhiteSpace(email))
                {
                    emails.Add(email);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to read student filter file '{filterFilePath}': {ex.Message}");
            return null;
        }
        
        return emails.Count > 0 ? emails : null;
    }

    /// <summary>
    /// Retrieves progress entries for students in the given process, optionally filtered by email
    /// </summary>
    private async Task<List<StudentProgressEntryDto>> GetProgressEntriesAsync(
        int processId, 
        HashSet<string>? studentEmailFilter,
        Dictionary<int, StudentDailyStatus?> latestDailyStatuses)
    {
        var progressQuery = _dbContext.StudentProgress
            .Where(sp => sp.ProcessId == processId)
            .Include(sp => sp.Student)
            .AsQueryable();
        
        if (studentEmailFilter != null && studentEmailFilter.Count > 0)
        {
            progressQuery = progressQuery.Where(sp => studentEmailFilter.Contains(sp.Student.Email));
        }

        var progressEntries = await progressQuery
            .Select(sp => new StudentProgressEntryDto
            {
                Id = sp.Id,
                StudentId = sp.StudentId,
                DifferenceOfCourses = sp.DifferenceOfCourses,
                DifferenceOfChapters = sp.DifferenceOfChapters,
                DifferenceOfXp = sp.DifferenceOfXp,
                Notes = sp.Notes,
                
                DatacampId = sp.Student.DatacampId,
                FullName = sp.Student.FullName,
                Email = sp.Student.Email,
                Slug = sp.Student.Slug,
                IsActive = sp.Student.IsActive,
                LastXpDate = latestDailyStatuses.ContainsKey(sp.StudentId) 
                    ? latestDailyStatuses[sp.StudentId]!.LastXpDate 
                    : null
            })
            .OrderByDescending(x => x.DifferenceOfXp)
            .ToListAsync();

        return progressEntries;
    }

    /// <summary>
    /// Retrieves inactive students based on the threshold date, optionally filtered by email
    /// </summary>
    private async Task<List<InactiveStudentDto>> GetInactiveStudentsAsync(
        Models.Process process,
        HashSet<string>? studentEmailFilter,
        Dictionary<int, StudentDailyStatus?> latestDailyStatuses)
    {
        var thresholdDate = process.FinalDate.AddDays(-InactiveThresholdDays);
        
        var inactiveStudentsQuery = _dbContext.Student
            .Where(s => s.IsActive)
            .AsQueryable();
        
        if (studentEmailFilter != null && studentEmailFilter.Count > 0)
        {
            inactiveStudentsQuery = inactiveStudentsQuery.Where(s => studentEmailFilter.Contains(s.Email));
        }
        
        var inactiveStudents = await inactiveStudentsQuery
            .Select(x => new InactiveStudentDto
            {
                StudentId = x.Id,
                DatacampId = x.DatacampId,
                FullName = x.FullName,
                Email = x.Email,
                Slug = x.Slug,
            })
            .ToListAsync();
        
        inactiveStudents = inactiveStudents
            .Where(x => 
                latestDailyStatuses.ContainsKey(x.StudentId) && 
                latestDailyStatuses[x.StudentId]!.LastXpDate < thresholdDate)
            .ToList();

        inactiveStudents.ForEach(x => 
        {
            x.LastXpDate = latestDailyStatuses[x.StudentId]!.LastXpDate;
            x.DaysSinceLastXp = (int)(process.FinalDate - x.LastXpDate!.Value).TotalDays;
        });

        return inactiveStudents
            .OrderByDescending(x => x.DaysSinceLastXp)
            .ToList();
    }
    
    private async Task SaveHtmlAsync(object identifier, string html)
    {
        var fileName = $"StudentProgressReport_{identifier}_{DateTime.UtcNow:yyyyMMddHHmmss}.html";
        var path = Path.Combine(AppContext.BaseDirectory, "ReportsOut");
        Directory.CreateDirectory(path);
        await File.WriteAllTextAsync(Path.Combine(path, fileName), html);
    }
}
