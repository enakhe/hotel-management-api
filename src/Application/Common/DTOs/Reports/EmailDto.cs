namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for email
/// </summary>
public record EmailDto
{
    public required string To { get; init; }
    public string? Cc { get; init; }
    public string? Bcc { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public bool IsHtml { get; init; } = true;
    public List<EmailAttachmentDto>? Attachments { get; init; }
}

/// <summary>
/// DTO for email attachment
/// </summary>
public record EmailAttachmentDto
{
    public required string FileName { get; init; }
    public required byte[] Content { get; init; }
    public required string ContentType { get; init; }
}

