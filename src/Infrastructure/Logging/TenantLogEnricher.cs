using HotelManagement.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Logging;

/// <summary>
/// Log enricher that adds tenant context to all log entries
/// </summary>
public class TenantLogEnricher
{
    private readonly ITenantContext? _tenantContext;
    private readonly IUser? _currentUser;

    public TenantLogEnricher(ITenantContext? tenantContext = null, IUser? currentUser = null)
    {
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public IDictionary<string, object> EnrichLogContext()
    {
        var context = new Dictionary<string, object>();

        if (_tenantContext?.IsResolved == true)
        {
            context["TenantId"] = _tenantContext.TenantId?.ToString() ?? "unknown";
            context["TenantIdentifier"] = _tenantContext.TenantIdentifier ?? "unknown";
        }

        if (!string.IsNullOrEmpty(_currentUser?.Id))
        {
            context["UserId"] = _currentUser.Id;
        }

        context["Timestamp"] = DateTime.UtcNow;
        context["MachineName"] = Environment.MachineName;
        context["ProcessId"] = Environment.ProcessId;

        return context;
    }
}

/// <summary>
/// Custom logger that includes tenant context
/// </summary>
public class TenantAwareLogger<T> : ILogger<T>
{
    private readonly ILogger<T> _innerLogger;
    private readonly ITenantContext? _tenantContext;
    private readonly IUser? _currentUser;

    public TenantAwareLogger(
        ILogger<T> innerLogger,
        ITenantContext? tenantContext = null,
        IUser? currentUser = null)
    {
        _innerLogger = innerLogger;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return _innerLogger.BeginScope(state);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _innerLogger.IsEnabled(logLevel);
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        // Create enriched scope
        var enricher = new TenantLogEnricher(_tenantContext, _currentUser);
        var contextData = enricher.EnrichLogContext();

        using (_innerLogger.BeginScope(contextData))
        {
            _innerLogger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}

