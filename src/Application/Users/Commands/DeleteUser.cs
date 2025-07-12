using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Administrator;

namespace HotelManagement.Application.Users.Commands;

public record DeleteUserCommand : IRequest<Guid>
{
    [Required]
    public required Guid Id { get; set; }
}

public class DeleteUserCommandHandler(IUserService userService) : IRequestHandler<DeleteUserCommand, Guid>
{
    private readonly IUserService _userService = userService;

    public async Task<Guid> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var id = await _userService.DeleteUserAsync(request.Id);
        return id;
    }
}
