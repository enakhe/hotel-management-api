using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services;

/// <summary>
/// Email service implementation using MailKit
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration, ILogger<EmailService> _logger)
    {
        _configuration = configuration;
        this._logger = _logger;

        // Load SMTP configuration
        _smtpServer = _configuration["Email:SmtpServer"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        _smtpUsername = _configuration["Email:Username"] ?? "";
        _smtpPassword = _configuration["Email:Password"] ?? "";
        _fromEmail = _configuration["Email:FromEmail"] ?? "noreply@hotelmanagement.com";
        _fromName = _configuration["Email:FromName"] ?? "Hotel Management System";
    }

    public async Task<bool> SendEmailAsync(EmailDto email, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(MailboxAddress.Parse(email.To));

            if (!string.IsNullOrEmpty(email.Cc))
            {
                message.Cc.Add(MailboxAddress.Parse(email.Cc));
            }

            if (!string.IsNullOrEmpty(email.Bcc))
            {
                message.Bcc.Add(MailboxAddress.Parse(email.Bcc));
            }

            message.Subject = email.Subject;

            var bodyBuilder = new BodyBuilder();
            if (email.IsHtml)
            {
                bodyBuilder.HtmlBody = email.Body;
            }
            else
            {
                bodyBuilder.TextBody = email.Body;
            }

            // Add attachments if any
            if (email.Attachments != null && email.Attachments.Count > 0)
            {
                foreach (var attachment in email.Attachments)
                {
                    bodyBuilder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
                }
            }

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(_smtpUsername, _smtpPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully to {To}", email.To);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", email.To);
            return false;
        }
    }

    public async Task<bool> SendReportAsync(string to, string subject, string body, ReportFileDto reportFile, CancellationToken cancellationToken = default)
    {
        var emailDto = new EmailDto
        {
            To = to,
            Subject = subject,
            Body = body,
            IsHtml = true,
            Attachments = new List<EmailAttachmentDto>
            {
                new EmailAttachmentDto
                {
                    FileName = reportFile.FileName,
                    Content = reportFile.FileContent,
                    ContentType = reportFile.MimeType
                }
            }
        };

        return await SendEmailAsync(emailDto, cancellationToken);
    }

    public async Task<bool> SendBulkEmailAsync(List<string> recipients, string subject, string body, ReportFileDto? reportFile = null, CancellationToken cancellationToken = default)
    {
        var successCount = 0;

        foreach (var recipient in recipients)
        {
            try
            {
                EmailDto email;

                if (reportFile != null)
                {
                    email = new EmailDto
                    {
                        To = recipient,
                        Subject = subject,
                        Body = body,
                        IsHtml = true,
                        Attachments = new List<EmailAttachmentDto>
                        {
                            new EmailAttachmentDto
                            {
                                FileName = reportFile.FileName,
                                Content = reportFile.FileContent,
                                ContentType = reportFile.MimeType
                            }
                        }
                    };
                }
                else
                {
                    email = new EmailDto
                    {
                        To = recipient,
                        Subject = subject,
                        Body = body,
                        IsHtml = true
                    };
                }

                var sent = await SendEmailAsync(email, cancellationToken);
                if (sent) successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Recipient} in bulk send", recipient);
            }
        }

        _logger.LogInformation("Bulk email sent: {SuccessCount}/{TotalCount}", successCount, recipients.Count);
        return successCount == recipients.Count;
    }

    public async Task<bool> SendReportNotificationAsync(string to, ReportResponseDto report, string downloadUrl, CancellationToken cancellationToken = default)
    {
        var subject = $"Report Ready: {report.Name}";
        var body = $@"
            <html>
            <body>
                <h2>Your Report is Ready</h2>
                <p>The report <strong>{report.Name}</strong> has been successfully generated.</p>
                <ul>
                    <li><strong>Report Type:</strong> {report.Type}</li>
                    <li><strong>Format:</strong> {report.Format}</li>
                    <li><strong>Generated At:</strong> {report.GeneratedAt:yyyy-MM-dd HH:mm:ss}</li>
                    <li><strong>Records:</strong> {report.RecordCount ?? 0}</li>
                    <li><strong>File Size:</strong> {FormatFileSize(report.FileSize ?? 0)}</li>
                </ul>
                <p><a href=""{downloadUrl}"" style=""background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;"">Download Report</a></p>
                <p><small>This link will expire on {report.ExpiresAt:yyyy-MM-dd HH:mm:ss}</small></p>
            </body>
            </html>";

        var email = new EmailDto
        {
            To = to,
            Subject = subject,
            Body = body,
            IsHtml = true
        };

        return await SendEmailAsync(email, cancellationToken);
    }

    public bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}

