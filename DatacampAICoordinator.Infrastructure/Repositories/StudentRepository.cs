using DatacampAICoordinator.Infrastructure.Data;
using DatacampAICoordinator.Infrastructure.Models;
using DatacampAICoordinator.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatacampAICoordinator.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Student entity operations
/// </summary>
public class StudentRepository : IStudentRepository
{
    private readonly DatacampDbContext _context;

    public StudentRepository(DatacampDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<int> BulkCreateAsync(IEnumerable<Student> students, CancellationToken cancellationToken = default)
    {
        if (students == null)
            throw new ArgumentNullException(nameof(students));

        var studentList = students.ToList();
        if (!studentList.Any())
            return 0;

        await _context.Student.AddRangeAsync(studentList, cancellationToken);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Student>> BulkGetAsync(IEnumerable<int> datacampIds, CancellationToken cancellationToken = default)
    {
        if (datacampIds == null)
            throw new ArgumentNullException(nameof(datacampIds));

        var idList = datacampIds.ToList();
        if (!idList.Any())
            return Enumerable.Empty<Student>();

        return await _context.Student
            .Where(s => idList.Contains(s.DatacampId))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Student
            .ToListAsync(cancellationToken);
    }
}
