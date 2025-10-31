# DataCamp AI Coordinator - Console Application

The console application layer serves as the entry point for the DataCamp AI Coordinator. It orchestrates the data collection process and provides a command-line interface for running the application.

## Overview

This is a .NET 9.0 console application using modern C# features including top-level statements, which eliminates boilerplate code and provides a clean, streamlined entry point.

## Features

- **Environment-based Configuration**: Reads sensitive data from environment variables
- **Input Validation**: Validates required configuration before execution
- **Service Integration**: Connects with the Infrastructure layer services
- **Progress Reporting**: Displays real-time feedback during data collection
- **Summary Output**: Shows collection results and sample data

## Project Structure

```
DatacampAICoordinator/
├── Program.cs                          # Application entry point
├── DatacampAICoordinator.csproj       # Project configuration
└── README.md                          # This file
```

## Dependencies

### Project References

- `DatacampAICoordinator.Infrastructure` - Infrastructure layer containing data access and services

### Framework

- .NET 9.0
- Implicit usings enabled (System, System.Linq, etc. automatically available)
- Nullable reference types enabled

## Configuration

### Environment Variables

| Variable          | Required | Description                                    | Example                          |
| ----------------- | -------- | ---------------------------------------------- | -------------------------------- |
| `DATACAMP_COOKIE` | Yes      | DataCamp session cookie for API authentication | `"dc_consent={...}; _dct={...}"` |

### Setting Environment Variables

**PowerShell:**

```powershell
$env:DATACAMP_COOKIE="your_cookie_here"
```

**Command Prompt:**

```cmd
set DATACAMP_COOKIE=your_cookie_here
```

**Linux/macOS:**

```bash
export DATACAMP_COOKIE="your_cookie_here"
```

**Permanent (Windows):**

```powershell
[Environment]::SetEnvironmentVariable("DATACAMP_COOKIE", "your_cookie_here", "User")
```

## Usage

### Basic Run

```bash
cd DatacampAICoordinator
dotnet run
```

### Build and Run

```bash
dotnet build
dotnet run --no-build
```

### Run in Release Mode

```bash
dotnet run --configuration Release
```

## Application Flow

1. **Startup**

   - Application displays title banner
   - Reads `DATACAMP_COOKIE` environment variable

2. **Validation**

   - Checks if cookie is provided
   - Exits with error code 1 if missing

3. **Service Setup**

   - Instantiates `HttpClient`
   - Creates `DataCampService` instance

4. **Data Collection**

   - Calls `GetAllLeaderboardEntriesAsync()`
   - Service automatically handles pagination
   - Progress displayed for each page

5. **Results Display**
   - Shows total entries fetched
   - Displays details of first 5 entries (rank, name, XP, courses, chapters)

## Code Structure

### Program.cs

```csharp
using DatacampAICoordinator.Infrastructure.Services;
using DatacampAICoordinator.Infrastructure.Services.Interfaces;

// Application startup
Console.WriteLine("DataCamp AI Coordinator");

// Configuration validation
string? cookieValue = Environment.GetEnvironmentVariable("DATACAMP_COOKIE");
if (string.IsNullOrEmpty(cookieValue))
{
    // Error handling and exit
}

// Service instantiation
IDataCampService dataCampService = new DataCampService(new HttpClient());

// Data collection
var allEntries = await dataCampService.GetAllLeaderboardEntriesAsync(cookieValue);

// Results output
// ...
```

### Design Patterns

- **Dependency on Interfaces**: Uses `IDataCampService` interface for loose coupling
- **Top-Level Statements**: Modern C# 9+ feature for concise console apps
- **Async/Await**: Asynchronous operations for I/O-bound work

## Output Format

### Successful Execution

```
DataCamp AI Coordinator
Fetching page 1...
Page 1: Fetched 50 entries. Total so far: 50
Fetching page 2...
Page 2: Fetched 50 entries. Total so far: 100
Finished fetching all pages. Total entries: 100

Total entries fetched: 100

First 5 entries:
Rank 1: John Doe - XP: 45000, Courses: 15, Chapters: 120
Rank 2: Jane Smith - XP: 42500, Courses: 14, Chapters: 115
Rank 3: Bob Wilson - XP: 40000, Courses: 13, Chapters: 110
Rank 4: Alice Brown - XP: 38500, Courses: 12, Chapters: 105
Rank 5: Charlie Davis - XP: 36000, Courses: 12, Chapters: 100
```

### Error: Missing Environment Variable

```
DataCamp AI Coordinator
Error: DATACAMP_COOKIE environment variable is not set.
Please set the DATACAMP_COOKIE environment variable with your DataCamp session cookie.
```

**Exit Code:** 1

### Error: HTTP Request Failure

```
DataCamp AI Coordinator
Fetching page 1...
Request error on page 1: Unauthorized
```

**Exit Code:** Exception thrown (non-zero)

## Error Handling

### Environment Variable Validation

- Checks if `DATACAMP_COOKIE` is set
- Provides clear error message if missing
- Exits gracefully with code 1

### Service Errors

- HTTP exceptions propagate from service layer
- Displays error message with context (which page failed)
- Application terminates with exception

## Customization

### Changing DataCamp Group/Team

Modify the service call parameters:

```csharp
var allEntries = await dataCampService.GetAllLeaderboardEntriesAsync(
    cookieValue,
    group: "your-group-name",
    team: "your-team-name",
    days: 60,
    sortField: "xp",
    sortOrder: "desc"
);
```

### Changing Output Format

Modify the display logic:

```csharp
// Display all entries instead of first 5
foreach (var entry in allEntries)
{
    Console.WriteLine($"Rank {entry.Rank}: {entry.User.FullName} - XP: {entry.Xp}");
}

// Export to JSON
var json = JsonSerializer.Serialize(allEntries, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText("leaderboard.json", json);
```

## Building for Distribution

### Single-File Executable

```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

### Framework-Dependent

```bash
dotnet publish -c Release -r win-x64
```

Output location: `bin/Release/net9.0/win-x64/publish/`

## Troubleshooting

### Issue: "Type or namespace name 'Infrastructure' could not be found"

**Cause:** Project reference is missing or not restored.

**Solution:**

```bash
dotnet restore
dotnet build
```

### Issue: Application exits immediately

**Cause:** Environment variable not set in current session.

**Solution:**

- Verify environment variable: `echo $env:DATACAMP_COOKIE` (PowerShell)
- Set it in the current session before running

### Issue: "Cannot access a disposed object"

**Cause:** HttpClient is being disposed incorrectly.

**Solution:** This should not occur in the current implementation, but if it does, ensure HttpClient is created per request or use IHttpClientFactory.

## Best Practices

1. **Never commit sensitive data**: Keep cookies out of source control
2. **Use environment variables**: For all configuration and secrets
3. **Handle errors gracefully**: Provide clear error messages to users
4. **Keep Program.cs thin**: Move business logic to services
5. **Use dependency injection**: For better testability and maintainability

## Related Documentation

- [Infrastructure Layer README](../DatacampAICoordinator.Infrastructure/README.md)
- [Main Project README](../README.md)

---

For questions or issues specific to the console application, please refer to the main project documentation or create an issue.
