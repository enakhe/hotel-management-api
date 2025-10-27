using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Request to unlock a tenant
/// </summary>
public record UnlockTenantRequest([Required] string Reason);
