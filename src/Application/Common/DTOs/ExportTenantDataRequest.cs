using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Request to export tenant data
/// </summary>
public record ExportTenantDataRequest([Required] ExportOptions Options);
