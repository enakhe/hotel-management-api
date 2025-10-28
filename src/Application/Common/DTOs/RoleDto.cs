namespace HotelManagement.Application.Common.DTOs;

public class RoleDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? NormalizedName { get; set; }
    public required string Description { get; set; }
    public List<string> Permissions { get; set; } = [];
}
