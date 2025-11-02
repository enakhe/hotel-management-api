using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Common.Interfaces;

/// <summary>
/// Service for managing scheduled reports
/// </summary>
public interface IReportSchedulerService
{
    /// <summary>
    /// Create a new report schedule
    /// </summary>
    Task<Result<ReportScheduleDto>> CreateScheduleAsync(CreateReportScheduleDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing schedule
    /// </summary>
    Task<Result<ReportScheduleDto>> UpdateScheduleAsync(Guid scheduleId, UpdateReportScheduleDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a schedule
    /// </summary>
    Task<Result<bool>> DeleteScheduleAsync(Guid scheduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get schedule by ID
    /// </summary>
    Task<Result<ReportScheduleDto>> GetScheduleByIdAsync(Guid scheduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all schedules with filtering
    /// </summary>
    Task<Result<PaginatedResult<ReportScheduleDto>>> GetSchedulesAsync(ScheduleFilterDto filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pause a schedule
    /// </summary>
    Task<Result<bool>> PauseScheduleAsync(Guid scheduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resume a schedule
    /// </summary>
    Task<Result<bool>> ResumeScheduleAsync(Guid scheduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a schedule immediately
    /// </summary>
    Task<Result<ReportResponseDto>> ExecuteScheduleNowAsync(Guid scheduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate next run time for a schedule
    /// </summary>
    DateTime? CalculateNextRunTime(ReportScheduleDto schedule);
}

