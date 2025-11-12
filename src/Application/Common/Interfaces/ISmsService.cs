using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Common.Interfaces;

/// <summary>
/// Service for sending SMS messages
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Send SMS to a single recipient
    /// </summary>
    Task<Result<SmsResponseDto>> SendSmsAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send SMS to multiple recipients
    /// </summary>
    Task<Result<List<SmsResponseDto>>> SendBulkSmsAsync(
        List<string> phoneNumbers,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get delivery status of an SMS
    /// </summary>
    Task<Result<SmsDeliveryStatusDto>> GetDeliveryStatusAsync(
        string messageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send template-based SMS
    /// </summary>
    Task<Result<SmsResponseDto>> SendTemplateSmsAsync(
        string phoneNumber,
        string templateKey,
        Dictionary<string, string> variables,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate phone number format
    /// </summary>
    bool IsValidPhoneNumber(string phoneNumber);
}

