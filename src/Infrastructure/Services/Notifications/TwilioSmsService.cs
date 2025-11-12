using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace HotelManagement.Infrastructure.Services.Notifications;

/// <summary>
/// SMS service implementation using Twilio
/// </summary>
public class TwilioSmsService : ISmsService
{
    private readonly ILogger<TwilioSmsService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IUsageTrackingService _usageTrackingService;
    private readonly ITenantContext _tenantContext;
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromPhoneNumber;

    public TwilioSmsService(
        ILogger<TwilioSmsService> logger,
        IConfiguration configuration,
        IUsageTrackingService usageTrackingService,
        ITenantContext tenantContext)
    {
        _logger = logger;
        _configuration = configuration;
        _usageTrackingService = usageTrackingService;
        _tenantContext = tenantContext;

        // Load Twilio configuration
        _accountSid = _configuration["Twilio:AccountSid"] ?? throw new InvalidOperationException("Twilio AccountSid not configured");
        _authToken = _configuration["Twilio:AuthToken"] ?? throw new InvalidOperationException("Twilio AuthToken not configured");
        _fromPhoneNumber = _configuration["Twilio:PhoneNumber"] ?? throw new InvalidOperationException("Twilio PhoneNumber not configured");

        // Initialize Twilio client
        TwilioClient.Init(_accountSid, _authToken);
    }

    public async Task<Result<SmsResponseDto>> SendSmsAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsValidPhoneNumber(phoneNumber))
                return Result<SmsResponseDto>.Failure("Invalid phone number format", 400);

            if (string.IsNullOrWhiteSpace(message))
                return Result<SmsResponseDto>.Failure("Message cannot be empty", 400);

            if (message.Length > 1600) // Twilio limit for long messages
                return Result<SmsResponseDto>.Failure("Message too long (max 1600 characters)", 400);

            var messageOptions = new CreateMessageOptions(new PhoneNumber(phoneNumber))
            {
                From = new PhoneNumber(_fromPhoneNumber),
                Body = message
            };

            var smsMessage = await MessageResource.CreateAsync(messageOptions);

            _logger.LogInformation("SMS sent to {PhoneNumber}. Message SID: {Sid}",
                phoneNumber, smsMessage.Sid);

            // Record usage
            if (_tenantContext.IsResolved && _tenantContext.TenantId.HasValue)
            {
                await _usageTrackingService.RecordUsageAsync(
                    _tenantContext.TenantId.Value,
                    UsageMetric.SmsSent,
                    1,
                    cancellationToken);
            }

            var response = new SmsResponseDto
            {
                MessageId = smsMessage.Sid,
                PhoneNumber = phoneNumber,
                Message = message,
                Status = smsMessage.Status.ToString(),
                SentAt = DateTime.UtcNow,
                Cost = null // Twilio price is in string format, parse if needed
            };

            return Result<SmsResponseDto>.Success(response, 200);
        }
        catch (Twilio.Exceptions.ApiException ex)
        {
            _logger.LogError(ex, "Twilio API error sending SMS to {PhoneNumber}", phoneNumber);
            return Result<SmsResponseDto>.Failure($"SMS sending failed: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
            return Result<SmsResponseDto>.Failure("An error occurred while sending SMS", 500);
        }
    }

    public async Task<Result<List<SmsResponseDto>>> SendBulkSmsAsync(
        List<string> phoneNumbers,
        string message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var results = new List<SmsResponseDto>();

            foreach (var phoneNumber in phoneNumbers)
            {
                var result = await SendSmsAsync(phoneNumber, message, cancellationToken);

                if (result.Succeeded && result.Data != null)
                {
                    results.Add(result.Data);
                }
                else
                {
                    _logger.LogWarning("Failed to send SMS to {PhoneNumber}: {Error}",
                        phoneNumber, string.Join(", ", result.Errors));

                    // Add failed record
                    results.Add(new SmsResponseDto
                    {
                        PhoneNumber = phoneNumber,
                        Message = message,
                        Status = "Failed",
                        SentAt = DateTime.UtcNow,
                        ErrorMessage = string.Join(", ", result.Errors)
                    });
                }

                // Small delay to avoid rate limiting
                await Task.Delay(100, cancellationToken);
            }

            var successCount = results.Count(r => r.ErrorMessage == null);
            _logger.LogInformation("Bulk SMS completed: {SuccessCount}/{TotalCount} successful",
                successCount, results.Count);

            return Result<List<SmsResponseDto>>.Success(results, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk SMS");
            return Result<List<SmsResponseDto>>.Failure("An error occurred while sending bulk SMS", 500);
        }
    }

    public async Task<Result<SmsDeliveryStatusDto>> GetDeliveryStatusAsync(
        string messageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await MessageResource.FetchAsync(messageId);

            var status = new SmsDeliveryStatusDto
            {
                MessageId = message.Sid,
                Status = message.Status.ToString(),
                DeliveredAt = message.DateSent,
                ErrorCode = message.ErrorCode?.ToString(),
                ErrorMessage = message.ErrorMessage
            };

            return Result<SmsDeliveryStatusDto>.Success(status, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SMS delivery status for {MessageId}", messageId);
            return Result<SmsDeliveryStatusDto>.Failure("An error occurred while getting delivery status", 500);
        }
    }

    public Task<Result<SmsResponseDto>> SendTemplateSmsAsync(
        string phoneNumber,
        string templateKey,
        Dictionary<string, string> variables,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: Implement template loading and variable substitution
            // For now, this is a placeholder
            _logger.LogWarning("SMS template sending not yet fully implemented. Template: {TemplateKey}", templateKey);

            return Task.FromResult(Result<SmsResponseDto>.Failure("SMS template sending not yet implemented", 501));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending template SMS");
            return Task.FromResult(Result<SmsResponseDto>.Failure("An error occurred while sending template SMS", 500));
        }
    }

    public bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Remove common formatting characters
        var cleanNumber = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        // Must start with + and contain only digits after that
        if (!cleanNumber.StartsWith("+"))
            return false;

        // Must be between 10 and 15 digits (after +)
        var digitsOnly = cleanNumber.Substring(1);
        return digitsOnly.Length >= 10 && digitsOnly.Length <= 15 && digitsOnly.All(char.IsDigit);
    }
}

