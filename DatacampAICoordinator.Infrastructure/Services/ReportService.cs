using DatacampAICoordinator.Infrastructure.Data;
using DatacampAICoordinator.Infrastructure.DTOs;
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
        
        await _reportPublisher.PublishAsync(renderedReport, processId);
    }

    private async Task<StudentProgressReportDto> GenerateReportData(int processId)
    {
        var process = await _dbContext.Process
            .FirstOrDefaultAsync(p => p.ProcessId == processId);

        if (process == null)
        {
            throw new InvalidOperationException($"Process with ID {processId} not found");
        }

        // Get the most recent daily status for each student to retrieve LastXpDate
        var latestDailyStatuses = await _dbContext.StudentDailyStatus
            .GroupBy(sds => sds.StudentId)
            .Select(g => g.OrderByDescending(sds => sds.Date).FirstOrDefault())
            .ToDictionaryAsync(sds => sds!.StudentId, sds => sds);

        var progressEntries = await _dbContext.StudentProgress
            .Where(sp => sp.ProcessId == processId)
            .Include(sp => sp.Student)
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

        // Calculate inactive students - those whose LastXpDate is older than ProcessFinalDate by the threshold
        var thresholdDate = process.FinalDate.AddDays(-InactiveThresholdDays);
        
        var inactiveStudents = await _dbContext.Student
            .Where(s => s.IsActive)
            .Select(s => new
            {
                Student = s,
                LatestStatus = _dbContext.StudentDailyStatus
                    .Where(sds => sds.StudentId == s.Id)
                    .OrderByDescending(sds => sds.Date)
                    .FirstOrDefault()
            })
            .Where(x => x.LatestStatus != null && 
                       x.LatestStatus.LastXpDate.HasValue && 
                       x.LatestStatus.LastXpDate.Value < thresholdDate)
            .Select(x => new InactiveStudentDto
            {
                StudentId = x.Student.Id,
                DatacampId = x.Student.DatacampId,
                FullName = x.Student.FullName,
                Email = x.Student.Email,
                Slug = x.Student.Slug,
                LastXpDate = x.LatestStatus!.LastXpDate,
                DaysSinceLastXp = (int)(process.FinalDate - x.LatestStatus.LastXpDate!.Value).TotalDays
            })
            .OrderByDescending(x => x.DaysSinceLastXp)
            .ToListAsync();

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
    
    private async Task SaveHtmlAsync(int processId, string html)
    {
        var fileName = $"StudentProgressReport_{processId}_{DateTime.UtcNow:yyyyMMddHHmmss}.html";
        var path = Path.Combine(AppContext.BaseDirectory, "ReportsOut");
        Directory.CreateDirectory(path);
        await File.WriteAllTextAsync(Path.Combine(path, fileName), html);
    }
}
