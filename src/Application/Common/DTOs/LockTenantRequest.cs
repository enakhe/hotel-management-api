using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Request to lock a tenant
/// </summary>
public record LockTenantRequest([Required] string Reason);
