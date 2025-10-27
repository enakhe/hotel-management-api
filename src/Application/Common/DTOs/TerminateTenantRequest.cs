using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Request to terminate a tenant
/// </summary>
public record TerminateTenantRequest([Required] string Reason, DateTime? EffectiveDate = null);
