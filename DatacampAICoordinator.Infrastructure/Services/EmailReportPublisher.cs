using System.Net;
using System.Net.Mail;
using DatacampAICoordinator.Infrastructure.Services.Interfaces;

namespace DatacampAICoordinator.Infrastructure.Services;

/// <summary>
/// Email implementation of the report publisher
/// </summary>
public class EmailReportPublisher : IReportPublisher
{
    // Email configuration - can be moved to appsettings.json later
    private const string SmtpHost = "smtp.gmail.com"; // Change to your SMTP server
    private const int SmtpPort = 587;
    private const string SmtpUsername = "tanboursalah@gmail.com"; // Replace with your email
    private const string SmtpPassword = "your-password"; // Replace with your password or app-specific password
    private const string FromEmail = "tanboursalah@gmail.com"; // Replace with sender email
    private const string FromName = "DataCamp AI Coordinator";
    private const string ToEmail = "salah.aldin.tanbour@outlook.com"; // Replace with recipient email
    private const string Subject = "Student Progress Report";

    /// <summary>
    /// Publishes the report by sending it via email
    /// </summary>
    /// <param name="htmlContent">The HTML content to send</param>
    /// <param name="processId">The process ID for the report</param>
    public async Task PublishAsync(string htmlContent, int processId)
    {
        try
        {
            using var smtpClient = new SmtpClient(SmtpHost, SmtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(SmtpUsername, SmtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(FromEmail, FromName),
                Subject = $"{Subject} - Process {processId} - {DateTime.UtcNow:yyyy-MM-dd}",
                Body = htmlContent,
                IsBodyHtml = true
            };

            mailMessage.To.Add(ToEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email for process {processId}: {ex.Message}");
            throw;
        }
    }
}

