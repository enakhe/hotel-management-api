using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs;

public class CreateRoleDto
{
    [Required, MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }
}
