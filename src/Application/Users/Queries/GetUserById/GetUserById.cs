using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.DTOs.Administrator;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Administrator;

namespace HotelManagement.Application.Users.Queries.GetUserById;

public record GetUserByIdQuery : IRequest<UserDto>
{
    [Required]
    public required Guid Id { get; set; }
}

public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
    }
}

public class GetUserByIdQueryHandler(IUserService userService) : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserService _userService = userService;

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(request.Id);

        return user;
    }
}
