using DatacampAICoordinator.Infrastructure.Configuration;
using DatacampAICoordinator.Infrastructure.Data;
using DatacampAICoordinator.Infrastructure.Repositories;
using DatacampAICoordinator.Infrastructure.Repositories.Interfaces;
using DatacampAICoordinator.Infrastructure.Services;
using DatacampAICoordinator.Infrastructure.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RazorLight;

Console.WriteLine("=== DataCamp AI Coordinator ===\n");

try
{
    // Load configuration from appsettings.json
    var configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    // Bind configuration to strongly-typed settings objects
    var appSettings = new AppSettings();
    configuration.Bind(appSettings);

    var generalSettings = appSettings.GeneralSettings;
    var emailSettings = appSettings.EmailSettings;
    var dataCampSettings = appSettings.DataCampSettings;

    // Validate required settings
    Console.WriteLine("Validating configuration...");
    if (string.IsNullOrEmpty(generalSettings.DataCampCookie))
    {
        Console.WriteLine("ERROR: DataCampCookie is not set in appsettings.json");
        Console.WriteLine("Please add your DataCamp session cookie to the AppSettings section.");
        Environment.Exit(1);
    }

    if (string.IsNullOrEmpty(emailSettings.SmtpPassword))
    {
        Console.WriteLine("WARNING: Email SmtpPassword is not set. Email notifications will fail.");
    }

    // Setup database path relative to executable
    var dbPath = Path.IsPathRooted(generalSettings.DatabasePath)
        ? generalSettings.DatabasePath
        : Path.Combine(AppContext.BaseDirectory, generalSettings.DatabasePath);

    var connectionString = $"Data Source={dbPath}";
    Console.WriteLine($"Database path: {dbPath}");

    // Initialize DbContext with connection string
    var optionsBuilder = new DbContextOptionsBuilder<DatacampDbContext>();
    optionsBuilder.UseSqlite(connectionString);
    var dbContext = new DatacampDbContext(optionsBuilder.Options);

    // Ensure database is created and apply pending migrations
    Console.WriteLine("Checking database...");
    if (!File.Exists(dbPath))
    {
        Console.WriteLine("Database not found. Creating new database...");
    }
    
    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
    {
        Console.WriteLine($"Applying {pendingMigrations.Count()} pending migration(s)...");
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("Migrations applied successfully.");
    }
    else
    {
        // Ensure database is created if no migrations exist
        await dbContext.Database.EnsureCreatedAsync();
        Console.WriteLine("Database is up to date.");
    }

    // Create repositories
    IStudentRepository studentRepository = new StudentRepository(dbContext);
    IStudentDailyStatusRepository studentDailyStatusRepository = new StudentDailyStatusRepository(dbContext);
    IStudentProgressRepository studentProgressRepository = new StudentProgressRepository(dbContext);
    IProcessRepository processRepository = new ProcessRepository(dbContext);

    // Create RazorLight Engine with ReportTemplates
    var templatesPath = Path.Combine(AppContext.BaseDirectory, "ReportTemplates");
    if (!Directory.Exists(templatesPath))
    {
        throw new DirectoryNotFoundException($"ReportTemplates folder not found at: {templatesPath}");
    }

    var razorLightEngine = new RazorLightEngineBuilder()
        .UseFileSystemProject(templatesPath)
        .UseMemoryCachingProvider()
        .Build();

    // Create services with injected configuration
    IDataCampService dataCampService = new DataCampService(new HttpClient(), dataCampSettings);
    IStudentSyncService studentSyncService = new StudentSyncService(studentRepository);
    IStatusRecordService statusRecordService = new StatusRecordService(studentDailyStatusRepository, processRepository);
    IProgressCalculationService progressCalculationService = new ProgressCalculationService(studentDailyStatusRepository, studentProgressRepository);
    IReportPublisher reportPublisher = new EmailReportPublisher(emailSettings);
    IReportService reportService = new ReportService(dbContext, razorLightEngine, reportPublisher);

    // Create coordinator service
    IDataCampCoordinatorService coordinatorService = new DataCampCoordinatorService(
        dataCampService,
        studentSyncService,
        statusRecordService,
        progressCalculationService,
        processRepository,
        reportService);

    // Check for command-line arguments to determine execution mode
    if (args.Length >= 2)
    {
        // Date range report mode
        if (!DateTime.TryParse(args[0], out var startDate))
        {
            Console.WriteLine($"ERROR: Invalid start date format '{args[0]}'. Expected format: yyyy-MM-dd");
            Environment.Exit(1);
        }

        if (!DateTime.TryParse(args[1], out var endDate))
        {
            Console.WriteLine($"ERROR: Invalid end date format '{args[1]}'. Expected format: yyyy-MM-dd");
            Environment.Exit(1);
        }

        // Generate date range report
        var success = await coordinatorService.GenerateDateRangeReportAsync(startDate, endDate);
        
        if (success)
        {
            Console.WriteLine("\n=== Date Range Report Complete ===");
        }
        else
        {
            Console.WriteLine("\n=== Date Range Report Failed ===");
            Environment.Exit(1);
        }
    }
    else
    {
        // Normal sync mode
        Console.WriteLine("\n=== Starting Data Synchronization ===");
        Console.WriteLine("Usage: To generate date range report, run with: <start-date> <end-date>");
        Console.WriteLine("Example: DatacampAICoordinator.exe 2024-01-01 2024-01-15\n");
        
        // Execute the full sync workflow
        var progressCount = await coordinatorService.ExecuteFullSyncAsync(generalSettings.DataCampCookie);

        Console.WriteLine($"\n=== Sync Complete ===");
        Console.WriteLine($"Progress records created: {progressCount}");
    }
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"ERROR: Configuration file not found - {ex.Message}");
    Console.WriteLine("Make sure appsettings.json is in the same directory as the executable.");
    Environment.Exit(1);
}
catch (Exception ex)
{
    Console.WriteLine($"\n=== ERROR ===");
    Console.WriteLine($"An error occurred: {ex.Message}");
    Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");
    Environment.Exit(1);
}
