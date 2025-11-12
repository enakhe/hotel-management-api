using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Entities;
using HotelManagement.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HotelManagement.Web.Services;

/// <summary>
/// SignalR-based notification service implementation
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendToUserAsync(
        string userId,
        string message,
        object? data = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userGroup = $"user_{userId}";
            
            var notification = new Notification
            {
                Type = NotificationType.Info,
                Title = "Notification",
                Message = message,
                Data = data
            };

            await _hubContext.Clients
                .Group(userGroup)
                .SendAsync("ReceiveNotification", notification, cancellationToken);

            _logger.LogDebug("Notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to user {UserId}", userId);
        }
    }

    public async Task SendToTenantAsync(
        Guid tenantId,
        string message,
        object? data = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantGroup = $"tenant_{tenantId}";
            
            var notification = new Notification
            {
                Type = NotificationType.Info,
                Title = "Tenant Notification",
                Message = message,
                Data = data
            };

            await _hubContext.Clients
                .Group(tenantGroup)
                .SendAsync("ReceiveNotification", notification, cancellationToken);

            _logger.LogDebug("Notification sent to tenant {TenantId}", tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to tenant {TenantId}", tenantId);
        }
    }

    public async Task SendToRoleAsync(
        Guid tenantId,
        string roleName,
        string message,
        object? data = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var roleGroup = $"tenant_{tenantId}_role_{roleName}";
            
            var notification = new Notification
            {
                Type = NotificationType.Info,
                Title = $"Notification for {roleName}",
                Message = message,
                Data = data
            };

            await _hubContext.Clients
                .Group(roleGroup)
                .SendAsync("ReceiveNotification", notification, cancellationToken);

            _logger.LogDebug("Notification sent to role {Role} in tenant {TenantId}", roleName, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to role {Role}", roleName);
        }
    }

    public async Task SendToBranchAsync(
        Guid branchId,
        string message,
        object? data = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var branchGroup = $"branch_{branchId}";
            
            var notification = new Notification
            {
                Type = NotificationType.Info,
                Title = "Branch Notification",
                Message = message,
                Data = data
            };

            await _hubContext.Clients
                .Group(branchGroup)
                .SendAsync("ReceiveNotification", notification, cancellationToken);

            _logger.LogDebug("Notification sent to branch {BranchId}", branchId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to branch {BranchId}", branchId);
        }
    }

    public async Task SendSystemNotificationAsync(
        string message,
        object? data = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = new Notification
            {
                Type = NotificationType.Warning,
                Title = "System Notification",
                Message = message,
                Data = data
            };

            await _hubContext.Clients.All.SendAsync("ReceiveSystemNotification", notification, cancellationToken);

            _logger.LogInformation("System notification sent: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send system notification");
        }
    }
}

