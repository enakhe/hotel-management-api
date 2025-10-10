namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Tenant mode enumeration
/// </summary>
public enum TenantMode
{
    Active = 0,
    ReadOnly = 1,
    Suspended = 2,
    Locked = 3
}
