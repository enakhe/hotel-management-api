using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Administrator;

namespace HotelManagement.Application.Users.Commands;

public record DeactivateUserCommand : IRequest
{
    [Required]
    public required Guid Id { get; set; }
}


public class DeactivateUserCommandHandler(IUserService userService) : IRequestHandler<DeactivateUserCommand>
{
    private readonly IUserService _userService = userService;

    public async Task Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        await _userService.DeactivateUserAsync(request.Id);
    }
}
