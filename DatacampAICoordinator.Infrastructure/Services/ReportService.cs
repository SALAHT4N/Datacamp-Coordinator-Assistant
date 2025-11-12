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
    
    private readonly string ViewName = "StudentProgressReportV1.cshtml";
    
    public ReportService(DatacampDbContext dbContext, RazorLightEngine razorLightEngine)
    {
        _dbContext = dbContext;
        _razorLightEngine = razorLightEngine;
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
    }

    private async Task<StudentProgressReportDto> GenerateReportData(int processId)
    {
        var process = await _dbContext.Process
            .FirstOrDefaultAsync(p => p.ProcessId == processId);

        if (process == null)
        {
            throw new InvalidOperationException($"Process with ID {processId} not found");
        }

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
                IsActive = sp.Student.IsActive
            })
            .OrderByDescending(x => x.DifferenceOfXp)
            .ToListAsync();

        return new StudentProgressReportDto
        {
            ProcessId = process.ProcessId,
            ProcessRunDate = process.DateOfRun,
            ProcessInitialDate = process.InitialDate,
            ProcessFinalDate = process.FinalDate,
            ProgressEntries = progressEntries
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
