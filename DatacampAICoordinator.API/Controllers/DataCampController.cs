using DatacampAICoordinator.Infrastructure.Configuration;
using DatacampAICoordinator.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatacampAICoordinator.API.Controllers;

/// <summary>
/// Controller for DataCamp synchronization and reporting operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DataCampController : ControllerBase
{
    private readonly IDataCampCoordinatorService _coordinatorService;
    private readonly GeneralSettings _generalSettings;
    private readonly ILogger<DataCampController> _logger;

    public DataCampController(
        IDataCampCoordinatorService coordinatorService,
        IOptions<GeneralSettings> generalSettings,
        ILogger<DataCampController> logger)
    {
        _coordinatorService = coordinatorService;
        _generalSettings = generalSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Execute full synchronization workflow: fetch leaderboard data, sync students, create status records, and calculate progress
    /// </summary>
    /// <returns>Number of progress records created</returns>
    [HttpPost("sync")]
    public async Task<IActionResult> ExecuteFullSync()
    {
        try
        {
            _logger.LogInformation("Starting full data synchronization");

            if (string.IsNullOrEmpty(_generalSettings.DataCampCookie))
            {
                _logger.LogError("DataCampCookie is not configured");
                return BadRequest(new { error = "DataCampCookie is not set in configuration" });
            }

            var progressCount = await _coordinatorService.ExecuteFullSyncAsync(_generalSettings.DataCampCookie);

            _logger.LogInformation("Sync completed successfully. Progress records created: {ProgressCount}", progressCount);

            return Ok(new
            {
                success = true,
                message = "Synchronization completed successfully",
                progressRecordsCreated = progressCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during full synchronization");
            return StatusCode(500, new
            {
                success = false,
                error = "An error occurred during synchronization",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Generate a date range report comparing data between two processes
    /// </summary>
    /// <param name="startDate">Start date (format: yyyy-MM-dd)</param>
    /// <param name="endDate">End date (format: yyyy-MM-dd)</param>
    /// <returns>Success status of report generation</returns>
    [HttpPost("report/date-range")]
    public async Task<IActionResult> GenerateDateRangeReport([FromQuery] string startDate, [FromQuery] string endDate)
    {
        try
        {
            if (!DateTime.TryParse(startDate, out var parsedStartDate))
            {
                return BadRequest(new { error = $"Invalid start date format '{startDate}'. Expected format: yyyy-MM-dd" });
            }

            if (!DateTime.TryParse(endDate, out var parsedEndDate))
            {
                return BadRequest(new { error = $"Invalid end date format '{endDate}'. Expected format: yyyy-MM-dd" });
            }

            if (parsedStartDate > parsedEndDate)
            {
                return BadRequest(new { error = "Start date must be before or equal to end date" });
            }

            _logger.LogInformation("Generating date range report from {StartDate} to {EndDate}", parsedStartDate, parsedEndDate);

            var success = await _coordinatorService.GenerateDateRangeReportAsync(parsedStartDate, parsedEndDate);

            if (success)
            {
                _logger.LogInformation("Date range report generated successfully");
                return Ok(new
                {
                    success = true,
                    message = "Date range report generated successfully",
                    startDate = parsedStartDate.ToString("yyyy-MM-dd"),
                    endDate = parsedEndDate.ToString("yyyy-MM-dd")
                });
            }
            else
            {
                _logger.LogWarning("Date range report generation failed - insufficient data");
                return BadRequest(new
                {
                    success = false,
                    error = "Insufficient data to generate report for the specified date range"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating date range report");
            return StatusCode(500, new
            {
                success = false,
                error = "An error occurred while generating the report",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Get API health status
    /// </summary>
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        var hasValidCookie = !string.IsNullOrEmpty(_generalSettings.DataCampCookie);
        
        return Ok(new
        {
            status = "healthy",
            cookieConfigured = hasValidCookie,
            databasePath = _generalSettings.DatabasePath
        });
    }
}

