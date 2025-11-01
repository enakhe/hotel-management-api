namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for Tenant Administrator details
/// </summary>
public class TenantAdminDto
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    
    // Tenant information
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public string? TenantIdentifier { get; set; }
    
    // Branch information
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    
    // Role information
    public List<string> Roles { get; set; } = new();
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}

