namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for report file
/// </summary>
public record ReportFileDto
{
    public required string FileName { get; init; }
    public required string FilePath { get; init; }
    public required byte[] FileContent { get; init; }
    public required string MimeType { get; init; }
    public long FileSize { get; init; }
}

