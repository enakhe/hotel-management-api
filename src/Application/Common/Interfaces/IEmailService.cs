using HotelManagement.Application.Common.DTOs;

namespace HotelManagement.Application.Common.Interfaces;

/// <summary>
/// Service for sending emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send email with attachment
    /// </summary>
    Task<bool> SendEmailAsync(EmailDto email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send report via email
    /// </summary>
    Task<bool> SendReportAsync(string to, string subject, string body, ReportFileDto reportFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send bulk emails
    /// </summary>
    Task<bool> SendBulkEmailAsync(List<string> recipients, string subject, string body, ReportFileDto? reportFile = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send report notification
    /// </summary>
    Task<bool> SendReportNotificationAsync(string to, ReportResponseDto report, string downloadUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate email address
    /// </summary>
    bool IsValidEmail(string email);
}

