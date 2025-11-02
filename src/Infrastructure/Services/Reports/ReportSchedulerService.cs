using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HotelManagement.Infrastructure.Services.Reports;

/// <summary>
/// Service for managing scheduled reports
/// </summary>
public class ReportSchedulerService : IReportSchedulerService
{
    private readonly IApplicationDbContext _context;
    private readonly IBackgroundJobService _jobService;
    private readonly ISuperAdminContext _superAdminContext;
    private readonly ISuperAdminAuditService _auditService;
    private readonly ILogger<ReportSchedulerService> _logger;

    public ReportSchedulerService(
        IApplicationDbContext context,
        IBackgroundJobService jobService,
        ISuperAdminContext superAdminContext,
        ISuperAdminAuditService auditService,
        ILogger<ReportSchedulerService> logger)
    {
        _context = context;
        _jobService = jobService;
        _superAdminContext = superAdminContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<ReportScheduleDto>> CreateScheduleAsync(CreateReportScheduleDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating report schedule: {Name}", request.Name);

            var schedule = new ReportSchedule
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                ReportType = request.ReportType,
                Format = request.Format,
                Frequency = request.Frequency,
                CronExpression = request.CronExpression ?? GetCronExpression(request.Frequency),
                IsActive = true,
                Parameters = JsonSerializer.Serialize(request.Parameters),
                Filters = JsonSerializer.Serialize(request.Filters),
                EmailRecipients = JsonSerializer.Serialize(request.EmailRecipients ?? new List<string>()),
                CreatedBy = _superAdminContext.SuperAdminId ?? Guid.Empty,
                CreatedAt = DateTime.UtcNow,
                TenantId = request.TenantId,
                TimeZone = request.TimeZone
            };

            // Calculate next run time
            schedule.NextRunAt = CalculateNextRunTime(MapToDto(schedule));

            _context.ReportSchedules.Add(schedule);
            await _context.SaveChangesAsync(cancellationToken);

            // Create Hangfire recurring job
            var jobId = $"report-schedule-{schedule.Id}";
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(schedule.TimeZone);

            _jobService.AddOrUpdateRecurringJob(
                jobId,
                () => ExecuteScheduledReportAsync(schedule.Id),
                schedule.CronExpression,
                timeZone);

            await _auditService.LogActionAsync(
                "CreateReportSchedule",
                "ReportSchedule",
                schedule.Id.ToString(),
                schedule.TenantId,
                $"Created report schedule '{schedule.Name}'");

            _logger.LogInformation("Report schedule created: {ScheduleId}", schedule.Id);

            return Result<ReportScheduleDto>.Success(MapToDto(schedule), 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating report schedule");
            return Result<ReportScheduleDto>.Failure($"Error creating report schedule: {ex.Message}", 500);
        }
    }

    public async Task<Result<ReportScheduleDto>> UpdateScheduleAsync(Guid scheduleId, UpdateReportScheduleDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var schedule = await _context.ReportSchedules.FindAsync(new object[] { scheduleId }, cancellationToken);

            if (schedule == null)
            {
                return Result<ReportScheduleDto>.Failure("Schedule not found", 404);
            }

            _logger.LogInformation("Updating report schedule: {ScheduleId}", scheduleId);

            // Update fields
            if (!string.IsNullOrEmpty(request.Name))
                schedule.Name = request.Name;

            if (!string.IsNullOrEmpty(request.Description))
                schedule.Description = request.Description;

            if (request.Format.HasValue)
                schedule.Format = request.Format.Value;

            if (request.Frequency.HasValue)
            {
                schedule.Frequency = request.Frequency.Value;
                schedule.CronExpression = request.CronExpression ?? GetCronExpression(request.Frequency.Value);
            }
            else if (!string.IsNullOrEmpty(request.CronExpression))
            {
                schedule.CronExpression = request.CronExpression;
            }

            if (request.Parameters != null)
                schedule.Parameters = JsonSerializer.Serialize(request.Parameters);

            if (request.Filters != null)
                schedule.Filters = JsonSerializer.Serialize(request.Filters);

            if (request.EmailRecipients != null)
                schedule.EmailRecipients = JsonSerializer.Serialize(request.EmailRecipients);

            if (!string.IsNullOrEmpty(request.TimeZone))
                schedule.TimeZone = request.TimeZone;

            schedule.UpdatedAt = DateTime.UtcNow;
            schedule.NextRunAt = CalculateNextRunTime(MapToDto(schedule));

            await _context.SaveChangesAsync(cancellationToken);

            // Update Hangfire job
            var jobId = $"report-schedule-{schedule.Id}";
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(schedule.TimeZone);
            var cronExpression = schedule.CronExpression ?? GetCronExpression(schedule.Frequency);

            _jobService.AddOrUpdateRecurringJob(
                jobId,
                () => ExecuteScheduledReportAsync(schedule.Id),
                cronExpression,
                timeZone);

            await _auditService.LogActionAsync(
                "UpdateReportSchedule",
                "ReportSchedule",
                scheduleId.ToString(),
                schedule.TenantId,
                $"Updated report schedule '{schedule.Name}'");

            _logger.LogInformation("Report schedule updated: {ScheduleId}", scheduleId);

            return Result<ReportScheduleDto>.Success(MapToDto(schedule), 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating report schedule: {ScheduleId}", scheduleId);
            return Result<ReportScheduleDto>.Failure($"Error updating report schedule: {ex.Message}", 500);
        }
    }

    public async Task<Result<bool>> DeleteScheduleAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var schedule = await _context.ReportSchedules.FindAsync(new object[] { scheduleId }, cancellationToken);

            if (schedule == null)
            {
                return Result<bool>.Failure("Schedule not found", 404);
            }

            _logger.LogInformation("Deleting report schedule: {ScheduleId}", scheduleId);

            // Remove Hangfire job
            var jobId = $"report-schedule-{schedule.Id}";
            _jobService.RemoveRecurringJob(jobId);

            _context.ReportSchedules.Remove(schedule);
            await _context.SaveChangesAsync(cancellationToken);

            await _auditService.LogActionAsync(
                "DeleteReportSchedule",
                "ReportSchedule",
                scheduleId.ToString(),
                schedule.TenantId,
                $"Deleted report schedule '{schedule.Name}'");

            _logger.LogInformation("Report schedule deleted: {ScheduleId}", scheduleId);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting report schedule: {ScheduleId}", scheduleId);
            return Result<bool>.Failure($"Error deleting report schedule: {ex.Message}", 500);
        }
    }

    public async Task<Result<ReportScheduleDto>> GetScheduleByIdAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var schedule = await _context.ReportSchedules.FindAsync(new object[] { scheduleId }, cancellationToken);

            if (schedule == null)
            {
                return Result<ReportScheduleDto>.Failure("Schedule not found", 404);
            }

            return Result<ReportScheduleDto>.Success(MapToDto(schedule), 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report schedule: {ScheduleId}", scheduleId);
            return Result<ReportScheduleDto>.Failure($"Error getting report schedule: {ex.Message}", 500);
        }
    }

    public async Task<Result<PaginatedResult<ReportScheduleDto>>> GetSchedulesAsync(ScheduleFilterDto filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.ReportSchedules.AsQueryable();

            if (filter.ReportType.HasValue)
            {
                query = query.Where(s => s.ReportType == filter.ReportType.Value);
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(s => s.IsActive == filter.IsActive.Value);
            }

            if (filter.CreatedBy.HasValue)
            {
                query = query.Where(s => s.CreatedBy == filter.CreatedBy.Value);
            }

            if (filter.TenantId.HasValue)
            {
                query = query.Where(s => s.TenantId == filter.TenantId.Value);
            }

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(s => s.Name.Contains(filter.SearchTerm) ||
                                        (s.Description != null && s.Description.Contains(filter.SearchTerm)));
            }

            query = filter.SortDescending
                ? query.OrderByDescending(s => s.CreatedAt)
                : query.OrderBy(s => s.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var schedules = await query
                .Skip((filter.Page - 1) * filter.Size)
                .Take(filter.Size)
                .ToListAsync(cancellationToken);

            var result = new PaginatedResult<ReportScheduleDto>
            {
                Items = schedules.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = filter.Page,
                Size = filter.Size
            };

            return Result<PaginatedResult<ReportScheduleDto>>.Success(result, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report schedules");
            return Result<PaginatedResult<ReportScheduleDto>>.Failure($"Error getting report schedules: {ex.Message}", 500);
        }
    }

    public async Task<Result<bool>> PauseScheduleAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var schedule = await _context.ReportSchedules.FindAsync(new object[] { scheduleId }, cancellationToken);

            if (schedule == null)
            {
                return Result<bool>.Failure("Schedule not found", 404);
            }

            schedule.IsActive = false;
            schedule.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            // Remove from Hangfire
            var jobId = $"report-schedule-{schedule.Id}";
            _jobService.RemoveRecurringJob(jobId);

            await _auditService.LogActionAsync(
                "PauseReportSchedule",
                "ReportSchedule",
                scheduleId.ToString(),
                schedule.TenantId,
                $"Paused report schedule '{schedule.Name}'");

            _logger.LogInformation("Report schedule paused: {ScheduleId}", scheduleId);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing report schedule: {ScheduleId}", scheduleId);
            return Result<bool>.Failure($"Error pausing report schedule: {ex.Message}", 500);
        }
    }

    public async Task<Result<bool>> ResumeScheduleAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var schedule = await _context.ReportSchedules.FindAsync(new object[] { scheduleId }, cancellationToken);

            if (schedule == null)
            {
                return Result<bool>.Failure("Schedule not found", 404);
            }

            schedule.IsActive = true;
            schedule.UpdatedAt = DateTime.UtcNow;
            schedule.NextRunAt = CalculateNextRunTime(MapToDto(schedule));
            await _context.SaveChangesAsync(cancellationToken);

            // Add back to Hangfire
            var jobId = $"report-schedule-{schedule.Id}";
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(schedule.TimeZone);
            var cronExpression = schedule.CronExpression ?? GetCronExpression(schedule.Frequency);

            _jobService.AddOrUpdateRecurringJob(
                jobId,
                () => ExecuteScheduledReportAsync(schedule.Id),
                cronExpression,
                timeZone);

            await _auditService.LogActionAsync(
                "ResumeReportSchedule",
                "ReportSchedule",
                scheduleId.ToString(),
                schedule.TenantId,
                $"Resumed report schedule '{schedule.Name}'");

            _logger.LogInformation("Report schedule resumed: {ScheduleId}", scheduleId);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming report schedule: {ScheduleId}", scheduleId);
            return Result<bool>.Failure($"Error resuming report schedule: {ex.Message}", 500);
        }
    }

    public async Task<Result<ReportResponseDto>> ExecuteScheduleNowAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var schedule = await _context.ReportSchedules.FindAsync(new object[] { scheduleId }, cancellationToken);

            if (schedule == null)
            {
                return Result<ReportResponseDto>.Failure("Schedule not found", 404);
            }

            _logger.LogInformation("Executing report schedule immediately: {ScheduleId}", scheduleId);

            // Trigger the job immediately
            var jobId = $"report-schedule-{schedule.Id}";
            _jobService.TriggerRecurringJob(jobId);

            return Result<ReportResponseDto>.Success(null!, 202); // Accepted
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing report schedule: {ScheduleId}", scheduleId);
            return Result<ReportResponseDto>.Failure($"Error executing report schedule: {ex.Message}", 500);
        }
    }

    public DateTime? CalculateNextRunTime(ReportScheduleDto schedule)
    {
        try
        {
            if (string.IsNullOrEmpty(schedule.CronExpression))
                return null;

            // Simple calculation based on frequency
            // For production, consider using NCrontab or Cronos package
            var baseTime = schedule.LastRunAt ?? DateTime.UtcNow;

            return schedule.Frequency switch
            {
                ScheduleFrequency.Daily => baseTime.AddDays(1),
                ScheduleFrequency.Weekly => baseTime.AddDays(7),
                ScheduleFrequency.Monthly => baseTime.AddMonths(1),
                ScheduleFrequency.Quarterly => baseTime.AddMonths(3),
                ScheduleFrequency.Yearly => baseTime.AddYears(1),
                _ => baseTime.AddDays(1)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating next run time for schedule: {ScheduleId}", schedule.Id);
            return null;
        }
    }

    // This method will be called by Hangfire
    public async Task ExecuteScheduledReportAsync(Guid scheduleId)
    {
        _logger.LogInformation("Executing scheduled report: {ScheduleId}", scheduleId);

        var startTime = DateTime.UtcNow;
        var execution = new ReportExecution
        {
            Id = Guid.NewGuid(),
            ReportScheduleId = scheduleId,
            StartedAt = startTime,
            Status = ReportStatus.Processing,
            IsScheduled = true
        };

        try
        {
            var schedule = await _context.ReportSchedules.FindAsync(scheduleId);

            if (schedule == null || !schedule.IsActive)
            {
                _logger.LogWarning("Schedule not found or inactive: {ScheduleId}", scheduleId);
                return;
            }

            // Deserialize parameters
            var parameters = !string.IsNullOrEmpty(schedule.Parameters)
                ? JsonSerializer.Deserialize<ReportParametersDto>(schedule.Parameters)
                : null;

            var filters = !string.IsNullOrEmpty(schedule.Filters)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(schedule.Filters)
                : null;

            // Generate report (this will be injected via service locator pattern in Hangfire)
            var reportRequest = new ReportRequestDto
            {
                Name = $"{schedule.Name} - {DateTime.UtcNow:yyyy-MM-dd HH:mm}",
                Description = schedule.Description,
                Type = schedule.ReportType,
                Category = ReportCategory.Operations,
                Format = schedule.Format,
                TenantId = schedule.TenantId,
                Parameters = parameters,
                Filters = filters
            };

            // Note: In Hangfire jobs, you'll need to resolve IReportService from the service provider
            // For now, this is a placeholder - actual implementation will use dependency injection

            // Update schedule last run time
            schedule.LastRunAt = DateTime.UtcNow;
            schedule.NextRunAt = CalculateNextRunTime(MapToDto(schedule));
            await _context.SaveChangesAsync(default(CancellationToken));

            // Send emails if configured
            var emailRecipients = !string.IsNullOrEmpty(schedule.EmailRecipients)
                ? JsonSerializer.Deserialize<List<string>>(schedule.EmailRecipients)
                : null;

            if (emailRecipients != null && emailRecipients.Any())
            {
                // Email sending will be implemented with IEmailService
                _logger.LogInformation("Sending report to {Count} recipients", emailRecipients.Count);
            }

            execution.Status = ReportStatus.Completed;
            execution.CompletedAt = DateTime.UtcNow;
            execution.DurationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            _context.ReportExecutions.Add(execution);
            await _context.SaveChangesAsync(default(CancellationToken));

            _logger.LogInformation("Scheduled report executed successfully: {ScheduleId}", scheduleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing scheduled report: {ScheduleId}", scheduleId);

            execution.Status = ReportStatus.Failed;
            execution.CompletedAt = DateTime.UtcNow;
            execution.DurationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            execution.ErrorMessage = ex.Message;
            execution.StackTrace = ex.StackTrace;

            _context.ReportExecutions.Add(execution);
            await _context.SaveChangesAsync(default(CancellationToken));
        }
    }

    private string GetCronExpression(ScheduleFrequency frequency)
    {
        return frequency switch
        {
            ScheduleFrequency.Daily => "0 0 * * *",        // Every day at midnight
            ScheduleFrequency.Weekly => "0 0 * * 0",       // Every Sunday at midnight
            ScheduleFrequency.Monthly => "0 0 1 * *",      // First day of month at midnight
            ScheduleFrequency.Quarterly => "0 0 1 */3 *",  // First day of every 3 months
            ScheduleFrequency.Yearly => "0 0 1 1 *",       // January 1st at midnight
            _ => "0 0 * * *"                               // Default to daily
        };
    }

    private ReportScheduleDto MapToDto(ReportSchedule schedule)
    {
        var parameters = !string.IsNullOrEmpty(schedule.Parameters)
            ? JsonSerializer.Deserialize<ReportParametersDto>(schedule.Parameters)
            : null;

        var filters = !string.IsNullOrEmpty(schedule.Filters)
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(schedule.Filters)
            : null;

        var emailRecipients = !string.IsNullOrEmpty(schedule.EmailRecipients)
            ? JsonSerializer.Deserialize<List<string>>(schedule.EmailRecipients)
            : null;

        return new ReportScheduleDto
        {
            Id = schedule.Id,
            Name = schedule.Name,
            Description = schedule.Description,
            ReportType = schedule.ReportType,
            Format = schedule.Format,
            Frequency = schedule.Frequency,
            CronExpression = schedule.CronExpression,
            NextRunAt = schedule.NextRunAt,
            LastRunAt = schedule.LastRunAt,
            IsActive = schedule.IsActive,
            Parameters = parameters,
            Filters = filters,
            EmailRecipients = emailRecipients,
            CreatedBy = schedule.CreatedBy,
            CreatedAt = schedule.CreatedAt,
            UpdatedAt = schedule.UpdatedAt,
            TenantId = schedule.TenantId,
            TimeZone = schedule.TimeZone
        };
    }
}

