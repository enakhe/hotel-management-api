namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Limits response DTO
/// </summary>
public record LimitsResponseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    // Core Limits
    public int MaxUsers { get; init; }
    public int MaxBranches { get; init; }
    public int MaxRooms { get; init; }
    public int MaxReservations { get; init; }
    public int MaxStorageGB { get; init; }
    public int ApiRateLimit { get; init; }
    public int ConcurrentSessions { get; init; }

    // Additional Limits
    public int MaxGuests { get; init; }
    public int MaxBookings { get; init; }
    public int MaxReports { get; init; }
    public int MaxIntegrations { get; init; }

    // Custom Limits
    public Dictionary<string, int>? CustomLimits { get; init; }

    // Metadata
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public string? LastModifiedBy { get; init; }
}

/// <summary>
/// Create limits request DTO
/// </summary>
public record CreateLimitsRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    // Core Limits
    public int MaxUsers { get; init; } = -1;
    public int MaxBranches { get; init; } = -1;
    public int MaxRooms { get; init; } = -1;
    public int MaxReservations { get; init; } = -1;
    public int MaxStorageGB { get; init; } = -1;
    public int ApiRateLimit { get; init; } = -1;
    public int ConcurrentSessions { get; init; } = -1;

    // Additional Limits
    public int MaxGuests { get; init; } = -1;
    public int MaxBookings { get; init; } = -1;
    public int MaxReports { get; init; } = -1;
    public int MaxIntegrations { get; init; } = -1;

    // Custom Limits
    public Dictionary<string, int>? CustomLimits { get; init; }

    // Metadata
    public bool IsDefault { get; init; } = false;
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Update limits request DTO
/// </summary>
public record UpdateLimitsRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }

    // Core Limits
    public int? MaxUsers { get; init; }
    public int? MaxBranches { get; init; }
    public int? MaxRooms { get; init; }
    public int? MaxReservations { get; init; }
    public int? MaxStorageGB { get; init; }
    public int? ApiRateLimit { get; init; }
    public int? ConcurrentSessions { get; init; }

    // Additional Limits
    public int? MaxGuests { get; init; }
    public int? MaxBookings { get; init; }
    public int? MaxReports { get; init; }
    public int? MaxIntegrations { get; init; }

    // Custom Limits
    public Dictionary<string, int>? CustomLimits { get; init; }

    // Metadata
    public bool? IsDefault { get; init; }
    public bool? IsActive { get; init; }
}

/// <summary>
/// Limits list request DTO
/// </summary>
public record LimitsListRequest
{
    public string? Query { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsDefault { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
    public string SortBy { get; init; } = "createdAt";
    public bool SortDescending { get; init; } = true;
}

/// <summary>
/// Limits template DTO
/// </summary>
public record LimitsTemplateDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public Dictionary<string, int> TemplateData { get; init; } = new();
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Create limits template request DTO
/// </summary>
public record CreateLimitsTemplateRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public Dictionary<string, int> TemplateData { get; init; } = new();
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Limits usage DTO
/// </summary>
public record LimitsUsageDto
{
    public Guid LimitsId { get; init; }
    public string LimitsName { get; init; } = string.Empty;
    public int PlanCount { get; init; }
    public int TenantCount { get; init; }
    public int LicenseCount { get; init; }
    public DateTime LastUpdated { get; init; }
}
