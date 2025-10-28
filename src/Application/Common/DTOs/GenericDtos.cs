using HotelManagement.Domain.Common;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Generic create request DTO
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public record CreateRequest<T> where T : BaseEntity
{
    public required T Data { get; init; }
}

/// <summary>
/// Generic update request DTO
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public record UpdateRequest<T> where T : BaseEntity
{
    public Guid Id { get; init; }
    public required T Data { get; init; }
}

/// <summary>
/// Generic paginated request DTO
/// </summary>
public record PaginatedRequest
{
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
    public string? Query { get; init; }
    public string SortBy { get; init; } = "created";
    public bool SortDescending { get; init; } = true;
    public Dictionary<string, object>? Filters { get; init; }
}

/// <summary>
/// Generic response DTO
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public record GenericResponse<T>
{
    public T Data { get; init; } = default!;
    public bool Success { get; init; } = true;
    public string Message { get; init; } = string.Empty;
    public int StatusCode { get; init; } = 200;
    public List<string> Errors { get; init; } = new();
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Generic bulk operation request DTO
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public record BulkOperationRequest<T> where T : BaseEntity
{
    public required Guid[] Ids { get; init; }
    public required T Data { get; init; }
    public string? Reason { get; init; }
}

/// <summary>
/// Generic bulk operation response DTO
/// </summary>
public record BulkOperationResponse
{
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
    public List<string> Errors { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
    public Dictionary<Guid, string> FailedItems { get; init; } = new();
}

/// <summary>
/// Generic analytics request DTO
/// </summary>
public record AnalyticsRequest
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? GroupBy { get; init; } // day, week, month, year
    public Dictionary<string, object>? Filters { get; init; }
}

/// <summary>
/// Generic analytics response DTO
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public record AnalyticsResponse<T>
{
    public T Data { get; init; } = default!;
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
    public string Period { get; init; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Generic export request DTO
/// </summary>
public record ExportRequest
{
    public string Format { get; init; } = "csv"; // csv, json, xlsx
    public Dictionary<string, object>? Filters { get; init; }
    public string[]? Columns { get; init; }
    public string? FileName { get; init; }
}

/// <summary>
/// Generic export response DTO
/// </summary>
public record ExportResponse
{
    public string DownloadUrl { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public string FileName { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public string MimeType { get; init; } = string.Empty;
}
