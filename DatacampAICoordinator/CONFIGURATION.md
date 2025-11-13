# Configuration Guide

## appsettings.json

This application uses `appsettings.json` for configuration. The file will be automatically copied to the build output directory.

### Configuration Sections

#### AppSettings (General Settings)
- **DataCampCookie**: Your DataCamp session cookie (Required)
- **DatabasePath**: Path to SQLite database file (relative to exe or absolute path)
  - Default: `datacamp.db`
  - Will be created in the same directory as the executable
- **InactiveDaysThreshold**: Number of days to mark a student as inactive
  - Default: `3`
- **StudentFilterCsvFileName**: Name of the CSV file for filtering students
  - Default: `students_filter.csv`

#### EmailSettings
- **SmtpHost**: SMTP server hostname (e.g., `smtp.gmail.com`)
- **SmtpPort**: SMTP server port (typically `587` for TLS)
- **SmtpUsername**: Your email address
- **SmtpPassword**: Your email app password (for Gmail, use App Password, not your regular password)
- **FromEmail**: Email address to send from
- **FromName**: Display name for the sender
- **ToEmail**: Email address to send reports to
- **Subject**: Subject line for email reports

#### DataCampSettings
- **GroupName**: DataCamp group identifier
- **TeamName**: DataCamp team identifier
- **Days**: Number of days to fetch data for
- **SortField**: Field to sort leaderboard by (e.g., `xp`)
- **SortOrder**: Sort order (`asc` or `desc`)

### Setup Instructions

1. Copy `appsettings.template.json` to `appsettings.json`
2. Fill in your credentials:
   - Add your DataCamp session cookie
   - Add your email credentials (use App Password for Gmail)
   - Configure recipient email address
3. Run the application

### Security Note

⚠️ **IMPORTANT**: Never commit `appsettings.json` with real credentials to version control!
- Add `appsettings.json` to `.gitignore`
- Use `appsettings.template.json` as a template for others

### Database

The database will be automatically:
- Created if it doesn't exist
- Migrated to the latest schema version
- Placed in the same directory as the executable (or custom path if specified)

No manual database setup is required!


