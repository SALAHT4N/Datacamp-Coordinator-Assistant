using DatacampAICoordinator.Infrastructure.DTOs;
using DatacampAICoordinator.Infrastructure.Models;
using DatacampAICoordinator.Infrastructure.Repositories.Interfaces;
using DatacampAICoordinator.Infrastructure.Services.Interfaces;

namespace DatacampAICoordinator.Infrastructure.Services;

/// <summary>
/// Service for creating and storing student daily status records
/// </summary>
public class StatusRecordService : IStatusRecordService
{
    private readonly IStudentDailyStatusRepository _studentDailyStatusRepository;
    private readonly IProcessRepository _processRepository;

    public StatusRecordService(
        IStudentDailyStatusRepository studentDailyStatusRepository,
        IProcessRepository processRepository)
    {
        _studentDailyStatusRepository = studentDailyStatusRepository;
        _processRepository = processRepository;
    }

    /// <summary>
    /// Creates status records from leaderboard entries and stores them in the database
    /// </summary>
    public async Task<(Process Process, List<StudentDailyStatus> StatusRecords)> CreateAndStoreStatusRecordsAsync(
        List<LeaderboardEntry> leaderboardEntries,
        Dictionary<int, int> datacampIdToSystemIdMap,
        Process? previousProcess)
    {
        var now = DateTime.Now;

        var process = new Process
        {
            DateOfRun = now,
            InitialDate = previousProcess?.DateOfRun ?? DateTime.MinValue,
            FinalDate = now,
            Status = ProcessStatus.Pending
        };

        var createdStatuses = leaderboardEntries.Select(entry => new StudentDailyStatus
        {
            Chapters = entry.Chapters,
            Courses = entry.Courses,
            LastXpDate = string.IsNullOrEmpty(entry.LastXp) ? null : DateTime.Parse(entry.LastXp),
            Xp = entry.Xp,
            StudentId = datacampIdToSystemIdMap[entry.User.Id],
            Date = now,
            Process = process,
        }).ToList();

        var storedRecords = await _studentDailyStatusRepository.BulkCreateAsync(createdStatuses);

        process.Status = ProcessStatus.Completed;
        process.RecordsProcessed = storedRecords;

        await _processRepository.UpdateAsync(process);

        return (process, createdStatuses);
    }
}

