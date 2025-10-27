using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Request to lock a tenant
/// </summary>
public record LockTenantRequest([Required] string Reason);
