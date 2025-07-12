using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Administrator;

namespace HotelManagement.Application.Users.Commands;

public record AssignRolesCommand : IRequest
{
    [Required]
    public required Guid Id { get; set; }

    [Required]
    public required List<Guid> RoleIds { get; set; }
}

public class AssignRolesCommandHandler(IUserService userService) : IRequestHandler<AssignRolesCommand>
{
    private readonly IUserService _userService = userService;


    public async Task Handle(AssignRolesCommand request, CancellationToken cancellationToken)
    {
        await _userService.AssignRolesAsync(request.Id, request.RoleIds);
    }
}
