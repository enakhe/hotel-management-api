using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Enums;
using HotelManagement.Infrastructure.Services.Reports.Generators;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services.Reports;

/// <summary>
/// Service for generating report data using strategy pattern
/// </summary>
public class ReportGeneratorService : IReportGeneratorService
{
    private readonly Dictionary<ReportType, IReportDataGenerator> _generators;
    private readonly ILogger<ReportGeneratorService> _logger;

    public ReportGeneratorService(
        SystemOverviewReportGenerator systemOverviewGenerator,
        FinancialReportGenerator financialGenerator,
        TenantUsageReportGenerator tenantUsageGenerator,
        AuditReportGenerator auditGenerator,
        AnalyticsReportGenerator analyticsGenerator,
        ILogger<ReportGeneratorService> logger)
    {
        _logger = logger;

        // Register all generators
        _generators = new Dictionary<ReportType, IReportDataGenerator>
        {
            { ReportType.SystemOverview, systemOverviewGenerator },
            { ReportType.Financial, financialGenerator },
            { ReportType.TenantUsage, tenantUsageGenerator },
            { ReportType.Audit, auditGenerator },
            { ReportType.Analytics, analyticsGenerator }
        };
    }

    public async Task<ReportDataDto> GenerateReportDataAsync(ReportType reportType, ReportParametersDto parameters, CancellationToken cancellationToken = default)
    {
        if (!_generators.TryGetValue(reportType, out var generator))
        {
            _logger.LogError("No generator found for report type: {ReportType}", reportType);
            throw new NotSupportedException($"Report type {reportType} is not supported");
        }

        _logger.LogInformation("Generating report data for type: {ReportType}", reportType);

        try
        {
            var reportData = await generator.GenerateAsync(parameters, cancellationToken);
            _logger.LogInformation("Successfully generated report data for type: {ReportType}", reportType);
            return reportData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating report data for type: {ReportType}", reportType);
            throw;
        }
    }

    public async Task<bool> ValidateParametersAsync(ReportType reportType, ReportParametersDto parameters)
    {
        if (!_generators.TryGetValue(reportType, out var generator))
        {
            _logger.LogWarning("No generator found for report type: {ReportType}", reportType);
            return false;
        }

        return await generator.ValidateParametersAsync(parameters);
    }

    public ReportParametersDto GetDefaultParameters(ReportType reportType)
    {
        if (!_generators.TryGetValue(reportType, out var generator))
        {
            _logger.LogWarning("No generator found for report type: {ReportType}, returning empty parameters", reportType);
            return new ReportParametersDto();
        }

        return generator.GetDefaultParameters();
    }
}

