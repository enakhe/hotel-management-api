using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Administrator;

namespace HotelManagement.Application.Users.Commands;

public record ActivateUserCommand : IRequest
{
    [Required]
    public required Guid Id { get; set; }
}

public class ActivateUserCommandHandler(IUserService userService) : IRequestHandler<ActivateUserCommand>
{
    private readonly IUserService _userService = userService;

    public async Task Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        await _userService.ActivateUserAsync(request.Id);
    }
}
