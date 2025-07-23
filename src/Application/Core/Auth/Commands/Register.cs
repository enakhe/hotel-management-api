using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Application.Common.Interfaces.Auth;

namespace HotelManagement.Application.Core.Auth.Commands;

public record RegisterCommand : IRequest<Guid>
{
    [Required, MaxLength(50)]
    public required string FirstName { get; set; }

    [Required, MaxLength(50)]
    public required string MiddleName { get; set; }

    [Required, MaxLength(50)]
    public required string LastName { get; set; }

    [Required, EmailAddress]
    public required string Email { get; set; }

    [Required, MinLength(8)]
    public required string Password { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public Guid? BranchId { get; set; }

    public List<Guid> RoleIds { get; set; } = [];
}

public class RegisterCommandHandler(IAuthService authService, IMapper mapper) : IRequestHandler<RegisterCommand, Guid>
{
    private readonly IAuthService _authService = authService;
    private readonly IMapper _mapper = mapper;

    public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var registerUserDto = _mapper.Map<RegisterUserDto>(request);

        var userId = await _authService.RegisterAsync(registerUserDto);

        return userId;
    }
}
