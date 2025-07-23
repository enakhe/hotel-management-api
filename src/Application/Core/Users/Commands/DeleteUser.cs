using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.Interfaces.Administrator;

namespace HotelManagement.Application.Core.Users.Commands;

public record DeleteUserCommand : IRequest
{
    [Required]
    public required Guid Id { get; set; }
}

public class DeleteUserCommandHandler(IUserService userService) : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserService _userService = userService;

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        await _userService.DeleteUserAsync(request.Id);
    }
}
