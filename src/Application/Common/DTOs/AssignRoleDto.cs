namespace HotelManagement.Application.Common.DTOs;

public class AssignRoleDto
{
    public required Guid UserId { get; set; }
    public required string RoleName { get; set; }
}
