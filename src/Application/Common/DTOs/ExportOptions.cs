namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Export options
/// </summary>
public record ExportOptions
{
    public Guid TenantId { get; init; }
    public bool IncludeUsers { get; init; } = true;
    public bool IncludeReservations { get; init; } = true;
    public bool IncludeRooms { get; init; } = true;
    public bool IncludeAuditLogs { get; init; } = false;
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string Format { get; init; } = "JSON"; // JSON, CSV, Excel
}
