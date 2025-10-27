using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs.Administrator;
public class CreateUserDto
{
    [Required, MaxLength(50)]
    public required string FirstName { get; set; }

    [Required, MaxLength(50)]
    public required string MiddleName { get; set; }

    [Required, MaxLength(50)]
    public required string LastName { get; set; }

    [Required, EmailAddress]
    public required string Email { get; set; }

    [Required, MinLength(8)]
    public required string Password { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public Guid? BranchId { get; set; }

    public List<Guid> RoleIds { get; set; } = [];
}
