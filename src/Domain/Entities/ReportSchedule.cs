using System.ComponentModel.DataAnnotations;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities;

/// <summary>
/// Represents a scheduled report configuration
/// </summary>
public class ReportSchedule : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public ReportType ReportType { get; set; }

    public ReportFormat Format { get; set; }

    public ScheduleFrequency Frequency { get; set; }

    /// <summary>
    /// Cron expression for custom scheduling
    /// </summary>
    [MaxLength(100)]
    public string? CronExpression { get; set; }

    /// <summary>
    /// Next scheduled run time
    /// </summary>
    public DateTime? NextRunAt { get; set; }

    /// <summary>
    /// Last run time
    /// </summary>
    public DateTime? LastRunAt { get; set; }

    /// <summary>
    /// Whether the schedule is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Report parameters as JSON
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    /// Report filters as JSON
    /// </summary>
    public string? Filters { get; set; }

    /// <summary>
    /// Email recipients as JSON array
    /// </summary>
    public string? EmailRecipients { get; set; }

    /// <summary>
    /// SuperAdmin who created the schedule
    /// </summary>
    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Optional tenant ID for tenant-specific scheduled reports
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Time zone for scheduling
    /// </summary>
    [MaxLength(50)]
    public string TimeZone { get; set; } = "UTC";

    /// <summary>
    /// Reports generated from this schedule
    /// </summary>
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    /// <summary>
    /// Subscriptions to this schedule
    /// </summary>
    public virtual ICollection<ReportSubscription> Subscriptions { get; set; } = new List<ReportSubscription>();

    /// <summary>
    /// Execution history for this schedule
    /// </summary>
    public virtual ICollection<ReportExecution> Executions { get; set; } = new List<ReportExecution>();
}

