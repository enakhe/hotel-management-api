# Practical Example: Adding Caching & Notifications to Your Services

## Overview

This guide shows step-by-step how to enhance your existing services with caching and real-time notifications.

---

## Example: Enhancing UserService

### BEFORE (Without Caching/Notifications)

```csharp
public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UserService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        return _mapper.Map<UserDto>(user);
    }

    public async Task UpdateUserAsync(UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(dto.Id);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        _mapper.Map(dto, user);
        await _context.SaveChangesAsync();
    }
}
```

**Problems**:

- ❌ Every request hits the database
- ❌ No real-time updates
- ❌ Slow performance under load
- ❌ Users don't know when data changes

---

### AFTER (With Caching & Notifications)

```csharp
public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;              // ← Added
    private readonly INotificationService _notifications; // ← Added
    private readonly ITenantContext _tenantContext;      // ← Added
    private readonly ILogger<UserService> _logger;       // ← Added

    public UserService(
        ApplicationDbContext context,
        IMapper mapper,
        ICacheService cache,
        INotificationService notifications,
        ITenantContext tenantContext,
        ILogger<UserService> logger)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
        _notifications = notifications;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        var cacheKey = CacheKeys.User(userId);

        // Try cache first
        var cachedUser = await _cache.GetAsync<UserDto>(cacheKey);
        if (cachedUser != null)
        {
            _logger.LogDebug("Cache hit for user {UserId}", userId);
            return cachedUser;
        }

        _logger.LogDebug("Cache miss for user {UserId}, fetching from database", userId);

        // Fetch from database
        var user = await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        var userDto = _mapper.Map<UserDto>(user);

        // Store in cache
        await _cache.SetAsync(cacheKey, userDto, TimeSpan.FromMinutes(15));

        return userDto;
    }

    public async Task UpdateUserAsync(UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(dto.Id);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        var oldEmail = user.Email;
        _mapper.Map(dto, user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} updated", user.Id);

        // Invalidate cache
        await _cache.RemoveAsync(CacheKeys.User(user.Id));
        await _cache.RemoveAsync(CacheKeys.UserPermissions(user.Id));
        await _cache.RemoveAsync(CacheKeys.TenantUsers(_tenantContext.TenantId!.Value, 1, 20));

        // Send real-time notification
        await _notifications.SendToTenantAsync(
            _tenantContext.TenantId!.Value,
            $"User profile updated: {user.FullName}",
            new
            {
                userId = user.Id,
                userName = user.FullName,
                changedFields = new[] { "Email", "FirstName", "LastName" }
            }
        );

        // Notify the specific user
        await _notifications.SendToUserAsync(
            user.Id.ToString(),
            "Your profile has been updated",
            new { changes = dto }
        );
    }
}
```

**Benefits**:

- ✅ 90%+ faster reads (cache hits)
- ✅ Real-time UI updates
- ✅ Reduced database load
- ✅ Better user experience

---

## Step-by-Step Implementation

### Step 1: Update Service Constructor

```csharp
// Add these dependencies
private readonly ICacheService _cache;
private readonly INotificationService _notifications;
private readonly ITenantContext _tenantContext;
private readonly ILogger<YourService> _logger;

// Inject in constructor
public YourService(
    // existing dependencies...
    ICacheService cache,
    INotificationService notifications,
    ITenantContext tenantContext,
    ILogger<YourService> logger)
{
    // existing assignments...
    _cache = cache;
    _notifications = notifications;
    _tenantContext = tenantContext;
    _logger = logger;
}
```

### Step 2: Add Caching to GET Methods

**Template**:

```csharp
public async Task<TDto> GetByIdAsync(Guid id)
{
    var cacheKey = CacheKeys.YourEntity(id);

    return await _cache.GetOrCreateAsync(
        cacheKey,
        factory: async () =>
        {
            var entity = await _context.YourEntities
                .Include(e => e.RelatedEntity) // Include what you need
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entity == null)
            {
                throw new NotFoundException(nameof(YourEntity), id);
            }

            return _mapper.Map<TDto>(entity);
        },
        expiration: TimeSpan.FromMinutes(15) // Adjust based on update frequency
    );
}
```

### Step 3: Invalidate Cache on Writes

**Template**:

```csharp
public async Task<Result> UpdateAsync(UpdateDto dto)
{
    // 1. Update database
    var entity = await _context.YourEntities.FindAsync(dto.Id);
    _mapper.Map(dto, entity);
    await _context.SaveChangesAsync();

    // 2. Invalidate cache
    await _cache.RemoveAsync(CacheKeys.YourEntity(dto.Id));
    await _cache.RemoveAsync(CacheKeys.TenantYourEntities(_tenantContext.TenantId!.Value));

    // 3. Send notification
    await _notifications.SendToTenantAsync(
        _tenantContext.TenantId!.Value,
        $"Updated: {entity.Name}",
        new { id = entity.Id, name = entity.Name }
    );

    return Result.Success();
}
```

### Step 4: Frontend Integration

**Next.js Component**:

```typescript
"use client";

import { useEffect } from "react";
import useSWR from "swr";
import { signalRService } from "@/lib/signalr-service";

export function UserList() {
  // SWR provides client-side caching
  const { data: users, mutate } = useSWR("/api/v1/users");

  useEffect(() => {
    // Invalidate cache when backend sends notification
    const handleUpdate = (notification: any) => {
      if (notification.message.includes("User") || notification.data?.userId) {
        mutate(); // Refetch users
      }
    };

    signalRService.on("notification", handleUpdate);
    return () => signalRService.off("notification", handleUpdate);
  }, [mutate]);

  return (
    <div>
      {users?.map((user) => (
        <UserCard key={user.id} user={user} />
      ))}
    </div>
  );
}
```

---

## Real-World Service Examples

### Example 1: Branch Service (Complete)

**File**: `src/Infrastructure/Services/BranchService.cs`

```csharp
public class BranchService : IBranchService
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cache;
    private readonly INotificationService _notifications;
    private readonly ITenantContext _tenantContext;
    private readonly IMapper _mapper;
    private readonly ILogger<BranchService> _logger;

    public BranchService(
        ApplicationDbContext context,
        ICacheService cache,
        INotificationService notifications,
        ITenantContext tenantContext,
        IMapper mapper,
        ILogger<BranchService> logger)
    {
        _context = context;
        _cache = cache;
        _notifications = notifications;
        _tenantContext = tenantContext;
        _mapper = mapper;
        _logger = logger;
    }

    // GET - With caching
    public async Task<BranchDto> GetByIdAsync(Guid branchId)
    {
        var cacheKey = CacheKeys.Branch(branchId);

        return await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                var branch = await _context.Branches
                    .Include(b => b.Users)
                    .Include(b => b.Rooms)
                    .FirstOrDefaultAsync(b => b.Id == branchId);

                if (branch == null)
                {
                    throw new NotFoundException(nameof(Branch), branchId);
                }

                return _mapper.Map<BranchDto>(branch);
            },
            TimeSpan.FromHours(1)
        );
    }

    // LIST - With caching and pagination
    public async Task<List<BranchDto>> GetAllAsync()
    {
        var tenantId = _tenantContext.TenantId!.Value;
        var cacheKey = CacheKeys.TenantBranches(tenantId);

        return await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                var branches = await _context.Branches
                    .Where(b => b.TenantId == tenantId && b.IsActive)
                    .OrderBy(b => b.Name)
                    .ToListAsync();

                return _mapper.Map<List<BranchDto>>(branches);
            },
            TimeSpan.FromMinutes(30)
        );
    }

    // CREATE - Cache new item and notify
    public async Task<Guid> CreateAsync(CreateBranchDto dto)
    {
        var branch = _mapper.Map<Branch>(dto);
        branch.TenantId = _tenantContext.TenantId!.Value;

        await _context.Branches.AddAsync(branch);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Branch created: {BranchId} - {BranchName}", branch.Id, branch.Name);

        // Cache the new branch
        var branchDto = _mapper.Map<BranchDto>(branch);
        await _cache.SetAsync(
            CacheKeys.Branch(branch.Id),
            branchDto,
            TimeSpan.FromHours(1)
        );

        // Invalidate list cache
        await _cache.RemoveAsync(CacheKeys.TenantBranches(branch.TenantId));

        // Notify users
        await _notifications.SendToTenantAsync(
            branch.TenantId,
            $"New branch created: {branch.Name}",
            new { branchId = branch.Id, branchName = branch.Name }
        );

        return branch.Id;
    }

    // UPDATE - Invalidate and notify
    public async Task UpdateAsync(UpdateBranchDto dto)
    {
        var branch = await _context.Branches.FindAsync(dto.Id);
        if (branch == null)
        {
            throw new NotFoundException(nameof(Branch), dto.Id);
        }

        var oldName = branch.Name;
        _mapper.Map(dto, branch);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Branch updated: {BranchId} - {BranchName}", branch.Id, branch.Name);

        // Invalidate cache
        await _cache.RemoveAsync(CacheKeys.Branch(branch.Id));
        await _cache.RemoveAsync(CacheKeys.TenantBranches(branch.TenantId));

        // Notify users of the change
        await _notifications.SendToTenantAsync(
            branch.TenantId,
            $"Branch updated: {oldName} → {branch.Name}",
            new
            {
                branchId = branch.Id,
                oldName,
                newName = branch.Name
            }
        );
    }

    // DELETE - Invalidate and notify
    public async Task DeleteAsync(Guid branchId)
    {
        var branch = await _context.Branches.FindAsync(branchId);
        if (branch == null)
        {
            throw new NotFoundException(nameof(Branch), branchId);
        }

        _context.Branches.Remove(branch);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Branch deleted: {BranchId} - {BranchName}", branch.Id, branch.Name);

        // Invalidate cache
        await _cache.RemoveAsync(CacheKeys.Branch(branchId));
        await _cache.RemoveAsync(CacheKeys.TenantBranches(branch.TenantId));

        // Notify users
        await _notifications.SendToTenantAsync(
            branch.TenantId,
            $"Branch deleted: {branch.Name}",
            new { branchId = branch.Id }
        );
    }
}
```

---

## Frontend Integration Examples

### Next.js Complete Example

#### 1. API Client with Caching

**File**: `lib/api-client.ts`

```typescript
import axios from "axios";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "https://localhost:5001";

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    "Content-Type": "application/json",
  },
});

// Add request interceptor for auth token and tenant
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("access_token");
  const tenantId = localStorage.getItem("tenant_identifier");

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  if (tenantId) {
    config.headers["X-Tenant-Identifier"] = tenantId;
  }

  return config;
});

// Branch API
export const branchAPI = {
  getAll: () => apiClient.get<BranchDto[]>("/api/v1/administrator/branch"),
  getById: (id: string) =>
    apiClient.get<BranchDto>(`/api/v1/administrator/branch/${id}`),
  create: (data: CreateBranchDto) =>
    apiClient.post("/api/v1/administrator/branch", data),
  update: (data: UpdateBranchDto) =>
    apiClient.put("/api/v1/administrator/branch", data),
  delete: (id: string) =>
    apiClient.delete(`/api/v1/administrator/branch/${id}`),
};
```

#### 2. React Hook with Cache + Real-Time

**File**: `hooks/useBranches.ts`

```typescript
import useSWR from "swr";
import { branchAPI } from "@/lib/api-client";
import { signalRService } from "@/lib/signalr-service";
import { useEffect } from "react";

const fetcher = (url: string) => branchAPI.getAll().then((res) => res.data);

export function useBranches() {
  const { data, error, isLoading, mutate } = useSWR(
    "/api/v1/administrator/branch",
    fetcher,
    {
      revalidateOnFocus: false,
      dedupingInterval: 60000, // Client cache for 1 minute
      keepPreviousData: true,
    }
  );

  useEffect(() => {
    // Real-time updates from SignalR
    const handleBranchNotification = (notification: any) => {
      const message = notification.message.toLowerCase();

      if (message.includes("branch")) {
        console.log("Branch data changed, refreshing...");
        mutate(); // Revalidate SWR cache
      }
    };

    signalRService.on("notification", handleBranchNotification);

    return () => {
      signalRService.off("notification", handleBranchNotification);
    };
  }, [mutate]);

  return {
    branches: data || [],
    isLoading,
    error,
    refresh: mutate,
  };
}
```

#### 3. Component Usage

**File**: `app/branches/page.tsx`

```typescript
"use client";

import { useBranches } from "@/hooks/useBranches";
import { branchAPI } from "@/lib/api-client";
import { toast } from "sonner";

export default function BranchesPage() {
  const { branches, isLoading, refresh } = useBranches();

  const handleCreateBranch = async (data: CreateBranchDto) => {
    try {
      const response = await branchAPI.create(data);
      toast.success("Branch created successfully");
      refresh(); // Manually refresh if needed
      // Note: SignalR notification will also trigger auto-refresh
    } catch (error) {
      toast.error("Failed to create branch");
    }
  };

  const handleUpdateBranch = async (id: string, data: UpdateBranchDto) => {
    try {
      await branchAPI.update(data);
      toast.success("Branch updated successfully");
      // SignalR will auto-refresh the list
    } catch (error) {
      toast.error("Failed to update branch");
    }
  };

  if (isLoading) return <div>Loading...</div>;

  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold mb-4">Branches</h1>

      <div className="grid gap-4">
        {branches.map((branch) => (
          <BranchCard
            key={branch.id}
            branch={branch}
            onUpdate={(data) => handleUpdateBranch(branch.id, data)}
          />
        ))}
      </div>

      <CreateBranchDialog onCreate={handleCreateBranch} />
    </div>
  );
}
```

---

## MAUI Complete Example

### 1. Service with Caching

**File**: `Services/BranchService.cs`

```csharp
public class BranchService
{
    private readonly IApiClient _api;
    private readonly Dictionary<Guid, (BranchDto Branch, DateTime Expiry)> _cache = new();
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(15);

    public async Task<BranchDto> GetBranchAsync(Guid branchId)
    {
        // Check cache
        if (_cache.TryGetValue(branchId, out var cached))
        {
            if (cached.Expiry > DateTime.UtcNow)
            {
                return cached.Branch;
            }
            _cache.Remove(branchId);
        }

        // Fetch from API
        var branch = await _api.GetAsync<BranchDto>($"/api/v1/administrator/branch/{branchId}");

        // Cache it
        _cache[branchId] = (branch, DateTime.UtcNow.Add(_cacheDuration));

        return branch;
    }

    public async Task UpdateBranchAsync(Guid branchId, UpdateBranchDto dto)
    {
        await _api.PutAsync($"/api/v1/administrator/branch", dto);

        // Invalidate cache
        _cache.Remove(branchId);
    }

    public void InvalidateBranch(Guid branchId)
    {
        _cache.Remove(branchId);
    }

    public void ClearAllCache()
    {
        _cache.Clear();
    }
}
```

### 2. ViewModel with Real-Time Updates

**File**: `ViewModels/BranchListViewModel.cs`

```csharp
public class BranchListViewModel : BaseViewModel
{
    private readonly BranchService _branchService;
    private readonly SignalRService _signalR;

    public ObservableCollection<BranchDto> Branches { get; } = new();

    public BranchListViewModel(BranchService branchService, SignalRService signalR)
    {
        _branchService = branchService;
        _signalR = signalR;

        // Setup SignalR notifications
        _signalR.NotificationReceived += OnNotificationReceived;
    }

    public async Task LoadBranchesAsync()
    {
        IsBusy = true;
        try
        {
            var branches = await _branchService.GetAllBranchesAsync();

            Branches.Clear();
            foreach (var branch in branches)
            {
                Branches.Add(branch);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnNotificationReceived(object? sender, Notification notification)
    {
        var message = notification.Message.ToLower();

        if (message.Contains("branch"))
        {
            // Reload branches on main thread
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await LoadBranchesAsync();
            });

            // Show toast notification
            Application.Current?.MainPage?.DisplayAlert(
                notification.Title,
                notification.Message,
                "OK"
            );
        }
    }

    public override void Dispose()
    {
        _signalR.NotificationReceived -= OnNotificationReceived;
        base.Dispose();
    }
}
```

### 3. MAUI Page

**File**: `Pages/BranchesPage.xaml.cs`

```csharp
public partial class BranchesPage : ContentPage
{
    private readonly BranchListViewModel _viewModel;

    public BranchesPage(BranchListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadBranchesAsync();
    }

    private async void OnBranchTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is BranchDto branch)
        {
            await Navigation.PushAsync(new BranchDetailsPage(branch.Id));
        }
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        _viewModel.ClearCache();
        await _viewModel.LoadBranchesAsync();
    }
}
```

**XAML**:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="YourApp.Pages.BranchesPage"
             Title="Branches">

    <RefreshView Command="{Binding RefreshCommand}" IsRefreshing="{Binding IsBusy}">
        <CollectionView ItemsSource="{Binding Branches}">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <SwipeView>
                        <SwipeView.RightItems>
                            <SwipeItems>
                                <SwipeItem Text="Edit"
                                          BackgroundColor="Blue"
                                          Command="{Binding Source={RelativeSource AncestorType={x:Type local:BranchListViewModel}}, Path=EditCommand}"
                                          CommandParameter="{Binding .}"/>
                            </SwipeItems>
                        </SwipeView.RightItems>

                        <Frame Padding="10" Margin="5">
                            <StackLayout>
                                <Label Text="{Binding Name}" FontSize="18" FontAttributes="Bold"/>
                                <Label Text="{Binding Address}" FontSize="14" TextColor="Gray"/>
                                <Label Text="{Binding ContactNumber}" FontSize="12"/>
                            </StackLayout>
                        </Frame>
                    </SwipeView>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
    </RefreshView>
</ContentPage>
```

---

## Performance Comparison

### Without Caching

```
Request 1: Database query (150ms)
Request 2: Database query (150ms)
Request 3: Database query (150ms)
Request 4: Database query (150ms)
Request 5: Database query (150ms)
────────────────────────────────
Total: 750ms for 5 requests
Database load: 5 queries
```

### With Caching

```
Request 1: Database query (150ms) + Cache set (5ms)
Request 2: Cache hit (2ms)
Request 3: Cache hit (2ms)
Request 4: Cache hit (2ms)
Request 5: Cache hit (2ms)
────────────────────────────────
Total: 163ms for 5 requests
Database load: 1 query
Speedup: 4.6x faster
```

---

## Testing

### Unit Test with Mocked Cache

```csharp
[Test]
public async Task GetUserById_CacheHit_DoesNotQueryDatabase()
{
    // Arrange
    var userId = Guid.NewGuid();
    var cachedUser = new UserDto { Id = userId, FullName = "Test User" };

    _mockCache
        .Setup(c => c.GetAsync<UserDto>(It.IsAny<string>(), default))
        .ReturnsAsync(cachedUser);

    // Act
    var result = await _userService.GetUserByIdAsync(userId);

    // Assert
    result.Should().BeEquivalentTo(cachedUser);
    _mockContext.Verify(c => c.Users, Times.Never); // Database NOT accessed
}

[Test]
public async Task UpdateUser_InvalidatesCache()
{
    // Arrange
    var dto = new UpdateUserDto { Id = Guid.NewGuid() };

    // Act
    await _userService.UpdateUserAsync(dto);

    // Assert
    _mockCache.Verify(
        c => c.RemoveAsync(It.IsAny<string>(), default),
        Times.AtLeastOnce
    );
}
```

---

## Quick Reference

### Cache Expiration Guidelines

| Data Type         | Expiration | Reason             |
| ----------------- | ---------- | ------------------ |
| User permissions  | 15 min     | Security-sensitive |
| User profiles     | 15-30 min  | Moderate changes   |
| Branch/Room lists | 30-60 min  | Infrequent changes |
| Tenant config     | 1 hour     | Rarely changes     |
| Plans/Modules     | 24 hours   | Almost static      |
| Dashboard stats   | 5 min      | Needs freshness    |

### Notification Guidelines

| Event                | Send To         | Priority |
| -------------------- | --------------- | -------- |
| User created/updated | Administrators  | Medium   |
| Reservation created  | Receptionists   | High     |
| Room status changed  | Branch users    | Medium   |
| System maintenance   | All users       | High     |
| Report generated     | Requesting user | Low      |

---

## Checklist for Adding Caching to a Service

- [ ] Inject `ICacheService` in constructor
- [ ] Inject `INotificationService` in constructor
- [ ] Inject `ITenantContext` in constructor
- [ ] Add cache key generation (use `CacheKeys` helper)
- [ ] Wrap GET methods with `GetOrCreateAsync`
- [ ] Invalidate cache in CREATE/UPDATE/DELETE
- [ ] Send notifications for important changes
- [ ] Set appropriate expiration times
- [ ] Add logging for cache hits/misses
- [ ] Test with and without cache

---

## Additional Resources

- [Redis Caching Best Practices](https://redis.io/docs/manual/patterns/)
- [SignalR Documentation](https://docs.microsoft.com/aspnet/core/signalr)
- [SWR (Next.js)](https://swr.vercel.app/)
- [MAUI Data Binding](https://docs.microsoft.com/dotnet/maui/fundamentals/data-binding/)
