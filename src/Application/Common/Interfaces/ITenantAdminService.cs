using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Common.Interfaces;

/// <summary>
/// Service for managing Tenant Administrator accounts via SuperAdmin control plane
/// </summary>
public interface ITenantAdminService
{
    /// <summary>
    /// Creates a new Tenant Administrator user
    /// Auto-creates "Headquarters" branch if tenant has no branches
    /// Automatically assigns "Administrator" role
    /// </summary>
    /// <param name="dto">Tenant admin creation details</param>
    /// <returns>Created tenant admin details</returns>
    Task<Result<TenantAdminDto>> CreateTenantAdminAsync(CreateTenantAdminDto dto);

    /// <summary>
    /// Updates a Tenant Administrator's profile information
    /// </summary>
    /// <param name="dto">Update details</param>
    /// <returns>Updated tenant admin details</returns>
    Task<Result<TenantAdminDto>> UpdateTenantAdminAsync(UpdateTenantAdminDto dto);

    /// <summary>
    /// Gets a Tenant Administrator by ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Tenant admin details</returns>
    Task<Result<TenantAdminDto>> GetTenantAdminByIdAsync(Guid userId);

    /// <summary>
    /// Gets all Tenant Administrators with pagination and filtering
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="tenantId">Optional tenant filter</param>
    /// <param name="email">Optional email filter</param>
    /// <param name="isActive">Optional active status filter</param>
    /// <returns>Paginated list of tenant admins</returns>
    Task<Result<PaginatedResult<TenantAdminDto>>> GetTenantAdminsAsync(
        int page = 1,
        int pageSize = 20,
        Guid? tenantId = null,
        string? email = null,
        bool? isActive = null);

    /// <summary>
    /// Gets the Administrator for a specific tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Tenant admin details</returns>
    Task<Result<TenantAdminDto>> GetTenantAdminByTenantIdAsync(Guid tenantId);

    /// <summary>
    /// Resets a Tenant Administrator's password
    /// </summary>
    /// <param name="dto">Password reset details</param>
    /// <returns>Success result</returns>
    Task<Result<bool>> ResetPasswordAsync(ResetTenantAdminPasswordDto dto);

    /// <summary>
    /// Activates a Tenant Administrator account
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Success result</returns>
    Task<Result<bool>> ActivateAsync(Guid userId);

    /// <summary>
    /// Deactivates a Tenant Administrator account
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Success result</returns>
    Task<Result<bool>> DeactivateAsync(Guid userId);

    /// <summary>
    /// Deletes a Tenant Administrator
    /// Validates that tenant still has an admin after deletion
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Success result</returns>
    Task<Result<bool>> DeleteAsync(Guid userId);

    /// <summary>
    /// Validates that a tenant has only one admin (used during creation)
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>True if tenant has no admin yet</returns>
    Task<bool> ValidateOneTenantAdminAsync(Guid tenantId);
}

