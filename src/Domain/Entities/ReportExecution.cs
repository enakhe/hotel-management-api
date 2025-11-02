using System.ComponentModel.DataAnnotations;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities;

/// <summary>
/// Represents a report generation execution log
/// </summary>
public class ReportExecution : BaseEntity
{
    /// <summary>
    /// Associated report schedule
    /// </summary>
    public Guid? ReportScheduleId { get; set; }

    public virtual ReportSchedule? ReportSchedule { get; set; }

    /// <summary>
    /// Generated report
    /// </summary>
    public Guid? ReportId { get; set; }

    public virtual Report? Report { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Duration in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Number of records in the report
    /// </summary>
    public int? RecordCount { get; set; }

    /// <summary>
    /// Error message if execution failed
    /// </summary>
    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Stack trace if execution failed
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// SuperAdmin who initiated the execution (for on-demand reports)
    /// </summary>
    public Guid? InitiatedBy { get; set; }

    /// <summary>
    /// Whether this was a scheduled or on-demand execution
    /// </summary>
    public bool IsScheduled { get; set; } = false;
}

