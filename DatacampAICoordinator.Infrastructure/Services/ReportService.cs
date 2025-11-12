using DatacampAICoordinator.Infrastructure.Data;
using DatacampAICoordinator.Infrastructure.DTOs;
using DatacampAICoordinator.Infrastructure.Models;
using DatacampAICoordinator.Infrastructure.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatacampAICoordinator.Infrastructure.Services;

/// <summary>
/// Service implementation for generating and publishing reports using database queries
/// </summary>
public class ReportService : IReportService
{
    private readonly DatacampDbContext _dbContext;

    public ReportService(DatacampDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Generates and publishes a report based on the current data for a specific process.
    /// Uses LINQ queries to retrieve data from the database.
    /// </summary>
    /// <param name="processId">The ID of the process to generate the report for</param>
    public async Task GenerateAndPublishReportAsync(int processId)
    {
        var report = await GenerateReportData(processId);
        
        await Task.CompletedTask;
    }

    private async Task<StudentProgressReportDto> GenerateReportData(int processId)
    {
        // First, get the process information
        var process = await _dbContext.Process
            .FirstOrDefaultAsync(p => p.ProcessId == processId);

        if (process == null)
        {
            throw new InvalidOperationException($"Process with ID {processId} not found");
        }

        // Query to get all StudentProgress records for the given processId
        var progressEntries = await _dbContext.StudentProgress
            .Where(sp => sp.ProcessId == processId)
            .Include(sp => sp.Student)
            .Select(sp => new StudentProgressEntryDto
            {
                // StudentProgress fields
                Id = sp.Id,
                StudentId = sp.StudentId,
                DifferenceOfCourses = sp.DifferenceOfCourses,
                DifferenceOfChapters = sp.DifferenceOfChapters,
                DifferenceOfXp = sp.DifferenceOfXp,
                Notes = sp.Notes,
                
                // Student fields
                DatacampId = sp.Student.DatacampId,
                FullName = sp.Student.FullName,
                Email = sp.Student.Email,
                Slug = sp.Student.Slug,
                IsActive = sp.Student.IsActive
            })
            .ToListAsync();

        // Build the report with process info at top level and list of entries
        return new StudentProgressReportDto
        {
            ProcessId = process.ProcessId,
            ProcessRunDate = process.DateOfRun,
            ProcessInitialDate = process.InitialDate,
            ProcessFinalDate = process.FinalDate,
            ProgressEntries = progressEntries
        };
    }
}
