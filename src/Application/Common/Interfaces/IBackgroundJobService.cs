using System.Linq.Expressions;

namespace HotelManagement.Application.Common.Interfaces;

/// <summary>
/// Service for managing background jobs
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Enqueue a background job to be executed once
    /// </summary>
    string Enqueue(Expression<Func<Task>> methodCall);

    /// <summary>
    /// Schedule a job to be executed at a specific time
    /// </summary>
    string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay);

    /// <summary>
    /// Schedule a job to be executed at a specific date/time
    /// </summary>
    string Schedule(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt);

    /// <summary>
    /// Create or update a recurring job
    /// </summary>
    void AddOrUpdateRecurringJob(string jobId, Expression<Func<Task>> methodCall, string cronExpression, TimeZoneInfo? timeZone = null);

    /// <summary>
    /// Remove a recurring job
    /// </summary>
    void RemoveRecurringJob(string jobId);

    /// <summary>
    /// Delete a job
    /// </summary>
    bool DeleteJob(string jobId);

    /// <summary>
    /// Trigger a recurring job immediately
    /// </summary>
    void TriggerRecurringJob(string jobId);
}

