using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services.Reports;

/// <summary>
/// Service for managing report subscriptions
/// </summary>
public class ReportSubscriptionService : IReportSubscriptionService
{
    private readonly IApplicationDbContext _context;
    private readonly ISuperAdminContext _superAdminContext;
    private readonly ISuperAdminAuditService _auditService;
    private readonly ILogger<ReportSubscriptionService> _logger;

    public ReportSubscriptionService(
        IApplicationDbContext context,
        ISuperAdminContext superAdminContext,
        ISuperAdminAuditService auditService,
        ILogger<ReportSubscriptionService> logger)
    {
        _context = context;
        _superAdminContext = superAdminContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<ReportSubscriptionDto>> CreateSubscriptionAsync(CreateReportSubscriptionDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating report subscription for {Email}", request.Email);

            // Check if subscription already exists
            var existing = await _context.ReportSubscriptions
                .FirstOrDefaultAsync(s => s.SuperAdminId == request.SuperAdminId &&
                                         s.ReportScheduleId == request.ReportScheduleId,
                                    cancellationToken);

            if (existing != null)
            {
                return Result<ReportSubscriptionDto>.Failure("Subscription already exists for this user and schedule", 400);
            }

            // Verify schedule exists
            var scheduleExists = await _context.ReportSchedules.AnyAsync(s => s.Id == request.ReportScheduleId, cancellationToken);
            if (!scheduleExists)
            {
                return Result<ReportSubscriptionDto>.Failure("Report schedule not found", 404);
            }

            var subscription = new ReportSubscription
            {
                Id = Guid.NewGuid(),
                SuperAdminId = request.SuperAdminId,
                Email = request.Email,
                ReportScheduleId = request.ReportScheduleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UnsubscribeToken = Guid.NewGuid().ToString(),
                SentCount = 0
            };

            _context.ReportSubscriptions.Add(subscription);
            await _context.SaveChangesAsync(cancellationToken);

            await _auditService.LogActionAsync(
                "CreateReportSubscription",
                "ReportSubscription",
                subscription.Id.ToString(),
                null,
                $"Created subscription for {subscription.Email} to schedule {subscription.ReportScheduleId}");

            _logger.LogInformation("Report subscription created: {SubscriptionId}", subscription.Id);

            return Result<ReportSubscriptionDto>.Success(MapToDto(subscription), 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating report subscription");
            return Result<ReportSubscriptionDto>.Failure($"Error creating subscription: {ex.Message}", 500);
        }
    }

    public async Task<Result<bool>> DeleteSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _context.ReportSubscriptions.FindAsync(new object[] { subscriptionId }, cancellationToken);

            if (subscription == null)
            {
                return Result<bool>.Failure("Subscription not found", 404);
            }

            _logger.LogInformation("Deleting report subscription: {SubscriptionId}", subscriptionId);

            _context.ReportSubscriptions.Remove(subscription);
            await _context.SaveChangesAsync(cancellationToken);

            await _auditService.LogActionAsync(
                "DeleteReportSubscription",
                "ReportSubscription",
                subscription.Id.ToString(),
                null,
                $"Deleted subscription for {subscription.Email}");

            _logger.LogInformation("Report subscription deleted: {SubscriptionId}", subscriptionId);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting report subscription: {SubscriptionId}", subscriptionId);
            return Result<bool>.Failure($"Error deleting subscription: {ex.Message}", 500);
        }
    }

    public async Task<Result<List<ReportSubscriptionDto>>> GetSubscriptionsAsync(Guid? reportScheduleId = null, Guid? superAdminId = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.ReportSubscriptions.AsQueryable();

            if (reportScheduleId.HasValue)
            {
                query = query.Where(s => s.ReportScheduleId == reportScheduleId.Value);
            }

            if (superAdminId.HasValue)
            {
                query = query.Where(s => s.SuperAdminId == superAdminId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(s => s.IsActive == isActive.Value);
            }

            var subscriptions = await query.OrderByDescending(s => s.CreatedAt).ToListAsync(cancellationToken);

            var dtos = subscriptions.Select(MapToDto).ToList();

            return Result<List<ReportSubscriptionDto>>.Success(dtos, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report subscriptions");
            return Result<List<ReportSubscriptionDto>>.Failure($"Error getting subscriptions: {ex.Message}", 500);
        }
    }

    public async Task<Result<ReportSubscriptionDto>> GetSubscriptionByIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _context.ReportSubscriptions.FindAsync(new object[] { subscriptionId }, cancellationToken);

            if (subscription == null)
            {
                return Result<ReportSubscriptionDto>.Failure("Subscription not found", 404);
            }

            return Result<ReportSubscriptionDto>.Success(MapToDto(subscription), 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report subscription: {SubscriptionId}", subscriptionId);
            return Result<ReportSubscriptionDto>.Failure($"Error getting subscription: {ex.Message}", 500);
        }
    }

    public async Task UpdateSentCountAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _context.ReportSubscriptions.FindAsync(new object[] { subscriptionId }, cancellationToken);

            if (subscription != null)
            {
                subscription.SentCount++;
                subscription.LastSentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sent count for subscription: {SubscriptionId}", subscriptionId);
        }
    }

    private ReportSubscriptionDto MapToDto(ReportSubscription subscription)
    {
        return new ReportSubscriptionDto
        {
            Id = subscription.Id,
            SuperAdminId = subscription.SuperAdminId,
            Email = subscription.Email,
            ReportScheduleId = subscription.ReportScheduleId,
            IsActive = subscription.IsActive,
            CreatedAt = subscription.CreatedAt,
            LastSentAt = subscription.LastSentAt,
            SentCount = subscription.SentCount
        };
    }
}

