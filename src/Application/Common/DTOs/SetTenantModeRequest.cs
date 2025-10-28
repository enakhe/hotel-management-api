using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.Interfaces;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Request to set tenant mode
/// </summary>
public record SetTenantModeRequest([Required] TenantMode Mode, [Required] string Reason);
