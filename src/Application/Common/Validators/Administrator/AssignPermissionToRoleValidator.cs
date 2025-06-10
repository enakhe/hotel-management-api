using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManagement.Application.Common.DTOs.Administrator;

namespace HotelManagement.Application.Common.Validators.Administrator;
public class AssignPermissionToRoleValidator : AbstractValidator<AssignPermissionsToRoleDto>
{
    public AssignPermissionToRoleValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.PermissionIds).NotEmpty()
            .WithMessage("At least one permission is requred");
    }
}
