using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs.Administrator;
public class CreateBranchDto
{
    [Required, MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    [Phone]
    public string? ContactNumber { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? TimeZone { get; set; }

    [MaxLength(10)]
    public string? CurrencyCode { get; set; }
}
