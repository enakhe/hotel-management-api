namespace HotelManagement.Application.Common.DTOs.Administrator;
public class AuditLogDetailDto
{
    public string? PropertyName { get; set; }
    public string? OriginalValue { get; set; }
    public string? NewValue { get; set; }
}
