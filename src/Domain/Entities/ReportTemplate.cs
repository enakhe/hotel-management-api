using System.ComponentModel.DataAnnotations;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities;

/// <summary>
/// Represents a custom report template
/// </summary>
public class ReportTemplate : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public ReportType ReportType { get; set; }

    /// <summary>
    /// Template configuration as JSON
    /// </summary>
    public string? TemplateConfig { get; set; }

    /// <summary>
    /// Custom fields definition as JSON
    /// </summary>
    public string? CustomFields { get; set; }

    /// <summary>
    /// Whether this template is available to all SuperAdmins
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// SuperAdmin who created the template
    /// </summary>
    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// SuperAdmin who last updated the template
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Number of times this template has been used
    /// </summary>
    public int UsageCount { get; set; } = 0;

    /// <summary>
    /// Whether the template is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}

