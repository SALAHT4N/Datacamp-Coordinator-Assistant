using System.Net;
using System.Net.Mail;
using DatacampAICoordinator.Infrastructure.Configuration;
using DatacampAICoordinator.Infrastructure.Services.Interfaces;

namespace DatacampAICoordinator.Infrastructure.Services;

/// <summary>
/// Email implementation of the report publisher
/// </summary>
public class EmailReportPublisher : IReportPublisher
{
    private readonly EmailSettings _emailSettings;

    public EmailReportPublisher(EmailSettings emailSettings)
    {
        _emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
    }

    /// <summary>
    /// Publishes the report by sending it via email
    /// </summary>
    /// <param name="htmlContent">The HTML content to send</param>
    /// <param name="processId">The process ID for the report</param>
    public async Task PublishAsync(string htmlContent, int processId)
    {
        try
        {
            using var smtpClient = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = $"{_emailSettings.Subject} - Process {processId} - {DateTime.UtcNow:yyyy-MM-dd}",
                Body = htmlContent,
                IsBodyHtml = true
            };

            mailMessage.To.Add(_emailSettings.ToEmail);

            await smtpClient.SendMailAsync(mailMessage);

            Console.WriteLine($"Email sent successfully for process {processId} to {_emailSettings.ToEmail}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email for process {processId}: {ex.Message}");
            throw;
        }
    }
}
