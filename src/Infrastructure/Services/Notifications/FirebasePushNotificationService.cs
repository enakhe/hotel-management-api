using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services.Notifications;

/// <summary>
/// Push notification service implementation using Firebase Cloud Messaging
/// </summary>
public class FirebasePushNotificationService : IPushNotificationService
{
    private readonly ILogger<FirebasePushNotificationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IUsageTrackingService _usageTrackingService;
    private readonly ITenantContext _tenantContext;
    private readonly FirebaseMessaging _messaging;
    private static bool _initialized = false;
    private static readonly object _lock = new object();

    public FirebasePushNotificationService(
        ILogger<FirebasePushNotificationService> logger,
        IConfiguration configuration,
        IUsageTrackingService usageTrackingService,
        ITenantContext tenantContext)
    {
        _logger = logger;
        _configuration = configuration;
        _usageTrackingService = usageTrackingService;
        _tenantContext = tenantContext;

        // Initialize Firebase Admin SDK
        InitializeFirebase();
        _messaging = FirebaseMessaging.DefaultInstance;
    }

    private void InitializeFirebase()
    {
        if (_initialized)
            return;

        lock (_lock)
        {
            if (_initialized)
                return;

            try
            {
                var credentialPath = _configuration["Firebase:CredentialPath"];
                
                if (string.IsNullOrWhiteSpace(credentialPath))
                {
                    _logger.LogWarning("Firebase credentials not configured. Push notifications will not work.");
                    return;
                }

                if (!File.Exists(credentialPath))
                {
                    _logger.LogWarning("Firebase credential file not found at {Path}", credentialPath);
                    return;
                }

                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(credentialPath)
                });

                _initialized = true;
                _logger.LogInformation("Firebase initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase");
            }
        }
    }

    public Task<Result<PushNotificationResponseDto>> SendPushAsync(
        string userId,
        string title,
        string body,
        object? data = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_initialized)
                return Task.FromResult(Result<PushNotificationResponseDto>.Failure("Push notifications not configured", 503));

            // TODO: Implement device token lookup for user
            // For now, this is a placeholder
            _logger.LogWarning("Device token lookup not implemented for user {UserId}", userId);
            
            return Task.FromResult(Result<PushNotificationResponseDto>.Failure("Device token lookup not implemented", 501));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to user {UserId}", userId);
            return Task.FromResult(Result<PushNotificationResponseDto>.Failure("An error occurred while sending push notification", 500));
        }
    }

    public async Task<Result<PushNotificationResponseDto>> SendPushToDeviceAsync(
        string deviceToken,
        string title,
        string body,
        object? data = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_initialized)
                return Result<PushNotificationResponseDto>.Failure("Push notifications not configured", 503);

            var message = new Message
            {
                Token = deviceToken,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data != null 
                    ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
                        System.Text.Json.JsonSerializer.Serialize(data))
                    : null
            };

            var response = await _messaging.SendAsync(message, cancellationToken);

            _logger.LogInformation("Push notification sent to device. Message ID: {MessageId}", response);

            // Record usage
            if (_tenantContext.IsResolved && _tenantContext.TenantId.HasValue)
            {
                await _usageTrackingService.RecordUsageAsync(
                    _tenantContext.TenantId.Value,
                    UsageMetric.PushNotificationsSent,
                    1,
                    cancellationToken);
            }

            var result = new PushNotificationResponseDto
            {
                MessageId = response,
                Title = title,
                Body = body,
                Status = "Sent",
                SentAt = DateTime.UtcNow,
                RecipientsCount = 1
            };

            return Result<PushNotificationResponseDto>.Success(result, 200);
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "Firebase error sending push notification");
            return Result<PushNotificationResponseDto>.Failure($"Push notification failed: {ex.Message}", 500);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification");
            return Result<PushNotificationResponseDto>.Failure("An error occurred while sending push notification", 500);
        }
    }

    public async Task<Result<PushNotificationResponseDto>> SendToTopicAsync(
        string topic,
        string title,
        string body,
        object? data = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_initialized)
                return Result<PushNotificationResponseDto>.Failure("Push notifications not configured", 503);

            var message = new Message
            {
                Topic = topic,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data != null
                    ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
                        System.Text.Json.JsonSerializer.Serialize(data))
                    : null
            };

            var response = await _messaging.SendAsync(message, cancellationToken);

            _logger.LogInformation("Push notification sent to topic {Topic}. Message ID: {MessageId}",
                topic, response);

            var result = new PushNotificationResponseDto
            {
                MessageId = response,
                Title = title,
                Body = body,
                Status = "Sent",
                SentAt = DateTime.UtcNow,
                RecipientsCount = -1 // Unknown for topics
            };

            return Result<PushNotificationResponseDto>.Success(result, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to topic {Topic}", topic);
            return Result<PushNotificationResponseDto>.Failure("An error occurred while sending push notification", 500);
        }
    }

    public Task<Result<bool>> RegisterDeviceAsync(
        string userId,
        string deviceToken,
        DevicePlatform platform,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement device token storage in database
        _logger.LogWarning("Device registration not yet implemented");
        return Task.FromResult(Result<bool>.Failure("Device registration not yet implemented", 501));
    }

    public Task<Result<bool>> UnregisterDeviceAsync(
        string deviceToken,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement device token removal
        _logger.LogWarning("Device unregistration not yet implemented");
        return Task.FromResult(Result<bool>.Failure("Device unregistration not yet implemented", 501));
    }

    public async Task<Result<bool>> SubscribeToTopicAsync(
        string deviceToken,
        string topic,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_initialized)
                return Result<bool>.Failure("Push notifications not configured", 503);

            var response = await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(
                new List<string> { deviceToken },
                topic);

            _logger.LogInformation("Subscribed device to topic {Topic}. Success count: {SuccessCount}",
                topic, response.SuccessCount);

            return Result<bool>.Success(response.SuccessCount > 0, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing device to topic {Topic}", topic);
            return Result<bool>.Failure("An error occurred while subscribing to topic", 500);
        }
    }

    public async Task<Result<bool>> UnsubscribeFromTopicAsync(
        string deviceToken,
        string topic,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_initialized)
                return Result<bool>.Failure("Push notifications not configured", 503);

            var response = await FirebaseMessaging.DefaultInstance.UnsubscribeFromTopicAsync(
                new List<string> { deviceToken },
                topic);

            _logger.LogInformation("Unsubscribed device from topic {Topic}. Success count: {SuccessCount}",
                topic, response.SuccessCount);

            return Result<bool>.Success(response.SuccessCount > 0, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing device from topic {Topic}", topic);
            return Result<bool>.Failure("An error occurred while unsubscribing from topic", 500);
        }
    }
}

