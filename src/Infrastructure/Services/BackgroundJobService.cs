using System.Linq.Expressions;
using HotelManagement.Application.Common.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services;

/// <summary>
/// Background job service implementation using Hangfire
/// </summary>
public class BackgroundJobService : IBackgroundJobService
{
    private readonly ILogger<BackgroundJobService> _logger;

    public BackgroundJobService(ILogger<BackgroundJobService> logger)
    {
        _logger = logger;
    }

    public string Enqueue(Expression<Func<Task>> methodCall)
    {
        try
        {
            var jobId = BackgroundJob.Enqueue(methodCall);
            _logger.LogInformation("Background job enqueued with ID: {JobId}", jobId);
            return jobId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enqueueing background job");
            throw;
        }
    }

    public string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay)
    {
        try
        {
            var jobId = BackgroundJob.Schedule(methodCall, delay);
            _logger.LogInformation("Background job scheduled with ID: {JobId}, Delay: {Delay}", jobId, delay);
            return jobId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling background job");
            throw;
        }
    }

    public string Schedule(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt)
    {
        try
        {
            var jobId = BackgroundJob.Schedule(methodCall, enqueueAt);
            _logger.LogInformation("Background job scheduled with ID: {JobId}, EnqueueAt: {EnqueueAt}", jobId, enqueueAt);
            return jobId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling background job");
            throw;
        }
    }

    public void AddOrUpdateRecurringJob(string jobId, Expression<Func<Task>> methodCall, string cronExpression, TimeZoneInfo? timeZone = null)
    {
        try
        {
            var options = new RecurringJobOptions();
            if (timeZone != null)
            {
                options.TimeZone = timeZone;
            }

            RecurringJob.AddOrUpdate(jobId, methodCall, cronExpression, options);
            _logger.LogInformation("Recurring job added/updated: {JobId}, Cron: {CronExpression}", jobId, cronExpression);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding/updating recurring job: {JobId}", jobId);
            throw;
        }
    }

    public void RemoveRecurringJob(string jobId)
    {
        try
        {
            RecurringJob.RemoveIfExists(jobId);
            _logger.LogInformation("Recurring job removed: {JobId}", jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing recurring job: {JobId}", jobId);
            throw;
        }
    }

    public bool DeleteJob(string jobId)
    {
        try
        {
            var result = BackgroundJob.Delete(jobId);
            _logger.LogInformation("Background job deleted: {JobId}, Success: {Success}", jobId, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting background job: {JobId}", jobId);
            return false;
        }
    }

    public void TriggerRecurringJob(string jobId)
    {
        try
        {
            RecurringJob.TriggerJob(jobId);
            _logger.LogInformation("Recurring job triggered: {JobId}", jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering recurring job: {JobId}", jobId);
            throw;
        }
    }
}

