using DatacampAICoordinator.Infrastructure.Services;
using DatacampAICoordinator.Infrastructure.Services.Interfaces;

Console.WriteLine("DataCamp AI Coordinator");

string? cookieValue = Environment.GetEnvironmentVariable("DATACAMP_COOKIE");

if (string.IsNullOrEmpty(cookieValue))
{
    Console.WriteLine("Error: DATACAMP_COOKIE environment variable is not set.");
    Console.WriteLine("Please set the DATACAMP_COOKIE environment variable with your DataCamp session cookie.");
    Environment.Exit(1);
}

IDataCampService dataCampService = new DataCampService(new HttpClient());
var allEntries = await dataCampService.GetAllLeaderboardEntriesAsync(cookieValue);

Console.WriteLine($"\nTotal entries fetched: {allEntries.Count}");
Console.WriteLine("\nFirst 5 entries:");
foreach (var entry in allEntries.Take(5))
{
    Console.WriteLine($"Rank {entry.Rank}: {entry.User.FullName} - XP: {entry.Xp}, Courses: {entry.Courses}, Chapters: {entry.Chapters}");
}