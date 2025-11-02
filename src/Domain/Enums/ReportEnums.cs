namespace HotelManagement.Domain.Enums;

/// <summary>
/// Types of reports available in the system
/// </summary>
public enum ReportType
{
    SystemOverview = 0,
    Financial = 1,
    TenantUsage = 2,
    Audit = 3,
    Analytics = 4,
    Custom = 5
}

/// <summary>
/// Export formats supported for reports
/// </summary>
public enum ReportFormat
{
    PDF = 0,
    Excel = 1,
    CSV = 2,
    JSON = 3,
    HTML = 4
}

/// <summary>
/// Status of report generation
/// </summary>
public enum ReportStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Expired = 4
}

/// <summary>
/// Frequency for scheduled reports
/// </summary>
public enum ScheduleFrequency
{
    Daily = 0,
    Weekly = 1,
    Monthly = 2,
    Quarterly = 3,
    Yearly = 4,
    Custom = 5
}

/// <summary>
/// Report category for organization
/// </summary>
public enum ReportCategory
{
    Operations = 0,
    Financial = 1,
    Compliance = 2,
    Performance = 3,
    Security = 4,
    Custom = 5
}

