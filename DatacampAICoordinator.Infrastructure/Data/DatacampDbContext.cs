using Microsoft.EntityFrameworkCore;
using DatacampAICoordinator.Infrastructure.Models;

namespace DatacampAICoordinator.Infrastructure.Data;

/// <summary>
/// Database context for the DataCamp AI Coordinator application
/// </summary>
public class DatacampDbContext : DbContext
{
    public DatacampDbContext()
    {
        Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
    }
    
    /// <summary>
    /// Constructor that accepts DbContextOptions
    /// </summary>
    /// <param name="options">Database context options</param>
    public DatacampDbContext(DbContextOptions<DatacampDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// DbSet for Student entities
    /// </summary>
    public DbSet<Student> Student { get; set; } = null!;

    /// <summary>
    /// DbSet for StudentDailyStatus entities
    /// </summary>
    public DbSet<StudentDailyStatus> StudentDailyStatus { get; set; } = null!;

    /// <summary>
    /// DbSet for StudentProgress entities
    /// </summary>
    public DbSet<StudentProgress> StudentProgress { get; set; } = null!;

    /// <summary>
    /// DbSet for Process entities
    /// </summary>
    public DbSet<Process> Process { get; set; } = null!;

    /// <summary>
    /// Configure model relationships and constraints
    /// </summary>
    /// <param name="modelBuilder">Model builder instance</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Student entity
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Slug).HasMaxLength(100);
            entity.HasIndex(e => e.Slug);
        });

        // Configure StudentDailyStatus entity
        modelBuilder.Entity<StudentDailyStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.StudentId, e.Date });
            
            entity.HasOne(e => e.Student)
                .WithMany(s => s.DailyStatuses)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure StudentProgress entity
        modelBuilder.Entity<StudentProgress>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.StudentId, e.ProcessId });
            
            entity.HasOne(e => e.Student)
                .WithMany(s => s.ProgressRecords)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Process)
                .WithMany(p => p.ProgressRecords)
                .HasForeignKey(e => e.ProcessId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Process entity
        modelBuilder.Entity<Process>(entity =>
        {
            entity.HasKey(e => e.ProcessId);
            entity.HasIndex(e => e.DateOfRun);
            entity.HasIndex(e => new { e.InitialDate, e.FinalDate });

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(50);
            
            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(2000);
            
            // Configure relationship with StudentProgress
            entity.HasMany(p => p.ProgressRecords)
                .WithOne(s => s.Process)
                .HasForeignKey(s => s.ProcessId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=datacamp.db");
        base.OnConfiguring(optionsBuilder);
    }
    
    public override int SaveChanges()
    {
        EnsurePragmas();
        return base.SaveChanges();
    }
    
    private void EnsurePragmas()    
    {
        Database.ExecuteSqlRaw("PRAGMA foreign_keys=ON;");
    }
}
