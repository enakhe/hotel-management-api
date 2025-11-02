using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HotelManagement.Infrastructure.Services.Reports;

/// <summary>
/// Service for managing report templates
/// </summary>
public class ReportTemplateService : IReportTemplateService
{
    private readonly IApplicationDbContext _context;
    private readonly ISuperAdminContext _superAdminContext;
    private readonly ISuperAdminAuditService _auditService;
    private readonly ILogger<ReportTemplateService> _logger;

    public ReportTemplateService(
        IApplicationDbContext context,
        ISuperAdminContext superAdminContext,
        ISuperAdminAuditService auditService,
        ILogger<ReportTemplateService> logger)
    {
        _context = context;
        _superAdminContext = superAdminContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<ReportTemplateDto>> CreateTemplateAsync(CreateReportTemplateDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating report template: {Name}", request.Name);

            var template = new ReportTemplate
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                ReportType = request.ReportType,
                TemplateConfig = request.TemplateConfig != null ? JsonSerializer.Serialize(request.TemplateConfig) : null,
                CustomFields = request.CustomFields != null ? JsonSerializer.Serialize(request.CustomFields) : null,
                IsPublic = request.IsPublic,
                IsActive = true,
                CreatedBy = _superAdminContext.SuperAdminId ?? Guid.Empty,
                CreatedAt = DateTime.UtcNow,
                UsageCount = 0
            };

            _context.ReportTemplates.Add(template);
            await _context.SaveChangesAsync(cancellationToken);

            await _auditService.LogActionAsync(
                "CreateReportTemplate",
                "ReportTemplate",
                template.Id.ToString(),
                null,
                $"Created template '{template.Name}' for report type {template.ReportType}");

            _logger.LogInformation("Report template created: {TemplateId}", template.Id);

            return Result<ReportTemplateDto>.Success(MapToDto(template), 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating report template");
            return Result<ReportTemplateDto>.Failure($"Error creating template: {ex.Message}", 500);
        }
    }

    public async Task<Result<ReportTemplateDto>> UpdateTemplateAsync(Guid templateId, UpdateReportTemplateDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _context.ReportTemplates.FindAsync(new object[] { templateId }, cancellationToken);

            if (template == null)
            {
                return Result<ReportTemplateDto>.Failure("Template not found", 404);
            }

            _logger.LogInformation("Updating report template: {TemplateId}", templateId);

            // Update fields if provided
            if (!string.IsNullOrEmpty(request.Name))
                template.Name = request.Name;

            if (request.Description != null)
                template.Description = request.Description;

            if (request.TemplateConfig != null)
                template.TemplateConfig = JsonSerializer.Serialize(request.TemplateConfig);

            if (request.CustomFields != null)
                template.CustomFields = JsonSerializer.Serialize(request.CustomFields);

            if (request.IsPublic.HasValue)
                template.IsPublic = request.IsPublic.Value;

            if (request.IsActive.HasValue)
                template.IsActive = request.IsActive.Value;

            template.UpdatedAt = DateTime.UtcNow;
            template.UpdatedBy = _superAdminContext.SuperAdminId;

            await _context.SaveChangesAsync(cancellationToken);

            await _auditService.LogActionAsync(
                "UpdateReportTemplate",
                "ReportTemplate",
                templateId.ToString(),
                null,
                $"Updated template '{template.Name}'");

            _logger.LogInformation("Report template updated: {TemplateId}", templateId);

            return Result<ReportTemplateDto>.Success(MapToDto(template), 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating report template: {TemplateId}", templateId);
            return Result<ReportTemplateDto>.Failure($"Error updating template: {ex.Message}", 500);
        }
    }

    public async Task<Result<bool>> DeleteTemplateAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _context.ReportTemplates.FindAsync(new object[] { templateId }, cancellationToken);

            if (template == null)
            {
                return Result<bool>.Failure("Template not found", 404);
            }

            _logger.LogInformation("Deleting report template: {TemplateId}", templateId);

            _context.ReportTemplates.Remove(template);
            await _context.SaveChangesAsync(cancellationToken);

            await _auditService.LogActionAsync(
                "DeleteReportTemplate",
                "ReportTemplate",
                templateId.ToString(),
                null,
                $"Deleted template '{template.Name}'");

            _logger.LogInformation("Report template deleted: {TemplateId}", templateId);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting report template: {TemplateId}", templateId);
            return Result<bool>.Failure($"Error deleting template: {ex.Message}", 500);
        }
    }

    public async Task<Result<ReportTemplateDto>> GetTemplateByIdAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _context.ReportTemplates.FindAsync(new object[] { templateId }, cancellationToken);

            if (template == null)
            {
                return Result<ReportTemplateDto>.Failure("Template not found", 404);
            }

            return Result<ReportTemplateDto>.Success(MapToDto(template), 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report template: {TemplateId}", templateId);
            return Result<ReportTemplateDto>.Failure($"Error getting template: {ex.Message}", 500);
        }
    }

    public async Task<Result<List<ReportTemplateDto>>> GetTemplatesAsync(ReportType? reportType = null, bool? isPublic = null, bool? isActive = null, Guid? createdBy = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.ReportTemplates.AsQueryable();

            if (reportType.HasValue)
            {
                query = query.Where(t => t.ReportType == reportType.Value);
            }

            if (isPublic.HasValue)
            {
                query = query.Where(t => t.IsPublic == isPublic.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(t => t.IsActive == isActive.Value);
            }

            if (createdBy.HasValue)
            {
                query = query.Where(t => t.CreatedBy == createdBy.Value);
            }

            var templates = await query.OrderByDescending(t => t.CreatedAt).ToListAsync(cancellationToken);

            var dtos = templates.Select(MapToDto).ToList();

            return Result<List<ReportTemplateDto>>.Success(dtos, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report templates");
            return Result<List<ReportTemplateDto>>.Failure($"Error getting templates: {ex.Message}", 500);
        }
    }

    public async Task IncrementUsageCountAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _context.ReportTemplates.FindAsync(new object[] { templateId }, cancellationToken);

            if (template != null)
            {
                template.UsageCount++;
                template.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogDebug("Incremented usage count for template: {TemplateId}", templateId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing usage count for template: {TemplateId}", templateId);
        }
    }

    private ReportTemplateDto MapToDto(ReportTemplate template)
    {
        var templateConfig = !string.IsNullOrEmpty(template.TemplateConfig)
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(template.TemplateConfig)
            : null;

        var customFields = !string.IsNullOrEmpty(template.CustomFields)
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(template.CustomFields)
            : null;

        return new ReportTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            ReportType = template.ReportType,
            TemplateConfig = templateConfig,
            CustomFields = customFields,
            IsPublic = template.IsPublic,
            CreatedBy = template.CreatedBy,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            UsageCount = template.UsageCount,
            IsActive = template.IsActive
        };
    }
}

