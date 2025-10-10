using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Request to export tenant data
/// </summary>
public record ExportTenantDataRequest([Required] ExportOptions Options);
