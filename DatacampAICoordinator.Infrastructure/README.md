# DataCamp AI Coordinator - Infrastructure Layer

The Infrastructure layer contains all the data access, domain models, external API integrations, and core business logic for the DataCamp AI Coordinator application.

## Overview

This layer is responsible for:
- Database access and Entity Framework Core configuration
- Domain entity definitions and relationships
- External API integrations (DataCamp API)
- Data transfer objects (DTOs) for API responses
- Service implementations for data collection

## Project Structure

```
DatacampAICoordinator.Infrastructure/
├── Data/
│   ├── DatacampDbContext.cs              # EF Core DbContext
│   ├── DatacampDbContextFactory.cs       # Design-time factory
│   └── Migrations/                       # EF Core migrations
│       ├── 20251031161121_InitialCreate.cs
│       ├── 20251031161121_InitialCreate.Designer.cs
│       └── DatacampDbContextModelSnapshot.cs
├── Models/
│   ├── Student.cs                        # Student entity
│   ├── StudentDailyStatus.cs            # Daily snapshot entity
│   ├── StudentProgress.cs               # Progress calculation entity
│   └── Process.cs                       # Process metadata entity
├── DTOs/
│   └── LeaderboardDTOs.cs               # API response models
├── Services/
│   ├── Interfaces/
│   │   └── IDataCampService.cs          # Service contract
│   └── DataCampService.cs               # DataCamp API implementation
└── DatacampAICoordinator.Infrastructure.csproj
```

## Dependencies

### NuGet Packages

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.10" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.10" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.10" />
```

### Framework
- .NET 9.0
- C# 12 with nullable reference types
- Implicit usings enabled

---

## Domain Models

### Student Entity

Represents a DataCamp student/learner.

```csharp
public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string? DataCampUsername { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public ICollection<StudentDailyStatus> DailyStatuses { get; set; }
    public ICollection<StudentProgress> ProgressRecords { get; set; }
}
```

**Constraints:**
- Email must be unique
- FullName max length: 200 characters
- Email max length: 200 characters

**Indexes:**
- Unique index on `Email`
- Index on `Slug`

### StudentDailyStatus Entity

Stores daily snapshots of student performance metrics.

```csharp
public class StudentDailyStatus
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int XP { get; set; }
    public int Courses { get; set; }
    public int Chapters { get; set; }
    public DateTime? LastXPDate { get; set; }
    public DateTime Date { get; set; }
    public int? ProcessId { get; set; }
    
    // Navigation properties
    public Student Student { get; set; }
    public Process? Process { get; set; }
}
```

**Relationships:**
- Many-to-One with Student (cascade delete)
- Many-to-One with Process (set null on delete)

**Indexes:**
- Composite index on `(StudentId, Date)` for efficient querying

### StudentProgress Entity

Tracks calculated differences in student metrics between time periods.

```csharp
public class StudentProgress
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int ProcessId { get; set; }
    public int DifferenceOfCourses { get; set; }
    public int DifferenceOfChapters { get; set; }
    public int DifferenceOfXP { get; set; }
    public DateTime CalculatedAt { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public Student Student { get; set; }
    public Process Process { get; set; }
}
```

**Relationships:**
- Many-to-One with Student (cascade delete)
- Many-to-One with Process (cascade delete)

**Indexes:**
- Composite index on `(StudentId, ProcessId)`

### Process Entity

Metadata about data collection process runs.

```csharp
public class Process
{
    public int ProcessId { get; set; }
    public DateTime DateOfRun { get; set; }
    public DateTime InitialDate { get; set; }
    public DateTime FinalDate { get; set; }
    public ProcessType ProcessType { get; set; }
    public ProcessStatus Status { get; set; }
    public int RecordsProcessed { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration { get; }  // Calculated property
    
    // Navigation properties
    public ICollection<StudentDailyStatus> DailyStatuses { get; set; }
    public ICollection<StudentProgress> ProgressRecords { get; set; }
}
```

**Enums:**

```csharp
public enum ProcessType
{
    Daily,
    Monthly,
    Custom
}

public enum ProcessStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled
}
```

**Constraints:**
- ErrorMessage max length: 2000 characters
- ProcessType stored as string (max 50 chars)
- Status stored as string (max 50 chars)

**Indexes:**
- Index on `DateOfRun`
- Composite index on `(InitialDate, FinalDate)`

---

## Database Schema

### Entity Relationship Diagram

```
┌─────────────┐
│   Student   │
│  (Primary)  │
└──────┬──────┘
       │
       │ 1:N
       │
       ├────────────────┬────────────────┐
       │                │                │
       ▼                ▼                │
┌──────────────────┐  ┌──────────────┐  │
│StudentDailyStatus│  │StudentProgress│  │
└──────┬───────────┘  └──────┬────────┘  │
       │                     │           │
       │ N:1                 │ N:1       │
       │                     │           │
       ▼                     ▼           │
    ┌─────────────────────────┐          │
    │       Process           │          │
    │  (Scheduling/Tracking)  │◄─────────┘
    └─────────────────────────┘
```

### Table Relationships

**Student ↔ StudentDailyStatus**
- Type: One-to-Many
- Foreign Key: `StudentDailyStatus.StudentId`
- Delete Behavior: Cascade

**Student ↔ StudentProgress**
- Type: One-to-Many
- Foreign Key: `StudentProgress.StudentId`
- Delete Behavior: Cascade

**Process ↔ StudentDailyStatus**
- Type: One-to-Many
- Foreign Key: `StudentDailyStatus.ProcessId`
- Delete Behavior: Set Null (preserves historical data)

**Process ↔ StudentProgress**
- Type: One-to-Many
- Foreign Key: `StudentProgress.ProcessId`
- Delete Behavior: Cascade

---

## Data Access Layer

### DbContext Configuration

The `DatacampDbContext` class extends `DbContext` and configures all entity mappings.

```csharp
public class DatacampDbContext : DbContext
{
    public DatacampDbContext(DbContextOptions<DatacampDbContext> options) 
        : base(options) { }

    public DbSet<Student> Student { get; set; }
    public DbSet<StudentDailyStatus> StudentDailyStatus { get; set; }
    public DbSet<StudentProgress> StudentProgress { get; set; }
    public DbSet<Process> Process { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Entity configurations
    }
}
```

**Key Features:**
- Constructor accepts `DbContextOptions` for external configuration
- DbSets use singular naming convention
- Explicit relationship configuration in `OnModelCreating`
- Fluent API for constraints and indexes

### Design-Time Factory

Required for EF Core migrations without a runtime configuration.

```csharp
public class DatacampDbContextFactory : IDesignTimeDbContextFactory<DatacampDbContext>
{
    public DatacampDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatacampDbContext>();
        optionsBuilder.UseSqlite("Data Source=datacamp.db");
        return new DatacampDbContext(optionsBuilder.Options);
    }
}
```

### Database Provider

**SQLite** is used for its simplicity and portability.

**Connection String:** `Data Source=datacamp.db`

**Database File Location:** Project root directory (where the application runs)

---

## DataCamp API Service

### Service Interface

```csharp
public interface IDataCampService
{
    Task<List<LeaderboardEntry>> GetAllLeaderboardEntriesAsync(
        string cookieValue,
        string group = "gaza-sky-geeks-25-26",
        string team = "nnu-team",
        int days = 30,
        string sortField = "xp",
        string sortOrder = "desc");

    Task<LeaderboardResponse?> GetLeaderboardPageAsync(
        string cookieValue,
        string group = "gaza-sky-geeks-25-26",
        string team = "nnu-team",
        int page = 1,
        int days = 30,
        string sortField = "xp",
        string sortOrder = "desc");
}
```

### Service Implementation

The `DataCampService` class handles all interactions with the DataCamp API.

**Constructor:**
```csharp
public DataCampService(HttpClient httpClient)
{
    _httpClient = httpClient;
    _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
}
```

**Features:**
- Dependency injection of `HttpClient`
- Automatic pagination handling
- Cookie-based authentication
- JSON deserialization with case-insensitive matching
- Error handling for HTTP and JSON parsing errors
- Console logging for progress tracking

### API Endpoint

**Base URL:** `https://learn-hub-api.datacamp.com/leaderboard`

**Query Parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `group` | string | "gaza-sky-geeks-25-26" | DataCamp group identifier |
| `team` | string | "nnu-team" | Team identifier within the group |
| `page` | int | 1 | Page number for pagination |
| `days` | int | 30 | Time range in days |
| `sortField` | string | "xp" | Field to sort by (xp, courses, chapters) |
| `sortOrder` | string | "desc" | Sort direction (asc, desc) |

**Authentication:**
- Cookie-based authentication
- Requires valid DataCamp session cookie
- Cookie passed via `Cookie` HTTP header

**Response Format:**
```json
{
  "entries": [
    {
      "chapters": 17,
      "courses": 6,
      "lastXp": "2025-10-30",
      "rank": 1,
      "user": {
        "avatarUrl": "https://...",
        "email": "student@example.com",
        "fullName": "Student Name",
        "id": 12345,
        "slug": "student-name"
      },
      "xp": 19475
    }
  ],
  "pagination": {
    "currentPage": 1,
    "hasNextPage": true,
    "hasPreviousPage": false,
    "pageSize": 50,
    "totalPages": 3
  }
}
```

---

## Data Transfer Objects (DTOs)

### LeaderboardResponse

Main response wrapper from the API.

```csharp
public class LeaderboardResponse
{
    public List<LeaderboardEntry> Entries { get; set; }
    public PaginationInfo Pagination { get; set; }
}
```

### LeaderboardEntry

Individual student entry in the leaderboard.

```csharp
public class LeaderboardEntry
{
    public int Chapters { get; set; }
    public int Courses { get; set; }
    public string LastXp { get; set; }
    public int Rank { get; set; }
    public UserInfo User { get; set; }
    public int Xp { get; set; }
}
```

### UserInfo

Student information within a leaderboard entry.

```csharp
public class UserInfo
{
    public string AvatarUrl { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public int Id { get; set; }
    public string Slug { get; set; }
}
```

### PaginationInfo

Metadata about pagination state.

```csharp
public class PaginationInfo
{
    public int CurrentPage { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
```

---

## Entity Framework Migrations

### Creating Migrations

After modifying any entity model:

```bash
cd DatacampAICoordinator.Infrastructure
dotnet ef migrations add MigrationName
```

### Applying Migrations

```bash
dotnet ef database update
```

### Rolling Back Migrations

```bash
dotnet ef database update PreviousMigrationName
```

### Removing Last Migration

```bash
dotnet ef migrations remove
```

### Viewing Migration SQL

```bash
dotnet ef migrations script
```

### Migration History

**InitialCreate (20251031161121)**
- Created all four tables (Student, StudentDailyStatus, StudentProgress, Process)
- Configured all relationships and constraints
- Added indexes for performance

---

## Usage Examples

### Using the DbContext

```csharp
var optionsBuilder = new DbContextOptionsBuilder<DatacampDbContext>();
optionsBuilder.UseSqlite("Data Source=datacamp.db");

using var context = new DatacampDbContext(optionsBuilder.Options);

// Query students
var students = await context.Student
    .Include(s => s.DailyStatuses)
    .ToListAsync();

// Add new student
var student = new Student
{
    FullName = "John Doe",
    Email = "john@example.com",
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow,
    IsActive = true
};

context.Student.Add(student);
await context.SaveChangesAsync();
```

### Using the DataCamp Service

```csharp
var httpClient = new HttpClient();
IDataCampService service = new DataCampService(httpClient);

string cookie = Environment.GetEnvironmentVariable("DATACAMP_COOKIE");

// Fetch all entries (handles pagination automatically)
var allEntries = await service.GetAllLeaderboardEntriesAsync(cookie);

// Fetch single page
var page = await service.GetLeaderboardPageAsync(
    cookie,
    page: 1,
    days: 60
);
```

---

## Configuration

### Database Connection String

Default: `Data Source=datacamp.db`

To change:
```csharp
optionsBuilder.UseSqlite("Data Source=/path/to/your/database.db");
```

### Service Configuration

Default API parameters can be overridden when calling service methods:

```csharp
var entries = await service.GetAllLeaderboardEntriesAsync(
    cookieValue: cookie,
    group: "custom-group",
    team: "custom-team",
    days: 90,
    sortField: "courses",
    sortOrder: "asc"
);
```

---

## Best Practices

### Entity Framework Core

1. **Always use migrations** - Don't modify database schema manually
2. **Include navigation properties** - For efficient querying and eager loading
3. **Use appropriate delete behaviors** - Cascade vs. Set Null based on data retention needs
4. **Add indexes strategically** - On foreign keys and frequently queried columns
5. **Use async methods** - For all I/O operations

### Service Layer

1. **Inject HttpClient** - Don't create new instances
2. **Handle errors gracefully** - Catch and log exceptions
3. **Use DTOs** - Don't expose domain entities directly
4. **Validate inputs** - Check for null/empty values
5. **Log operations** - Track API calls and errors

### Data Modeling

1. **Nullable reference types** - Use `?` for optional properties
2. **Default values** - Initialize collections and dates
3. **Calculated properties** - Don't store derived data
4. **Appropriate data types** - Use correct types (DateTime vs string)
5. **Documentation** - Add XML comments to entities

---

## Troubleshooting

### Migration Issues

**Problem:** "No DbContext was found"

**Solution:** Ensure you're in the Infrastructure project directory:
```bash
cd DatacampAICoordinator.Infrastructure
```

**Problem:** "A migration has already been applied"

**Solution:** Either update the migration or remove and recreate it:
```bash
dotnet ef migrations remove
dotnet ef migrations add NewMigrationName
```

### Database Issues

**Problem:** "Database is locked"

**Solution:** Close all connections to the database file:
- Close DB browser tools
- Stop the application
- Delete the `.db-wal` and `.db-shm` files if they exist

**Problem:** "Table already exists"

**Solution:** Delete the database file and recreate:
```bash
rm datacamp.db
dotnet ef database update
```

### Service Issues

**Problem:** "Cookie authentication failed"

**Solution:** 
- Verify cookie is current (not expired)
- Get fresh cookie from active browser session
- Ensure entire cookie string is copied

**Problem:** "JSON deserialization error"

**Solution:**
- Check API response format hasn't changed
- Update DTOs to match current API schema
- Enable detailed error logging

---

## Testing

### Unit Testing (Planned)

```csharp
// Example: Testing DataCampService
[Fact]
public async Task GetAllLeaderboardEntriesAsync_ShouldHandlePagination()
{
    // Arrange
    var mockHttpClient = new Mock<HttpClient>();
    var service = new DataCampService(mockHttpClient.Object);
    
    // Act
    var entries = await service.GetAllLeaderboardEntriesAsync("cookie");
    
    // Assert
    Assert.NotEmpty(entries);
}
```

### Integration Testing (Planned)

```csharp
// Example: Testing DbContext
[Fact]
public async Task Should_SaveAndRetrieveStudent()
{
    // Arrange
    var options = new DbContextOptionsBuilder<DatacampDbContext>()
        .UseInMemoryDatabase("TestDb")
        .Options;
    
    using var context = new DatacampDbContext(options);
    
    // Act
    var student = new Student { FullName = "Test", Email = "test@test.com" };
    context.Student.Add(student);
    await context.SaveChangesAsync();
    
    // Assert
    var saved = await context.Student.FirstOrDefaultAsync();
    Assert.NotNull(saved);
    Assert.Equal("Test", saved.FullName);
}
```

---

## Related Documentation

- [Console Application README](../DatacampAICoordinator/README.md)
- [Main Project README](../README.md)

---

For detailed API documentation and advanced usage, refer to the inline XML documentation in the source code.
# DataCamp AI Coordinator

A .NET 9.0 application that tracks and analyzes student progress on DataCamp by periodically scraping leaderboard data. The system stores historical snapshots and calculates progress metrics over time.

## Overview

DataCamp AI Coordinator automates the collection of student performance data from DataCamp's leaderboard API. It captures daily snapshots of student XP, courses completed, and chapters finished, enabling progress tracking and analytics over configurable time periods.

### Key Features

- **Automated Data Collection**: Fetches all leaderboard entries with automatic pagination
- **Historical Tracking**: Stores daily snapshots of student performance
- **Progress Analysis**: Calculates differences in XP, courses, and chapters over time
- **Flexible Scheduling**: Supports daily, monthly, or custom collection intervals
- **SQLite Database**: Lightweight, portable data storage
- **Cookie-based Authentication**: Secure API access using DataCamp session cookies

## Architecture

The project follows a layered architecture with clear separation of concerns:

```
├── DatacampAICoordinator/              # Console Application Layer
│   └── Handles application startup and orchestration
│
└── DatacampAICoordinator.Infrastructure/  # Infrastructure Layer
    ├── Data/                           # Database context and migrations
    ├── Models/                         # Domain entities
    ├── DTOs/                          # Data transfer objects
    └── Services/                      # External API integrations
```

### Technology Stack

- **.NET 9.0** - Latest .NET framework
- **Entity Framework Core 9.0.10** - ORM for data access
- **SQLite** - Embedded database
- **C# 12** - Modern language features (top-level statements, nullable reference types)
- **System.Text.Json** - JSON serialization/deserialization

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- DataCamp account with access to the leaderboard

### Installation

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd datacamp-ai-coordinator
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Apply database migrations:**
   ```bash
   cd DatacampAICoordinator.Infrastructure
   dotnet ef database update
   cd ..
   ```

### Configuration

The application requires a DataCamp session cookie for API authentication. Set this as an environment variable before running the application.

#### On Windows (PowerShell):
```powershell
$env:DATACAMP_COOKIE="your_session_cookie_here"
```

#### On Windows (Command Prompt):
```cmd
set DATACAMP_COOKIE=your_session_cookie_here
```

#### On Linux/macOS:
```bash
export DATACAMP_COOKIE="your_session_cookie_here"
```

**Getting your DataCamp cookie:**
1. Log into DataCamp in your browser
2. Open browser Developer Tools (F12)
3. Go to the Network tab
4. Navigate to the leaderboard page
5. Find a request to `learn-hub-api.datacamp.com`
6. Copy the entire `Cookie` header value

### Running the Application

```bash
cd DatacampAICoordinator
dotnet run
```

The application will:
1. Validate the environment variable is set
2. Connect to the DataCamp API
3. Fetch all leaderboard entries (handling pagination automatically)
4. Display progress and summary information

**Sample Output:**
```
DataCamp AI Coordinator
Fetching page 1...
Page 1: Fetched 50 entries. Total so far: 50
Fetching page 2...
Page 2: Fetched 50 entries. Total so far: 100
Fetching page 3...
Page 3: Fetched 28 entries. Total so far: 128
Finished fetching all pages. Total entries: 128

Total entries fetched: 128

First 5 entries:
Rank 1: John Doe - XP: 45000, Courses: 15, Chapters: 120
Rank 2: Jane Smith - XP: 42500, Courses: 14, Chapters: 115
...
```

## Project Structure

### Console Application Layer
[See DatacampAICoordinator/README.md](./DatacampAICoordinator/README.md)

The entry point of the application. Handles:
- Environment variable validation
- Service instantiation
- Data collection orchestration
- Output formatting

### Infrastructure Layer
[See DatacampAICoordinator.Infrastructure/README.md](./DatacampAICoordinator.Infrastructure/README.md)

Contains all infrastructure concerns:
- **Data Access**: Entity Framework Core DbContext and migrations
- **Domain Models**: Student, StudentDailyStatus, StudentProgress, Process
- **External APIs**: DataCamp service implementation
- **DTOs**: API response models

## Database Schema

The application uses four main entities:

- **Student**: Core student information (name, email, DataCamp username)
- **StudentDailyStatus**: Daily snapshots of student metrics (XP, courses, chapters)
- **StudentProgress**: Calculated differences between time periods
- **Process**: Metadata about data collection runs

### Entity Relationships

```
Student (1) ─────┬─── (*) StudentDailyStatus
                 │
                 └─── (*) StudentProgress

Process (1) ─────┬─── (*) StudentDailyStatus
                 │
                 └─── (*) StudentProgress
```

For detailed schema information, see the [Infrastructure Layer README](./DatacampAICoordinator.Infrastructure/README.md#database-schema).

## API Integration

The application integrates with DataCamp's leaderboard API:

**Endpoint:** `https://learn-hub-api.datacamp.com/leaderboard`

**Query Parameters:**
- `group` - The DataCamp group identifier (default: "gaza-sky-geeks-25-26")
- `team` - The team identifier (default: "nnu-team")
- `page` - Page number for pagination
- `days` - Time range in days (default: 30)
- `sortField` - Field to sort by (default: "xp")
- `sortOrder` - Sort direction (default: "desc")

**Authentication:** Cookie-based (session cookie required)

**Response Format:** JSON with entries array and pagination metadata

For detailed API documentation, see the [Infrastructure Layer README](./DatacampAICoordinator.Infrastructure/README.md#datacamp-api-service).

## Configuration Options

Currently, the application uses default values for DataCamp group and team:
- **Group**: `gaza-sky-geeks-25-26`
- **Team**: `nnu-team`
- **Days**: `30`
- **Sort Field**: `xp`
- **Sort Order**: `desc`

To customize these values, modify the service call in `Program.cs`:

```csharp
var allEntries = await dataCampService.GetAllLeaderboardEntriesAsync(
    cookieValue,
    group: "your-group",
    team: "your-team",
    days: 60
);
```

## Development

### Building the Project

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```
*(Note: Tests not yet implemented)*

### Creating New Migrations

After modifying entity models:

```bash
cd DatacampAICoordinator.Infrastructure
dotnet ef migrations add MigrationName
dotnet ef database update
```

## Troubleshooting

### "DATACAMP_COOKIE environment variable is not set"

**Solution:** Set the environment variable before running the application (see Configuration section).

### "Request error: Unauthorized" or 401 status

**Cause:** Cookie has expired or is invalid.

**Solution:** 
1. Log out and log back into DataCamp
2. Get a fresh cookie from your browser
3. Update the environment variable

### Database locked errors

**Cause:** SQLite database file is being accessed by another process.

**Solution:** 
1. Close any database browsers or tools accessing the database
2. Ensure no other instances of the application are running

### Migration errors

**Solution:** Delete the database file and rerun migrations:
```bash
rm datacamp.db
cd DatacampAICoordinator.Infrastructure
dotnet ef database update
```

## Roadmap

### Planned Features

- [ ] Dependency injection container setup
- [ ] Structured logging (Serilog or NLog)
- [ ] Configuration system (appsettings.json)
- [ ] Repository pattern implementation
- [ ] Scheduled background jobs (Quartz.NET)
- [ ] Progress calculation automation
- [ ] Report generation and exports
- [ ] Retry policies and circuit breakers
- [ ] Unit and integration tests
- [ ] Web API layer for remote access

### Future Enhancements

- Multiple group/team support
- Email notifications for progress milestones
- Dashboard UI (Blazor or React)
- Export to CSV/Excel
- Data visualization and charts
- Comparative analytics between students
- OAuth authentication with DataCamp

## Contributing

(To be defined)

## License

(To be defined)

## Support

For issues, questions, or contributions, please [open an issue](link-to-issues) or contact the development team.

---

**Note:** This application is for educational and personal use. Ensure compliance with DataCamp's Terms of Service when using this tool.

