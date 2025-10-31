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
Rank 3: Bob Wilson - XP: 40000, Courses: 13, Chapters: 110
Rank 4: Alice Brown - XP: 38500, Courses: 12, Chapters: 105
Rank 5: Charlie Davis - XP: 36000, Courses: 12, Chapters: 100
```

## Project Structure

### Console Application Layer
[See DatacampAICoordinator/README.md](./README.md)

The entry point of the application. Handles:
- Environment variable validation
- Service instantiation
- Data collection orchestration
- Output formatting

### Infrastructure Layer
[See DatacampAICoordinator.Infrastructure/README.md](../DatacampAICoordinator.Infrastructure/README.md)

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

For detailed schema information, see the [Infrastructure Layer README](../DatacampAICoordinator.Infrastructure/README.md#database-schema).

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

For detailed API documentation, see the [Infrastructure Layer README](../DatacampAICoordinator.Infrastructure/README.md#datacamp-api-service).

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

## Additional Documentation

- [Console Application Layer Documentation](./README.md)
- [Infrastructure Layer Documentation](../DatacampAICoordinator.Infrastructure/README.md)
- [Development Log (DEVLOG)](../README.md)

---

**Project Status:** Active Development  
**Last Updated:** October 31, 2025  
**Version:** 0.1.0-alpha

