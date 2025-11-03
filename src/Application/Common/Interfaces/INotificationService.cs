namespace HotelManagement.Application.Common.Interfaces;

/// <summary>
/// Service for sending real-time notifications to users
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification to a specific user
    /// </summary>
    Task SendToUserAsync(string userId, string message, object? data = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification to all users in a tenant
    /// </summary>
    Task SendToTenantAsync(Guid tenantId, string message, object? data = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification to all users in a specific role
    /// </summary>
    Task SendToRoleAsync(Guid tenantId, string roleName, string message, object? data = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification to users in a specific branch
    /// </summary>
    Task SendToBranchAsync(Guid branchId, string message, object? data = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a system-wide notification to all connected users
    /// </summary>
    Task SendSystemNotificationAsync(string message, object? data = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Notification types
/// </summary>
public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// Notification message
/// </summary>
public class Notification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool Read { get; set; } = false;
}

