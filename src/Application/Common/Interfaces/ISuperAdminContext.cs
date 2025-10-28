namespace HotelManagement.Application.Common.Interfaces;

/// <summary>
/// Provides access to the current SuperAdmin context for control plane operations
/// </summary>
public interface ISuperAdminContext
{
    /// <summary>
    /// The current SuperAdmin user ID
    /// </summary>
    Guid? SuperAdminId { get; }

    /// <summary>
    /// The current SuperAdmin username
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// Whether the current user is a SuperAdmin
    /// </summary>
    bool IsSuperAdmin { get; }

    /// <summary>
    /// Whether MFA is required and completed
    /// </summary>
    bool IsMfaVerified { get; }

    /// <summary>
    /// The current session ID for audit purposes
    /// </summary>
    string? SessionId { get; }

    /// <summary>
    /// Sets the SuperAdmin context for the current request
    /// </summary>
    void SetSuperAdmin(Guid superAdminId, string username, bool isMfaVerified, string sessionId);
}
