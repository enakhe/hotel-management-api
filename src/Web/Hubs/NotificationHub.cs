using HotelManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HotelManagement.Web.Hubs;

/// <summary>
/// SignalR hub for real-time notifications
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ITenantContext _tenantContext;
    private readonly IUser _currentUser;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(
        ITenantContext tenantContext,
        IUser currentUser,
        ILogger<NotificationHub> logger)
    {
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var userId = _currentUser.Id ?? "anonymous";
        var tenantId = _tenantContext.TenantId?.ToString() ?? "none";

        _logger.LogInformation(
            "User {UserId} from tenant {TenantId} connected with connection ID {ConnectionId}",
            userId,
            tenantId,
            connectionId);

        // Add to tenant group for tenant-specific broadcasts
        if (_tenantContext.IsResolved)
        {
            var tenantGroup = $"tenant_{_tenantContext.TenantId}";
            await Groups.AddToGroupAsync(connectionId, tenantGroup);
            _logger.LogDebug("Added connection {ConnectionId} to group {Group}", connectionId, tenantGroup);
        }

        // Add to user-specific group
        if (!string.IsNullOrEmpty(_currentUser.Id))
        {
            var userGroup = $"user_{_currentUser.Id}";
            await Groups.AddToGroupAsync(connectionId, userGroup);
            _logger.LogDebug("Added connection {ConnectionId} to user group {Group}", connectionId, userGroup);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        var userId = _currentUser.Id ?? "anonymous";

        _logger.LogInformation(
            "User {UserId} disconnected. Connection ID: {ConnectionId}",
            userId,
            connectionId);

        if (exception != null)
        {
            _logger.LogError(
                exception,
                "User {UserId} disconnected with error",
                userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Sends a message to all users in the current tenant
    /// </summary>
    public async Task SendToTenant(string message)
    {
        if (!_tenantContext.IsResolved)
        {
            throw new InvalidOperationException("Tenant context not resolved");
        }

        var tenantGroup = $"tenant_{_tenantContext.TenantId}";
        
        _logger.LogInformation(
            "Broadcasting message to tenant {TenantId}: {Message}",
            _tenantContext.TenantId,
            message);

        await Clients.Group(tenantGroup).SendAsync("ReceiveMessage", message);
    }

    /// <summary>
    /// Sends a message to a specific user
    /// </summary>
    public async Task SendToUser(string targetUserId, string message)
    {
        var userGroup = $"user_{targetUserId}";
        
        _logger.LogInformation(
            "Sending message to user {TargetUserId}: {Message}",
            targetUserId,
            message);

        await Clients.Group(userGroup).SendAsync("ReceiveMessage", message);
    }

    /// <summary>
    /// Joins a specific room/channel
    /// </summary>
    public async Task JoinRoom(string roomName)
    {
        if (!_tenantContext.IsResolved)
        {
            throw new InvalidOperationException("Tenant context not resolved");
        }

        // Scope room to tenant
        var tenantScopedRoom = $"tenant_{_tenantContext.TenantId}_room_{roomName}";
        
        await Groups.AddToGroupAsync(Context.ConnectionId, tenantScopedRoom);
        
        _logger.LogInformation(
            "User {UserId} joined room {Room}",
            _currentUser.Id,
            tenantScopedRoom);

        await Clients.Group(tenantScopedRoom).SendAsync(
            "UserJoined",
            new { userId = _currentUser.Id, roomName });
    }

    /// <summary>
    /// Leaves a specific room/channel
    /// </summary>
    public async Task LeaveRoom(string roomName)
    {
        if (!_tenantContext.IsResolved)
        {
            return;
        }

        var tenantScopedRoom = $"tenant_{_tenantContext.TenantId}_room_{roomName}";
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, tenantScopedRoom);
        
        _logger.LogInformation(
            "User {UserId} left room {Room}",
            _currentUser.Id,
            tenantScopedRoom);

        await Clients.Group(tenantScopedRoom).SendAsync(
            "UserLeft",
            new { userId = _currentUser.Id, roomName });
    }
}

