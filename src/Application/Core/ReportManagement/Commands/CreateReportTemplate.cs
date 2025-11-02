using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HotelManagement.Application.Core.ReportManagement.Commands;

public record CreateReportTemplateCommand : IRequest<Result<ReportTemplateDto>>
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public ReportType ReportType { get; init; }
    public Dictionary<string, object>? TemplateConfig { get; init; }
    public Dictionary<string, object>? CustomFields { get; init; }
    public bool IsPublic { get; init; } = false;
}

public class CreateReportTemplateCommandValidator : AbstractValidator<CreateReportTemplateCommand>
{
    public CreateReportTemplateCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Template name is required")
            .MaximumLength(200).WithMessage("Template name must not exceed 200 characters");

        RuleFor(v => v.ReportType)
            .IsInEnum().WithMessage("Invalid report type");
    }
}

public class CreateReportTemplateCommandHandler : IRequestHandler<CreateReportTemplateCommand, Result<ReportTemplateDto>>
{
    private readonly IReportTemplateService _templateService;

    public CreateReportTemplateCommandHandler(IReportTemplateService templateService)
    {
        _templateService = templateService;
    }

    public async Task<Result<ReportTemplateDto>> Handle(CreateReportTemplateCommand request, CancellationToken cancellationToken)
    {
        var createDto = new CreateReportTemplateDto
        {
            Name = request.Name,
            Description = request.Description,
            ReportType = request.ReportType,
            TemplateConfig = request.TemplateConfig,
            CustomFields = request.CustomFields,
            IsPublic = request.IsPublic
        };

        return await _templateService.CreateTemplateAsync(createDto, cancellationToken);
    }
}

