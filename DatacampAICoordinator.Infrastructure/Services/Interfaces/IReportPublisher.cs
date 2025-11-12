namespace DatacampAICoordinator.Infrastructure.Services.Interfaces;

/// <summary>
/// Interface for publishing reports to various destinations
/// </summary>
public interface IReportPublisher
{
    /// <summary>
    /// Publishes the HTML report content
    /// </summary>
    /// <param name="htmlContent">The HTML content to publish</param>
    /// <param name="processId">The process ID associated with the report</param>
    Task PublishAsync(string htmlContent, int processId);
}

