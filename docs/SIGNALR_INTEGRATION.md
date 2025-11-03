# SignalR Real-Time Notifications - Integration Guide

## Overview

The notification system uses SignalR to provide real-time communication between the server and clients. Messages are automatically scoped to tenants, ensuring data isolation.

---

## Architecture

```
┌─────────────────────────────────────────────┐
│           Backend (ASP.NET Core)            │
│                                             │
│  ┌─────────────────────────────────────┐   │
│  │     NotificationHub                 │   │
│  │  /hubs/notifications                │   │
│  └─────────────────────────────────────┘   │
│                ↑                            │
│  ┌─────────────────────────────────────┐   │
│  │   INotificationService               │   │
│  │  (Send messages programmatically)   │   │
│  └─────────────────────────────────────┘   │
└─────────────────────────────────────────────┘
                 ↓ WebSocket
┌─────────────────────────────────────────────┐
│              Clients                        │
│  ┌──────────────┐      ┌──────────────┐    │
│  │   Next.js    │      │     MAUI     │    │
│  │  (Web App)   │      │  (Desktop)   │    │
│  └──────────────┘      └──────────────┘    │
└─────────────────────────────────────────────┘
```

---

## Backend Implementation

### 1. NotificationHub (Already Implemented)

**Location**: `src/Web/Hubs/NotificationHub.cs`

**Features**:
- Automatic tenant grouping on connection
- User-specific groups
- Room/channel support
- Connection logging

**Groups Created Automatically**:
- `tenant_{tenantId}` - All users in a tenant
- `user_{userId}` - Specific user
- `tenant_{tenantId}_room_{roomName}` - Custom rooms

### 2. INotificationService (Already Implemented)

**Location**: `src/Web/Services/NotificationService.cs`

**Usage in Your Code**:

```csharp
public class ReservationService
{
    private readonly INotificationService _notifications;

    public async Task CreateReservationAsync(CreateReservationDto dto)
    {
        // Create reservation
        var reservation = await _repository.CreateAsync(dto);

        // Notify relevant users
        await _notifications.SendToTenantAsync(
            tenantId: reservation.TenantId,
            message: $"New reservation created for Room {reservation.RoomNumber}",
            data: new { reservationId = reservation.Id, roomNumber = reservation.RoomNumber }
        );

        // Notify specific user (receptionist)
        await _notifications.SendToUserAsync(
            userId: reservation.CreatedBy,
            message: "Your reservation has been confirmed",
            data: reservation
        );
    }
}
```

### 3. Sending Notifications from Commands

```csharp
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IUserRepository _repository;
    private readonly INotificationService _notifications;
    private readonly ITenantContext _tenantContext;

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser { ... };
        await _repository.AddAsync(user);

        // Send notification to all administrators
        await _notifications.SendToRoleAsync(
            tenantId: _tenantContext.TenantId!.Value,
            roleName: "Administrator",
            message: $"New user created: {user.FullName}",
            data: new { userId = user.Id, email = user.Email }
        );

        return Result<Guid>.Success(user.Id);
    }
}
```

---

## Frontend Integration

### Next.js (Web App) Integration

#### 1. Install SignalR Client

```bash
npm install @microsoft/signalr
```

#### 2. Create SignalR Service

**File**: `lib/signalr-service.ts`

```typescript
import * as signalR from '@microsoft/signalr';

export interface Notification {
  id: string;
  type: 'Info' | 'Success' | 'Warning' | 'Error';
  title: string;
  message: string;
  data?: any;
  timestamp: string;
  read: boolean;
}

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private listeners: Map<string, Set<(data: any) => void>> = new Map();

  async connect(tenantIdentifier: string, accessToken: string) {
    // Create connection
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`https://your-api.com/hubs/notifications`, {
        accessTokenFactory: () => accessToken,
        headers: {
          'X-Tenant-Identifier': tenantIdentifier
        },
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // Exponential backoff: 0s, 2s, 10s, 30s, then every 60s
          if (retryContext.previousRetryCount === 0) return 0;
          if (retryContext.previousRetryCount === 1) return 2000;
          if (retryContext.previousRetryCount === 2) return 10000;
          if (retryContext.previousRetryCount === 3) return 30000;
          return 60000; // Max 1 minute
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Handle reconnection
    this.connection.onreconnecting((error) => {
      console.warn('SignalR reconnecting...', error);
    });

    this.connection.onreconnected((connectionId) => {
      console.log('SignalR reconnected:', connectionId);
    });

    this.connection.onclose((error) => {
      console.error('SignalR connection closed:', error);
    });

    // Setup event handlers
    this.setupEventHandlers();

    // Start connection
    try {
      await this.connection.start();
      console.log('SignalR connected successfully');
    } catch (error) {
      console.error('Error connecting to SignalR:', error);
      throw error;
    }
  }

  private setupEventHandlers() {
    if (!this.connection) return;

    // Listen for notifications
    this.connection.on('ReceiveNotification', (notification: Notification) => {
      console.log('Notification received:', notification);
      this.emit('notification', notification);
    });

    // Listen for system notifications
    this.connection.on('ReceiveSystemNotification', (notification: Notification) => {
      console.log('System notification received:', notification);
      this.emit('systemNotification', notification);
    });

    // User joined/left room
    this.connection.on('UserJoined', (data: any) => {
      this.emit('userJoined', data);
    });

    this.connection.on('UserLeft', (data: any) => {
      this.emit('userLeft', data);
    });

    // Custom messages
    this.connection.on('ReceiveMessage', (message: string) => {
      this.emit('message', message);
    });
  }

  // Event emitter pattern
  on(event: string, callback: (data: any) => void) {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set());
    }
    this.listeners.get(event)!.add(callback);
  }

  off(event: string, callback: (data: any) => void) {
    this.listeners.get(event)?.delete(callback);
  }

  private emit(event: string, data: any) {
    this.listeners.get(event)?.forEach(callback => callback(data));
  }

  // Join a room
  async joinRoom(roomName: string) {
    if (!this.connection) throw new Error('Not connected');
    await this.connection.invoke('JoinRoom', roomName);
  }

  // Leave a room
  async leaveRoom(roomName: string) {
    if (!this.connection) throw new Error('Not connected');
    await this.connection.invoke('LeaveRoom', roomName);
  }

  // Send message to tenant
  async sendToTenant(message: string) {
    if (!this.connection) throw new Error('Not connected');
    await this.connection.invoke('SendToTenant', message);
  }

  // Send message to specific user
  async sendToUser(targetUserId: string, message: string) {
    if (!this.connection) throw new Error('Not connected');
    await this.connection.invoke('SendToUser', targetUserId, message);
  }

  async disconnect() {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      this.listeners.clear();
    }
  }

  get connectionState() {
    return this.connection?.state || 'Disconnected';
  }
}

export const signalRService = new SignalRService();
```

#### 3. Create React Hook

**File**: `hooks/useNotifications.ts`

```typescript
import { useEffect, useState, useCallback } from 'react';
import { signalRService, Notification } from '@/lib/signalr-service';
import { useAuth } from '@/hooks/useAuth'; // Your auth hook
import { useTenant } from '@/hooks/useTenant'; // Your tenant hook

export function useNotifications() {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [isConnected, setIsConnected] = useState(false);
  const { accessToken } = useAuth();
  const { tenantIdentifier } = useTenant();

  useEffect(() => {
    if (!accessToken || !tenantIdentifier) return;

    // Connect to SignalR
    signalRService.connect(tenantIdentifier, accessToken)
      .then(() => setIsConnected(true))
      .catch(error => console.error('Failed to connect:', error));

    // Listen for notifications
    const handleNotification = (notification: Notification) => {
      setNotifications(prev => [notification, ...prev]);
      
      // Show toast notification
      toast.info(notification.message, {
        description: notification.title
      });
    };

    signalRService.on('notification', handleNotification);

    // Cleanup
    return () => {
      signalRService.off('notification', handleNotification);
      signalRService.disconnect();
      setIsConnected(false);
    };
  }, [accessToken, tenantIdentifier]);

  const markAsRead = useCallback((notificationId: string) => {
    setNotifications(prev =>
      prev.map(n => n.id === notificationId ? { ...n, read: true } : n)
    );
  }, []);

  const clearAll = useCallback(() => {
    setNotifications([]);
  }, []);

  return {
    notifications,
    isConnected,
    markAsRead,
    clearAll,
    unreadCount: notifications.filter(n => !n.read).length
  };
}
```

#### 4. Use in Components

**File**: `components/NotificationBell.tsx`

```typescript
'use client';

import { useNotifications } from '@/hooks/useNotifications';
import { Bell } from 'lucide-react';

export function NotificationBell() {
  const { notifications, unreadCount, isConnected, markAsRead } = useNotifications();

  return (
    <div className="relative">
      {/* Connection indicator */}
      {isConnected && (
        <div className="absolute -top-1 -right-1 w-2 h-2 bg-green-500 rounded-full animate-pulse" />
      )}

      {/* Bell icon with badge */}
      <button className="relative p-2">
        <Bell className="w-6 h-6" />
        {unreadCount > 0 && (
          <span className="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center">
            {unreadCount}
          </span>
        )}
      </button>

      {/* Notifications dropdown */}
      <div className="absolute right-0 mt-2 w-80 bg-white shadow-lg rounded-lg">
        {notifications.length === 0 ? (
          <p className="p-4 text-gray-500">No notifications</p>
        ) : (
          <ul className="divide-y">
            {notifications.map(notification => (
              <li 
                key={notification.id}
                className={`p-4 hover:bg-gray-50 cursor-pointer ${!notification.read ? 'bg-blue-50' : ''}`}
                onClick={() => markAsRead(notification.id)}
              >
                <div className="font-semibold">{notification.title}</div>
                <div className="text-sm text-gray-600">{notification.message}</div>
                <div className="text-xs text-gray-400 mt-1">
                  {new Date(notification.timestamp).toLocaleString()}
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
```

#### 5. Real-Time Dashboard Updates

```typescript
'use client';

import { useEffect, useState } from 'react';
import { signalRService } from '@/lib/signalr-service';

export function DashboardStats() {
  const [stats, setStats] = useState({
    totalReservations: 0,
    activeUsers: 0,
    revenue: 0
  });

  useEffect(() => {
    // Join dashboard room
    signalRService.joinRoom('dashboard');

    // Listen for stats updates
    const handleStatsUpdate = (data: any) => {
      setStats(prev => ({ ...prev, ...data }));
    };

    signalRService.on('dashboardStatsUpdated', handleStatsUpdate);

    return () => {
      signalRService.leaveRoom('dashboard');
      signalRService.off('dashboardStatsUpdated', handleStatsUpdate);
    };
  }, []);

  return (
    <div className="grid grid-cols-3 gap-4">
      <div className="p-4 bg-white rounded-lg shadow">
        <h3>Reservations</h3>
        <p className="text-3xl font-bold">{stats.totalReservations}</p>
      </div>
      <div className="p-4 bg-white rounded-lg shadow">
        <h3>Active Users</h3>
        <p className="text-3xl font-bold">{stats.activeUsers}</p>
      </div>
      <div className="p-4 bg-white rounded-lg shadow">
        <h3>Revenue</h3>
        <p className="text-3xl font-bold">${stats.revenue}</p>
      </div>
    </div>
  );
}
```

---

## MAUI Desktop App Integration

#### 1. Install NuGet Package

```xml
<!-- In your .csproj file -->
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="9.0.0" />
```

#### 2. Create SignalR Service

**File**: `Services/SignalRService.cs`

```csharp
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;

namespace YourApp.Services;

public class SignalRService
{
    private HubConnection? _connection;
    private readonly string _hubUrl;
    private string? _accessToken;
    private string? _tenantIdentifier;

    public event EventHandler<Notification>? NotificationReceived;
    public event EventHandler<Notification>? SystemNotificationReceived;
    public event EventHandler<string>? ConnectionStateChanged;

    public SignalRService(string baseUrl)
    {
        _hubUrl = $"{baseUrl}/hubs/notifications";
    }

    public async Task ConnectAsync(string tenantIdentifier, string accessToken)
    {
        _accessToken = accessToken;
        _tenantIdentifier = tenantIdentifier;

        _connection = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_accessToken)!;
                options.Headers.Add("X-Tenant-Identifier", _tenantIdentifier);
            })
            .WithAutomaticReconnect(new[] {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30)
            })
            .Build();

        // Setup event handlers
        SetupEventHandlers();

        // Connection events
        _connection.Reconnecting += (error) =>
        {
            Debug.WriteLine($"SignalR reconnecting: {error}");
            ConnectionStateChanged?.Invoke(this, "Reconnecting");
            return Task.CompletedTask;
        };

        _connection.Reconnected += (connectionId) =>
        {
            Debug.WriteLine($"SignalR reconnected: {connectionId}");
            ConnectionStateChanged?.Invoke(this, "Connected");
            return Task.CompletedTask;
        };

        _connection.Closed += (error) =>
        {
            Debug.WriteLine($"SignalR connection closed: {error}");
            ConnectionStateChanged?.Invoke(this, "Disconnected");
            return Task.CompletedTask;
        };

        try
        {
            await _connection.StartAsync();
            ConnectionStateChanged?.Invoke(this, "Connected");
            Debug.WriteLine("SignalR connected successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error connecting to SignalR: {ex.Message}");
            throw;
        }
    }

    private void SetupEventHandlers()
    {
        if (_connection == null) return;

        // Handle notifications
        _connection.On<Notification>("ReceiveNotification", (notification) =>
        {
            NotificationReceived?.Invoke(this, notification);
        });

        // Handle system notifications
        _connection.On<Notification>("ReceiveSystemNotification", (notification) =>
        {
            SystemNotificationReceived?.Invoke(this, notification);
        });

        // Handle messages
        _connection.On<string>("ReceiveMessage", (message) =>
        {
            Debug.WriteLine($"Message received: {message}");
        });
    }

    public async Task JoinRoomAsync(string roomName)
    {
        if (_connection?.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync("JoinRoom", roomName);
        }
    }

    public async Task LeaveRoomAsync(string roomName)
    {
        if (_connection?.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync("LeaveRoom", roomName);
        }
    }

    public async Task SendToTenantAsync(string message)
    {
        if (_connection?.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync("SendToTenant", message);
        }
    }

    public async Task DisconnectAsync()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;
}

public class Notification
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; }
    public bool Read { get; set; }
}
```

#### 3. Use in MAUI Pages

**File**: `MainPage.xaml.cs`

```csharp
public partial class MainPage : ContentPage
{
    private readonly SignalRService _signalR;

    public MainPage(SignalRService signalR)
    {
        InitializeComponent();
        _signalR = signalR;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Get token and tenant from secure storage
        var token = await SecureStorage.GetAsync("access_token");
        var tenant = await SecureStorage.GetAsync("tenant_identifier");

        if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(tenant))
        {
            // Connect to SignalR
            await _signalR.ConnectAsync(tenant, token);

            // Subscribe to notifications
            _signalR.NotificationReceived += OnNotificationReceived;
        }
    }

    private void OnNotificationReceived(object? sender, Notification notification)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Show notification in UI
            DisplayAlert(
                notification.Title,
                notification.Message,
                "OK"
            );

            // Or add to notification list
            // NotificationsList.Add(notification);
        });
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Unsubscribe
        _signalR.NotificationReceived -= OnNotificationReceived;
        
        // Disconnect (optional - keep alive for background notifications)
        // await _signalR.DisconnectAsync();
    }
}
```

#### 4. Register in MauiProgram.cs

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Register SignalR service
        builder.Services.AddSingleton(sp =>
        {
            var apiBaseUrl = "https://your-api.com"; // From configuration
            return new SignalRService(apiBaseUrl);
        });

        return builder.Build();
    }
}
```

---

## Common Scenarios

### Scenario 1: New Reservation Created

**Backend**:
```csharp
public class CreateReservationCommandHandler
{
    public async Task Handle(CreateReservationCommand request)
    {
        var reservation = await _repository.CreateAsync(request);

        // Notify all receptionists
        await _notifications.SendToRoleAsync(
            _tenantContext.TenantId!.Value,
            "Receptionist",
            $"New reservation: Room {reservation.RoomNumber}",
            new {
                reservationId = reservation.Id,
                roomNumber = reservation.RoomNumber,
                guestName = reservation.GuestName,
                checkIn = reservation.CheckInDate
            }
        );
    }
}
```

**Frontend (Next.js)**:
```typescript
signalRService.on('notification', (notification) => {
  if (notification.data?.reservationId) {
    // Navigate to reservation details
    router.push(`/reservations/${notification.data.reservationId}`);
    
    // Or refresh reservations list
    mutate('/api/v1/reservations');
  }
});
```

### Scenario 2: User Status Changed

**Backend**:
```csharp
public class DeactivateUserCommandHandler
{
    public async Task Handle(DeactivateUserCommand request)
    {
        await _userService.DeactivateAsync(request.Id);

        // Notify the user
        await _notifications.SendToUserAsync(
            request.Id.ToString(),
            "Your account has been deactivated",
            new { reason = "Administrator action" }
        );

        // Notify administrators
        await _notifications.SendToRoleAsync(
            _tenantContext.TenantId!.Value,
            "Administrator",
            $"User deactivated: {user.Email}",
            new { userId = request.Id }
        );
    }
}
```

### Scenario 3: System Maintenance Alert

**Backend**:
```csharp
// From a background job or admin action
public class SendMaintenanceAlertCommand
{
    public async Task Execute()
    {
        await _notifications.SendSystemNotificationAsync(
            "System maintenance scheduled for 2:00 AM UTC",
            new {
                maintenanceWindow = "2:00 AM - 4:00 AM UTC",
                affectedServices = new[] { "Reporting", "Backups" }
            }
        );
    }
}
```

---

## Testing SignalR

### Test with Browser Console

```javascript
// Connect
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:5001/hubs/notifications", {
        accessTokenFactory: () => "YOUR-JWT-TOKEN",
        headers: { "X-Tenant-Identifier": "testhotel" }
    })
    .build();

await connection.start();
console.log("Connected!");

// Listen for notifications
connection.on("ReceiveNotification", (notification) => {
    console.log("Notification:", notification);
});

// Join a room
await connection.invoke("JoinRoom", "dashboard");

// Send message to tenant
await connection.invoke("SendToTenant", "Hello from browser!");
```

### Test from Postman/REST Client

Since SignalR uses WebSockets, test via browser console or dedicated clients.

---

## Security Considerations

### 1. Authentication Required

All SignalR connections require a valid JWT token:

```typescript
// Token is automatically included
.withUrl(hubUrl, {
    accessTokenFactory: () => accessToken
})
```

### 2. Tenant Isolation

Users can only:
- ✅ Join their own tenant's groups
- ✅ Send messages within their tenant
- ❌ Access other tenants' notifications

### 3. Authorization

Protected hub methods check user roles:

```csharp
[Authorize(Roles = "Administrator")]
public class AdminHub : Hub
{
    // Only administrators can call these methods
}
```

---

## Troubleshooting

### Issue: "Connection Failed"

**Check**:
1. CORS is configured for WebSockets
2. Token is valid and not expired
3. Tenant identifier is correct

**Solution**:
```typescript
connection.on('error', (error) => {
    console.error('SignalR error:', error);
    // Handle token refresh
    if (error.message.includes('401')) {
        await refreshToken();
        await connection.start();
    }
});
```

### Issue: "Not Receiving Notifications"

**Check**:
1. Connection state: `connection.state === 'Connected'`
2. Event handler is registered before connection starts
3. Backend is sending to correct group

**Debug**:
```csharp
// Backend - add logging
_logger.LogInformation(
    "Sending notification to group: {Group}",
    $"tenant_{tenantId}"
);
```

### Issue: "Connection Drops Frequently"

**Solution**: Configure keep-alive

**Backend**:
```csharp
services.AddSignalR(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});
```

**Frontend**:
```typescript
.withAutomaticReconnect()
.withServerTimeout(30000)
.withKeepAliveInterval(15000)
```

---

## Performance Optimization

### 1. Use Groups Efficiently

```csharp
// ✅ Good: Use specific groups
await Clients.Group($"tenant_{tenantId}_role_Manager").SendAsync(...);

// ❌ Bad: Broadcasting to all
await Clients.All.SendAsync(...); // Avoid unless necessary
```

### 2. Batch Notifications

```csharp
// Instead of sending 100 individual notifications
for (int i = 0; i < 100; i++)
{
    await _notifications.SendToUserAsync(...); // Slow
}

// Send one batch notification
var users = await _repository.GetAllAsync();
await _notifications.SendToRoleAsync(
    tenantId,
    "User",
    "Batch update complete",
    new { affectedUsers = users.Select(u => u.Id) }
);
```

### 3. Client-Side Debouncing

```typescript
// Debounce rapid notifications
const debouncedHandler = debounce((notification) => {
    showNotification(notification);
}, 500);

signalRService.on('notification', debouncedHandler);
```

---

## Advanced Features

### Custom Events

**Backend**:
```csharp
// Define custom event
public async Task BroadcastRoomStatusChange(Guid roomId, string status)
{
    await Clients.Group($"tenant_{_tenantContext.TenantId}")
        .SendAsync("RoomStatusChanged", new {
            roomId,
            status,
            timestamp = DateTime.UtcNow
        });
}
```

**Frontend**:
```typescript
signalRService.on('RoomStatusChanged', (data) => {
    updateRoomStatus(data.roomId, data.status);
});
```

### Typing Indicators

**Backend**:
```csharp
public async Task UserTyping(string roomName)
{
    await Clients.OthersInGroup($"tenant_{_tenantContext.TenantId}_room_{roomName}")
        .SendAsync("UserIsTyping", new {
            userId = _currentUser.Id,
            userName = _currentUser.UserName
        });
}
```

**Frontend**:
```typescript
// Show typing indicator
signalRService.on('UserIsTyping', (data) => {
    setTypingUsers(prev => [...prev, data.userName]);
    setTimeout(() => removeTypingUser(data.userName), 3000);
});
```

---

## Production Deployment

### Redis Backplane (Multi-Instance)

For multiple server instances, enable Redis backplane:

**appsettings.Production.json**:
```json
{
  "SignalR": {
    "UseRedisBackplane": true
  }
}
```

**Code** (uncomment in DependencyInjection.cs):
```csharp
services.AddSignalR()
    .AddStackExchangeRedis(configuration.GetConnectionString("cache"));
```

### Azure SignalR Service

For massive scale, use Azure SignalR Service:

```csharp
services.AddSignalR()
    .AddAzureSignalR(options =>
    {
        options.ConnectionString = configuration["Azure:SignalR:ConnectionString"];
    });
```

---

## Related Documentation

- [SignalR Client API](https://docs.microsoft.com/aspnet/core/signalr/javascript-client)
- [SignalR in .NET MAUI](https://docs.microsoft.com/dotnet/maui/data-cloud/azure/signalr)
- [Next.js Real-Time](https://nextjs.org/docs/app/building-your-application/routing/loading-ui-and-streaming)

