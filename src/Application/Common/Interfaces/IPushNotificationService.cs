using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Common.Interfaces;

/// <summary>
/// Service for sending push notifications
/// </summary>
public interface IPushNotificationService
{
    /// <summary>
    /// Send push notification to a specific user
    /// </summary>
    Task<Result<PushNotificationResponseDto>> SendPushAsync(
        string userId,
        string title,
        string body,
        object? data = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send push notification to a specific device
    /// </summary>
    Task<Result<PushNotificationResponseDto>> SendPushToDeviceAsync(
        string deviceToken,
        string title,
        string body,
        object? data = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send push notification to a topic/channel
    /// </summary>
    Task<Result<PushNotificationResponseDto>> SendToTopicAsync(
        string topic,
        string title,
        string body,
        object? data = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Register a device token for a user
    /// </summary>
    Task<Result<bool>> RegisterDeviceAsync(
        string userId,
        string deviceToken,
        DevicePlatform platform,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregister a device token
    /// </summary>
    Task<Result<bool>> UnregisterDeviceAsync(
        string deviceToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribe device to a topic
    /// </summary>
    Task<Result<bool>> SubscribeToTopicAsync(
        string deviceToken,
        string topic,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unsubscribe device from a topic
    /// </summary>
    Task<Result<bool>> UnsubscribeFromTopicAsync(
        string deviceToken,
        string topic,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Device platform
/// </summary>
public enum DevicePlatform
{
    iOS = 0,
    Android = 1,
    Web = 2
}

