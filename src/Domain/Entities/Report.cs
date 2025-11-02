using System.ComponentModel.DataAnnotations;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities;

/// <summary>
/// Represents a generated report in the system
/// </summary>
public class Report : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public ReportType Type { get; set; }

    public ReportCategory Category { get; set; }

    public ReportFormat Format { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    /// <summary>
    /// SuperAdmin who generated the report
    /// </summary>
    public Guid GeneratedBy { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// File path where the report is stored
    /// </summary>
    [MaxLength(500)]
    public string? FilePath { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// Download URL for the report
    /// </summary>
    [MaxLength(1000)]
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// When the download URL expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Report parameters as JSON
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    /// Report filters as JSON
    /// </summary>
    public string? Filters { get; set; }

    /// <summary>
    /// Optional tenant ID for tenant-specific reports
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Number of records in the report
    /// </summary>
    public int? RecordCount { get; set; }

    /// <summary>
    /// Error message if report generation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Duration of report generation in milliseconds
    /// </summary>
    public long? GenerationDurationMs { get; set; }

    /// <summary>
    /// Associated report schedule if generated from a schedule
    /// </summary>
    public Guid? ReportScheduleId { get; set; }

    public virtual ReportSchedule? ReportSchedule { get; set; }

    /// <summary>
    /// Report executions related to this report
    /// </summary>
    public virtual ICollection<ReportExecution> Executions { get; set; } = new List<ReportExecution>();
}

