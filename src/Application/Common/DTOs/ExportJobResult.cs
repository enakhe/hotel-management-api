namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Export job result
/// </summary>
public record ExportJobResult
{
    public bool Success { get; init; }
    public string? JobId { get; init; }
    public string? ErrorMessage { get; init; }
    public string? DownloadUrl { get; init; }
    public DateTime? EstimatedCompletion { get; init; }
}
