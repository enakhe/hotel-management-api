using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Request to terminate a tenant
/// </summary>
public record TerminateTenantRequest([Required] string Reason, DateTime? EffectiveDate = null);
