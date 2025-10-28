using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Request to purge tenant data
/// </summary>
public record PurgeTenantDataRequest([Required] string Reason);
