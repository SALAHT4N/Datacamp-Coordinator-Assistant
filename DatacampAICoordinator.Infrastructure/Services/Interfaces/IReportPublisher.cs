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
    Task PublishAsync(string htmlContent);
}
