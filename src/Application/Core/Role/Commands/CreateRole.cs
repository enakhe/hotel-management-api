using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Exceptions;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Role.Commands;

public record CreateRoleCommand : IRequest<Result<RoleDto>>
{
    [Required, MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }
}

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description cannot exceed 200 characters.");
    }
}

public class CreateRoleCommandHandler(IRoleService roleService, IMapper mapper) : IRequestHandler<CreateRoleCommand, Result<RoleDto>>
{
    private readonly IRoleService _roleService = roleService;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<RoleDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var createRoleDto = _mapper.Map<CreateRoleDto>(request);

        var result = await _roleService.CreateRoleAsync(createRoleDto);

        return !result.Succeeded ?
            throw new ConflictException(string.Join("; ", result.Errors)) :
            result;
    }
}
