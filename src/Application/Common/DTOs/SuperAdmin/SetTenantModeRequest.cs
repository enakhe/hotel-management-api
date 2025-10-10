using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Request to set tenant mode
/// </summary>
public record SetTenantModeRequest([Required] TenantMode Mode, [Required] string Reason);
