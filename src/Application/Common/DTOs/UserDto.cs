namespace HotelManagement.Application.Common.DTOs.Administrator;
public class UserDto
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public List<string>? Roles { get; set; }
}
