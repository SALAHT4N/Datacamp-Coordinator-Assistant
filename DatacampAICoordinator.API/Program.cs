using DatacampAICoordinator.Infrastructure.Configuration;
using DatacampAICoordinator.Infrastructure.Data;
using DatacampAICoordinator.Infrastructure.Repositories;
using DatacampAICoordinator.Infrastructure.Repositories.Interfaces;
using DatacampAICoordinator.Infrastructure.Services;
using DatacampAICoordinator.Infrastructure.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using RazorLight;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Bind configuration to strongly-typed settings objects
builder.Services.Configure<GeneralSettings>(builder.Configuration.GetSection("GeneralSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<DataCampSettings>(builder.Configuration.GetSection("DataCampSettings"));

// Get settings for validation and DbContext setup
var generalSettings = builder.Configuration.GetSection("GeneralSettings").Get<GeneralSettings>() ?? new GeneralSettings();
var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>() ?? new EmailSettings();
var dataCampSettings = builder.Configuration.GetSection("DataCampSettings").Get<DataCampSettings>() ?? new DataCampSettings();

// Setup database path relative to application directory
var dbPath = Path.IsPathRooted(generalSettings.DatabasePath)
    ? generalSettings.DatabasePath
    : Path.Combine(builder.Environment.ContentRootPath, generalSettings.DatabasePath);

var connectionString = $"Data Source={dbPath}";

// Register DbContext with SQLite
builder.Services.AddDbContext<DatacampDbContext>(options =>
    options.UseSqlite(connectionString));

// Register Repositories
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentDailyStatusRepository, StudentDailyStatusRepository>();
builder.Services.AddScoped<IStudentProgressRepository, StudentProgressRepository>();
builder.Services.AddScoped<IProcessRepository, ProcessRepository>();

// Register RazorLight Engine
builder.Services.AddSingleton<RazorLightEngine>(provider =>
{
    var templatesPath = Path.Combine(builder.Environment.ContentRootPath, "ReportTemplates");
    if (!Directory.Exists(templatesPath))
    {
        throw new DirectoryNotFoundException($"ReportTemplates folder not found at: {templatesPath}");
    }

    return new RazorLightEngineBuilder()
        .UseFileSystemProject(templatesPath)
        .UseMemoryCachingProvider()
        .Build();
});

// Register Services
builder.Services.AddHttpClient<IDataCampService, DataCampService>((serviceProvider, httpClient) =>
{
    // HttpClient is automatically injected by AddHttpClient
});

builder.Services.AddScoped<IDataCampService>(provider =>
{
    var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
    return new DataCampService(httpClient, dataCampSettings);
});

builder.Services.AddScoped<IStudentSyncService, StudentSyncService>();
builder.Services.AddScoped<IStatusRecordService, StatusRecordService>();
builder.Services.AddScoped<IProgressCalculationService, ProgressCalculationService>();
builder.Services.AddScoped<IReportPublisher>(provider => new EmailReportPublisher(emailSettings));
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IDataCampCoordinatorService, DataCampCoordinatorService>();

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DatacampDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Checking database at: {DbPath}", dbPath);
    
    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
    {
        logger.LogInformation("Applying {Count} pending migration(s)...", pendingMigrations.Count());
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Migrations applied successfully");
    }
    else
    {
        await dbContext.Database.EnsureCreatedAsync();
        logger.LogInformation("Database is up to date");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();