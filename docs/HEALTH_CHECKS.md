# Health Checks Documentation

## Overview

The application implements comprehensive health checks for monitoring system health and readiness. Health checks are exposed via HTTP endpoints and provide detailed status information about all critical dependencies.

## Health Check Endpoints

### 1. `/health` - Comprehensive Health Status

**Purpose**: Returns detailed health status of all system components

**Response Format**:
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "description": "Database is healthy",
      "duration": 45.2,
      "data": {
        "connection_status": "connected",
        "applied_migrations": 3,
        "pending_migrations": 0,
        "provider": "Microsoft.EntityFrameworkCore.SqlServer"
      }
    },
    {
      "name": "redis",
      "status": "Healthy",
      "description": "Redis is healthy",
      "duration": 12.5,
      "data": {
        "status": "connected",
        "ping_ms": 2.3,
        "endpoints": 1,
        "redis_version": "7.0.5"
      }
    }
  ],
  "totalDuration": 89.7
}
```

**Status Values**:
- `Healthy` - All systems operational
- `Degraded` - System functional but with warnings
- `Unhealthy` - Critical systems down

### 2. `/health/ready` - Readiness Probe

**Purpose**: Kubernetes/container readiness check

**Response Format**:
```json
{
  "status": "Healthy"
}
```

**Use Case**: 
- Kubernetes readiness probes
- Load balancer health checks
- Deployment validation

### 3. `/health/live` - Liveness Probe

**Purpose**: Basic application liveness check

**Response**: HTTP 200 if app is running

**Use Case**:
- Kubernetes liveness probes
- Container orchestration
- Auto-restart triggers

## Individual Health Checks

### 1. Database Health Check

**Component**: `DatabaseHealthCheck`

**Checks**:
- Database connectivity
- Applied migrations count
- Pending migrations detection
- Database provider information

**Status Logic**:
- `Healthy`: Connected, no pending migrations
- `Degraded`: Connected, has pending migrations
- `Unhealthy`: Cannot connect

**Sample Response Data**:
```json
{
  "connection_status": "connected",
  "applied_migrations": 3,
  "pending_migrations": 0,
  "provider": "Microsoft.EntityFrameworkCore.SqlServer"
}
```

### 2. Redis Health Check

**Component**: `RedisHealthCheck`

**Checks**:
- Redis connectivity
- Response time (ping)
- Server information
- Endpoint count

**Status Logic**:
- `Healthy`: Connected, ping < 100ms
- `Degraded`: Connected, ping > 100ms or not configured
- `Unhealthy`: Not connected

**Sample Response Data**:
```json
{
  "status": "connected",
  "ping_ms": 2.3,
  "endpoints": 1,
  "redis_version": "7.0.5"
}
```

### 3. Hangfire Health Check

**Component**: `HangfireHealthCheck`

**Checks**:
- Active Hangfire servers
- Job queue statistics
- Failed job count
- Processing jobs

**Status Logic**:
- `Healthy`: Servers running, failed jobs < 100
- `Degraded`: Servers running, failed jobs > 100
- `Unhealthy`: No servers running

**Sample Response Data**:
```json
{
  "servers": 1,
  "enqueued": 5,
  "scheduled": 10,
  "processing": 2,
  "succeeded": 1523,
  "failed": 3,
  "recurring": 4,
  "queues": 3
}
```

### 4. System Resources Health Check

**Component**: `SystemResourcesHealthCheck`

**Checks**:
- Memory usage (working set, private memory)
- GC statistics
- Disk space availability
- CPU count

**Status Logic**:
- `Healthy`: Sufficient resources
- `Degraded`: High disk usage (>90%) or high Gen2 collections
- `Unhealthy`: Low memory (<500MB) or low disk (<1GB)

**Sample Response Data**:
```json
{
  "working_set_mb": 245,
  "private_memory_mb": 312,
  "gc_memory_mb": 189,
  "gen0_collections": 45,
  "gen1_collections": 12,
  "gen2_collections": 3,
  "available_disk_gb": 45,
  "total_disk_gb": 100,
  "disk_usage_percent": 55.0,
  "cpu_count": 4
}
```

## Using Health Checks

### In Development

```bash
# Check overall health
curl https://localhost:5001/health

# Check readiness
curl https://localhost:5001/health/ready

# Check liveness
curl https://localhost:5001/health/live
```

### In Kubernetes

**Deployment YAML**:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: hotelmanagement-api
spec:
  replicas: 3
  template:
    spec:
      containers:
      - name: api
        image: hotelmanagement-api:latest
        ports:
        - containerPort: 8080
        livenessProbe:
          httpGet:
            path: /health/live
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 3
```

### In Azure App Service

**Application Settings**:
```
WEBSITE_HEALTHCHECK_PATH=/health/ready
```

**Portal Configuration**:
1. Go to App Service → Monitoring → Health Check
2. Enable Health Check
3. Set Path: `/health/ready`
4. Set Interval: 30 seconds

### In Docker Compose

```yaml
services:
  api:
    image: hotelmanagement-api:latest
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health/live"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
```

## Monitoring Integration

### Application Insights

Health check results are automatically logged and can be queried:

```kusto
requests
| where name contains "health"
| summarize 
    HealthyCount = countif(resultCode == "200"),
    UnhealthyCount = countif(resultCode != "200"),
    AvgDuration = avg(duration)
  by bin(timestamp, 5m)
| render timechart
```

### Prometheus

Add Prometheus metrics exporter for health checks:

```csharp
services.AddHealthChecks()
    .ForwardToPrometheus();
```

Exposed metrics:
- `health_check_status{name="database"}` - 1 for healthy, 0 for unhealthy
- `health_check_duration_seconds{name="database"}` - Check duration

### Grafana Dashboard

Create alerts based on health check status:

```
Alert: Database Unhealthy
Condition: health_check_status{name="database"} == 0
For: 2m
Severity: Critical
```

## Custom Health Checks

### Creating a Custom Health Check

```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class EmailServiceHealthCheck : IHealthCheck
{
    private readonly IEmailService _emailService;

    public EmailServiceHealthCheck(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Test SMTP connectivity
            var canConnect = await _emailService.TestConnectionAsync();
            
            if (!canConnect)
            {
                return HealthCheckResult.Degraded(
                    "Email service cannot connect to SMTP server");
            }

            return HealthCheckResult.Healthy("Email service is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Email service health check failed",
                exception: ex);
        }
    }
}
```

### Registering Custom Health Check

```csharp
services.AddHealthChecks()
    .AddCheck<EmailServiceHealthCheck>(
        "email",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "email", "smtp" });
```

## Best Practices

### 1. Health Check Performance

- Keep health checks fast (<2 seconds)
- Use connection pooling
- Cache non-critical checks
- Avoid heavy computations

### 2. Health Check Dependencies

- Order checks by criticality
- Don't cascade failures
- Provide meaningful error messages
- Include actionable data

### 3. Security

- Consider exposing limited health info publicly
- Protect detailed endpoints with authentication
- Don't expose sensitive data in responses

**Example - Public vs Private**:

```csharp
// Public endpoint - minimal info
app.MapHealthChecks("/health/public", new HealthCheckOptions
{
    Predicate = _ => false, // Only liveness
    AllowCachingResponses = true
});

// Private endpoint - detailed info
app.MapHealthChecks("/health/internal", new HealthCheckOptions
{
    Predicate = _ => true
}).RequireAuthorization("HealthCheckPolicy");
```

### 4. Alerting

Set up alerts for:
- Any `Unhealthy` status → Immediate alert
- `Degraded` for >5 minutes → Warning
- Health check failures → Critical alert
- Slow response times → Investigation needed

## Troubleshooting

### Health Check Returns Unhealthy

**Database**:
1. Check connection string in secrets
2. Verify database is running: `sqlcmd -S server -U user`
3. Check firewall rules
4. Verify migrations: `dotnet ef database update`

**Redis**:
1. Check Redis is running: `redis-cli ping`
2. Verify connection string
3. Check network connectivity
4. Review Redis logs

**Hangfire**:
1. Check SQL Server connection (Hangfire uses same DB)
2. Verify Hangfire tables exist
3. Check for Hangfire errors in logs
4. Restart Hangfire servers if needed

### Health Check Timeout

Increase timeout in configuration:

```csharp
services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>(
        "database",
        timeout: TimeSpan.FromSeconds(10));
```

### High Resource Usage Warnings

**Memory**:
```bash
# Force garbage collection
dotnet-dump collect --process-id <pid>
dotnet-gcdump collect --process-id <pid>
```

**Disk**:
```bash
# Clean up old logs
find /app/logs -type f -mtime +30 -delete

# Clean temp files
rm -rf /tmp/*
```

## Health Check Configuration

**appsettings.json**:
```json
{
  "HealthChecks": {
    "MemoryThresholdMB": 500,
    "DiskThresholdGB": 1,
    "DatabaseTimeoutSeconds": 5,
    "RedisTimeoutSeconds": 3
  }
}
```

## Metrics and Analytics

### Key Metrics to Track

1. **Availability**: % of time health checks pass
2. **Response Time**: Average duration of health checks
3. **Failure Rate**: % of failed checks over time
4. **Recovery Time**: Time to recover from unhealthy state

### Sample Query (Application Insights)

```kusto
// Health check success rate over last 24 hours
requests
| where timestamp > ago(24h)
| where name contains "health"
| summarize 
    Total = count(),
    Successful = countif(resultCode == "200"),
    Failed = countif(resultCode != "200")
| extend SuccessRate = (Successful * 100.0) / Total
| project SuccessRate, Total, Successful, Failed
```

## Related Documentation

- [Database Migrations](./DATABASE_MIGRATIONS.md)
- [System Monitoring](./MONITORING.md)
- [Deployment Guide](./DEPLOYMENT.md)
- [Kubernetes Configuration](./KUBERNETES.md)

