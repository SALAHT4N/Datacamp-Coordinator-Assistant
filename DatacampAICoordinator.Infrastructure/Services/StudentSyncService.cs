using DatacampAICoordinator.Infrastructure.Data;
using DatacampAICoordinator.Infrastructure.DTOs;
using DatacampAICoordinator.Infrastructure.Models;
using DatacampAICoordinator.Infrastructure.Repositories;
using DatacampAICoordinator.Infrastructure.Repositories.Interfaces;
using DatacampAICoordinator.Infrastructure.Services.Interfaces;

namespace DatacampAICoordinator.Infrastructure.Services;

/// <summary>
/// Service for synchronizing students from DataCamp leaderboard entries to the database
/// </summary>
public class StudentSyncService : IStudentSyncService
{
    private readonly IStudentRepository _studentRepository;

    public StudentSyncService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    /// <summary>
    /// Syncs students from leaderboard entries to the database
    /// Creates new students if they don't exist
    /// </summary>
    public async Task<Dictionary<int, int>> SyncStudentsAsync(List<LeaderboardEntry> leaderboardEntries)
    {
        var datacampIds = leaderboardEntries.Select(entry => entry.User.Id).ToList();
        
        var existingStudents = (await _studentRepository.BulkGetAsync(datacampIds)).ToList();
        var existingDatacampIds = existingStudents.Select(student => student.DatacampId).ToList();

        var newStudents = leaderboardEntries
            .Where(entry => !existingDatacampIds.Contains(entry.User.Id))
            .Select(entry => new Student
            {
                DatacampId = entry.User.Id,
                FullName = entry.User.FullName,
                Email = entry.User.Email,
                Slug = entry.User.Slug,
                IsActive = true,
                UpdatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        await _studentRepository.BulkCreateAsync(newStudents);

        // Return mapping of DatacampId to system StudentId
        return newStudents.Concat(existingStudents)
            .ToDictionary(x => x.DatacampId, x => x.Id);
    }
}
