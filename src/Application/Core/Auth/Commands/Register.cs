using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Application.Common.Interfaces.Auth;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Auth.Commands;

public record RegisterCommand : IRequest<Result>
{
    [Required, MaxLength(50)]
    public required string FirstName { get; set; }

    [Required, MaxLength(50)]
    public required string LastName { get; set; }

    [Required, EmailAddress]
    public required string Email { get; set; }

    public Guid? BranchId { get; set; }

    public required string Tenant { get; set; }

    public List<Guid> Roles { get; set; } = [];
}

public class RegisterCommandHandler(IAuthService authService, IMapper mapper) : IRequestHandler<RegisterCommand, Result>
{
    private readonly IAuthService _authService = authService;
    private readonly IMapper _mapper = mapper;

    public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var registerUserDto = _mapper.Map<RegisterUserDto>(request);

        var registerResponse = await _authService.RegisterAsync(registerUserDto);

        return registerResponse;
    }
}
