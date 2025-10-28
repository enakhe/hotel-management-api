using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services;

/// <summary>
/// Service for managing limits and constraints
/// </summary>
public class LimitsService(
    ApplicationDbContext context,
    ILogger<LimitsService> logger) : ILimitsService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<LimitsService> _logger = logger;

    public async Task<Result<LimitsResponseDto>> CreateLimitsAsync(CreateLimitsRequest request)
    {
        try
        {
            var limits = new Domain.Entities.Limits
            {
                Name = request.Name,
                Description = request.Description,
                MaxUsers = request.MaxUsers,
                MaxBranches = request.MaxBranches,
                MaxRooms = request.MaxRooms,
                MaxReservations = request.MaxReservations,
                MaxStorageGB = request.MaxStorageGB,
                CustomLimits = System.Text.Json.JsonSerializer.Serialize(request.CustomLimits),
                CreatedBy = "System"
            };

            _context.Limits.Add(limits);
            await _context.SaveChangesAsync();

            var response = new LimitsResponseDto
            {
                Id = limits.Id,
                Name = limits.Name,
                Description = limits.Description,
                MaxUsers = limits.MaxUsers,
                MaxBranches = limits.MaxBranches,
                MaxRooms = limits.MaxRooms,
                MaxReservations = limits.MaxReservations,
                MaxStorageGB = limits.MaxStorageGB,
                CustomLimits = request.CustomLimits,
                CreatedBy = limits.CreatedBy,
                LastModifiedBy = limits.LastModifiedBy
            };

            return Result<LimitsResponseDto>.Success(response, 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating limits");
            return Result<LimitsResponseDto>.Failure("An error occurred while creating limits", 500);
        }
    }

    public async Task<Result<PaginatedResult<LimitsResponseDto>>> GetLimitsAsync(LimitsListRequest request)
    {
        try
        {
            var query = _context.Limits.AsNoTracking();

            if (!string.IsNullOrEmpty(request.Query))
            {
                query = query.Where(l => l.Name.Contains(request.Query) ||
                                       (l.Description != null && l.Description.Contains(request.Query)));
            }

            var totalCount = await query.CountAsync();
            var limits = await query
                .OrderBy(l => l.Name)
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .ToListAsync();

            var response = limits.Select(l => new LimitsResponseDto
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                MaxUsers = l.MaxUsers,
                MaxBranches = l.MaxBranches,
                MaxRooms = l.MaxRooms,
                MaxReservations = l.MaxReservations,
                MaxStorageGB = l.MaxStorageGB,
                CustomLimits = string.IsNullOrEmpty(l.CustomLimits) ? null :
                    System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(l.CustomLimits),
                CreatedBy = l.CreatedBy,
                LastModifiedBy = l.LastModifiedBy
            }).ToList();

            var paginatedResult = new PaginatedResult<LimitsResponseDto>
            {
                Items = response,
                TotalCount = totalCount,
                Page = request.Page,
                Size = request.Size
            };

            return Result<PaginatedResult<LimitsResponseDto>>.Success(paginatedResult, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting limits");
            return Result<PaginatedResult<LimitsResponseDto>>.Failure("An error occurred while getting limits", 500);
        }
    }

    public async Task<Result<LimitsResponseDto>> GetLimitsByIdAsync(Guid limitsId)
    {
        try
        {
            var limits = await _context.Limits
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == limitsId);

            if (limits == null)
                return Result<LimitsResponseDto>.Failure($"Limits with ID '{limitsId}' not found", 404);

            var response = new LimitsResponseDto
            {
                Id = limits.Id,
                Name = limits.Name,
                Description = limits.Description,
                MaxUsers = limits.MaxUsers,
                MaxBranches = limits.MaxBranches,
                MaxRooms = limits.MaxRooms,
                MaxReservations = limits.MaxReservations,
                MaxStorageGB = limits.MaxStorageGB,
                CustomLimits = string.IsNullOrEmpty(limits.CustomLimits) ? null :
                    System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(limits.CustomLimits),
                CreatedBy = limits.CreatedBy,
                LastModifiedBy = limits.LastModifiedBy
            };

            return Result<LimitsResponseDto>.Success(response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting limits by ID");
            return Result<LimitsResponseDto>.Failure("An error occurred while getting limits", 500);
        }
    }

    public async Task<Result<LimitsResponseDto>> UpdateLimitsAsync(Guid limitsId, UpdateLimitsRequest request)
    {
        try
        {
            var limits = await _context.Limits.FirstOrDefaultAsync(l => l.Id == limitsId);
            if (limits == null)
                return Result<LimitsResponseDto>.Failure($"Limits with ID '{limitsId}' not found", 404);

            limits.Name = request.Name ?? limits.Name;
            limits.Description = request.Description ?? limits.Description;
            limits.MaxUsers = request.MaxUsers ?? limits.MaxUsers;
            limits.MaxBranches = request.MaxBranches ?? limits.MaxBranches;
            limits.MaxRooms = request.MaxRooms ?? limits.MaxRooms;
            limits.MaxReservations = request.MaxReservations ?? limits.MaxReservations;
            limits.MaxStorageGB = request.MaxStorageGB ?? limits.MaxStorageGB;
            limits.CustomLimits = request.CustomLimits != null ?
                System.Text.Json.JsonSerializer.Serialize(request.CustomLimits) : limits.CustomLimits;
            limits.LastModifiedBy = "System";

            await _context.SaveChangesAsync();

            var response = new LimitsResponseDto
            {
                Id = limits.Id,
                Name = limits.Name,
                Description = limits.Description,
                MaxUsers = limits.MaxUsers,
                MaxBranches = limits.MaxBranches,
                MaxRooms = limits.MaxRooms,
                MaxReservations = limits.MaxReservations,
                MaxStorageGB = limits.MaxStorageGB,
                CustomLimits = request.CustomLimits,
                CreatedBy = limits.CreatedBy,
                LastModifiedBy = limits.LastModifiedBy
            };

            return Result<LimitsResponseDto>.Success(response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating limits");
            return Result<LimitsResponseDto>.Failure("An error occurred while updating limits", 500);
        }
    }

    public async Task<Result<bool>> DeleteLimitsAsync(Guid limitsId)
    {
        try
        {
            var limits = await _context.Limits
                .Include(l => l.Plans)
                .FirstOrDefaultAsync(l => l.Id == limitsId);

            if (limits == null)
                return Result<bool>.Failure($"Limits with ID '{limitsId}' not found", 404);

            if (limits.Plans.Any())
                return Result<bool>.Failure("Cannot delete limits that are assigned to plans", 400);

            _context.Limits.Remove(limits);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting limits");
            return Result<bool>.Failure("An error occurred while deleting limits", 500);
        }
    }

    public Task<Result<LimitsResponseDto>> CreateFromTemplateAsync(Guid templateId, string name)
    {
        try
        {
            // For now, return a simple implementation
            // This would typically load from a template system
            return Task.FromResult(Result<LimitsResponseDto>.Failure("Template functionality not implemented", 501));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating limits from template");
            return Task.FromResult(Result<LimitsResponseDto>.Failure("An error occurred while creating limits from template", 500));
        }
    }

    public Task<Result<LimitsTemplateDto[]>> GetLimitsTemplatesAsync()
    {
        try
        {
            // For now, return empty array
            // This would typically load from a template system
            return Task.FromResult(Result<LimitsTemplateDto[]>.Success([], 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting limits templates");
            return Task.FromResult(Result<LimitsTemplateDto[]>.Failure("An error occurred while getting limits templates", 500));
        }
    }

    public Task<Result<LimitsResponseDto>> CreateTemplateAsync(CreateLimitsTemplateRequest request)
    {
        try
        {
            // For now, return not implemented
            return Task.FromResult(Result<LimitsResponseDto>.Failure("Template functionality not implemented", 501));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating limits template");
            return Task.FromResult(Result<LimitsResponseDto>.Failure("An error occurred while creating limits template", 500));
        }
    }

    public async Task<Result<LimitsUsageDto[]>> GetLimitsUsageAsync()
    {
        try
        {
            var usage = await _context.Limits
                .AsNoTracking()
                .Include(l => l.Plans)
                .ThenInclude(p => p.Tenants)
                .Select(l => new LimitsUsageDto
                {
                    LimitsId = l.Id,
                    LimitsName = l.Name,
                    PlanCount = l.Plans.Count,
                    TenantCount = l.Plans.SelectMany(p => p.Tenants).Count(),
                    LicenseCount = l.Plans.SelectMany(p => p.Licenses).Count(),
                    LastUpdated = DateTime.UtcNow
                })
                .ToListAsync();

            return Result<LimitsUsageDto[]>.Success(usage.ToArray(), 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting limits usage");
            return Result<LimitsUsageDto[]>.Failure("An error occurred while getting limits usage", 500);
        }
    }
}
