using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs.Auth;
public class RegisterUserDto
{
    [Required, MaxLength(50)]
    public string? FirstName { get; set; }

    [Required, MaxLength(50)]
    public string? LastName { get; set; }

    [Required, EmailAddress]
    public required string Email { get; set; }

    public Guid BranchId { get; set; }

    public required string Tenant { get; set; }

    public List<Guid> Roles { get; set; } = [];
}
