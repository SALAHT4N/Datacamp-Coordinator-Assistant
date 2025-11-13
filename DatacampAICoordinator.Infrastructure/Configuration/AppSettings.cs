namespace DatacampAICoordinator.Infrastructure.Configuration;

/// <summary>
/// Root configuration class that holds all application settings
/// </summary>
public class AppSettings
{
    public GeneralSettings GeneralSettings { get; set; } = new();
    public EmailSettings EmailSettings { get; set; } = new();
    public DataCampSettings DataCampSettings { get; set; } = new();
}

/// <summary>
/// General application settings
/// </summary>
public class GeneralSettings
{
    /// <summary>
    /// DataCamp session cookie for authentication
    /// </summary>
    public string DataCampCookie { get; set; } = string.Empty;
    
    /// <summary>
    /// Path to the SQLite database file (relative to executable or absolute path)
    /// </summary>
    public string DatabasePath { get; set; } = "datacamp.db";
    
    /// <summary>
    /// Number of days threshold to mark a student as inactive
    /// </summary>
    public int InactiveDaysThreshold { get; set; } = 3;
    
    /// <summary>
    /// Name of the CSV file used to filter students
    /// </summary>
    public string StudentFilterCsvFileName { get; set; } = "students_filter.csv";
}

/// <summary>
/// Email configuration settings for sending reports
/// </summary>
public class EmailSettings
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "DataCamp AI Coordinator";
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = "Student Progress Report";
}

/// <summary>
/// DataCamp API settings
/// </summary>
public class DataCampSettings
{
    public string GroupName { get; set; } = "gaza-sky-geeks-25-26";
    public string TeamName { get; set; } = "nnu-team";
    public int Days { get; set; } = 30;
    public string SortField { get; set; } = "xp";
    public string SortOrder { get; set; } = "desc";
}


