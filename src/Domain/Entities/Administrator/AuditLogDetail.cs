using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagement.Domain.Entities.Administrator;
public class AuditLogDetail
{
    public Guid Id { get; set; }

    public required Guid AuditLogId { get; set; }
    public required AuditLog AuditLog { get; set; }

    public required string PropertyName { get; set; }
    public required string OriginalValue { get; set; }
    public required string NewValue { get; set; }
}

