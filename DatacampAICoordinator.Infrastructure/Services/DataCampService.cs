using System.Text.Json;
using DatacampAICoordinator.Infrastructure.Configuration;
using DatacampAICoordinator.Infrastructure.DTOs;
using DatacampAICoordinator.Infrastructure.Services.Interfaces;

namespace DatacampAICoordinator.Infrastructure.Services;

/// <summary>
/// Service for interacting with the DataCamp API
/// </summary>
public class DataCampService : IDataCampService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly DataCampSettings _dataCampSettings;

    public DataCampService(HttpClient httpClient, DataCampSettings dataCampSettings)
    {
        _httpClient = httpClient;
        _dataCampSettings = dataCampSettings ?? throw new ArgumentNullException(nameof(dataCampSettings));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<LeaderboardEntry>> GetAllLeaderboardEntriesAsync(
        string cookieValue,
        string? group = null,
        string? team = null,
        int? days = null,
        string? sortField = null,
        string? sortOrder = null)
    {
        // Use provided values or fall back to settings
        group ??= _dataCampSettings.GroupName;
        team ??= _dataCampSettings.TeamName;
        days ??= _dataCampSettings.Days;
        sortField ??= _dataCampSettings.SortField;
        sortOrder ??= _dataCampSettings.SortOrder;

        var allEntries = new List<LeaderboardEntry>();
        int currentPage = 1;
        bool hasMorePages = true;

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Cookie", cookieValue);

        while (hasMorePages)
        {
            try
            {
                string url = $"https://learn-hub-api.datacamp.com/leaderboard?group={group}&team={team}&page={currentPage}&days={days}&sortField={sortField}&sortOrder={sortOrder}";

                Console.WriteLine($"Fetching page {currentPage}...");

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var leaderboardResponse = JsonSerializer.Deserialize<LeaderboardResponse>(responseBody, _jsonOptions);

                if (leaderboardResponse != null && leaderboardResponse.Entries.Any())
                {
                    allEntries.AddRange(leaderboardResponse.Entries);
                    Console.WriteLine($"Page {currentPage}: Fetched {leaderboardResponse.Entries.Count} entries. Total so far: {allEntries.Count}");

                    hasMorePages = leaderboardResponse.Pagination.HasNextPage;
                    currentPage++;
                }
                else
                {
                    hasMorePages = false;
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error on page {currentPage}: {e.Message}");
                throw;
            }
            catch (JsonException e)
            {
                Console.WriteLine($"JSON parsing error on page {currentPage}: {e.Message}");
                throw;
            }
        }

        Console.WriteLine($"Finished fetching all pages. Total entries: {allEntries.Count}");
        return allEntries;
    }
}
