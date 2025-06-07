using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagement.Application.Common.DTOs.Administrator;
public class AuditLogDetailDto
{
    public string? PropertyName { get; set; }
    public string? OriginalValue { get; set; }
    public string? NewValue { get; set; }
}
