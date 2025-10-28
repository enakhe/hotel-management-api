using HotelManagement.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Application.Common.Services;

/// <summary>
/// Logger that automatically includes tenant context in log messages
/// </summary>
public class TenantAwareLogger<T> : ILogger<T>
{
    private readonly ILogger<T> _logger;
    private readonly ITenantContext _tenantContext;

    public TenantAwareLogger(ILogger<T> logger, ITenantContext tenantContext)
    {
        _logger = logger;
        _tenantContext = tenantContext;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return _logger.BeginScope(state);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _logger.IsEnabled(logLevel);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        // Create tenant-aware state
        var tenantState = new TenantLogState<TState>(_tenantContext, state);
        _logger.Log(logLevel, eventId, tenantState, exception, (s, ex) => formatter(s.OriginalState, ex));
    }

    private record TenantLogState<TState>(ITenantContext TenantContext, TState OriginalState)
    {
        public override string ToString()
        {
            var tenantInfo = TenantContext.IsResolved
                ? $"TenantId={TenantContext.TenantId}, TenantIdentifier={TenantContext.TenantIdentifier}"
                : "TenantId=null";

            return $"[{tenantInfo}] {OriginalState}";
        }
    }
}
