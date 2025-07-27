using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.DTOs.Role;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Administrator;

namespace HotelManagement.Application.Core.Role.Commands;

public record UpdateRoleCommand : IRequest
{
    [Required]
    public required Guid Id { get; init; }

    [Required, MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }
}

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description cannot exceed 200 characters.");
    }
}

public class UpdateRoleCommandHandler(IRoleService roleService, IMapper mapper) : IRequestHandler<UpdateRoleCommand>
{
    private readonly IRoleService _roleService = roleService;
    private readonly IMapper _mapper = mapper;

    public async Task Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var updateRoleDto = _mapper.Map<CreateRoleDto>(request);

        await _roleService.UpdateRoleAsync(request.Id, updateRoleDto);
    }
}
