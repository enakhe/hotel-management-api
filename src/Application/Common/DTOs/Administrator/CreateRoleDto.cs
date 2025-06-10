using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs.Administrator;
public class CreateRoleDto
{
    [Required, MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public required string Description { get; set; }
}
