using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.Interfaces;

/// <summary>
/// Service for managing report templates
/// </summary>
public interface IReportTemplateService
{
    /// <summary>
    /// Create a new template
    /// </summary>
    Task<Result<ReportTemplateDto>> CreateTemplateAsync(CreateReportTemplateDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a template
    /// </summary>
    Task<Result<ReportTemplateDto>> UpdateTemplateAsync(Guid templateId, UpdateReportTemplateDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a template
    /// </summary>
    Task<Result<bool>> DeleteTemplateAsync(Guid templateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get template by ID
    /// </summary>
    Task<Result<ReportTemplateDto>> GetTemplateByIdAsync(Guid templateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get templates with filtering
    /// </summary>
    Task<Result<List<ReportTemplateDto>>> GetTemplatesAsync(ReportType? reportType = null, bool? isPublic = null, bool? isActive = null, Guid? createdBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increment template usage count
    /// </summary>
    Task IncrementUsageCountAsync(Guid templateId, CancellationToken cancellationToken = default);
}

