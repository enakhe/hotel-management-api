using HotelManagement.Application.Common.Interfaces.SuperAdmin;

namespace HotelManagement.Application.Common.Services;

/// <summary>
/// Scoped service that provides SuperAdmin context for control plane operations
/// </summary>
public class SuperAdminContext : ISuperAdminContext
{
    public Guid? SuperAdminId { get; private set; }
    public string? Username { get; private set; }
    public bool IsSuperAdmin => SuperAdminId.HasValue;
    public bool IsMfaVerified { get; private set; }
    public string? SessionId { get; private set; }

    public void SetSuperAdmin(Guid superAdminId, string username, bool isMfaVerified, string sessionId)
    {
        SuperAdminId = superAdminId;
        Username = username;
        IsMfaVerified = isMfaVerified;
        SessionId = sessionId;
    }
}
