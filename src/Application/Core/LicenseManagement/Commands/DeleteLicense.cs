using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.LicenseManagement.Commands;

public record DeleteLicenseCommand : IRequest<Result<bool>>
{
    public Guid LicenseId { get; init; }
}

public class DeleteLicenseCommandValidator : AbstractValidator<DeleteLicenseCommand>
{
    public DeleteLicenseCommandValidator()
    {
        RuleFor(x => x.LicenseId)
            .NotEmpty()
            .WithMessage("License ID is required.");
    }
}

public class DeleteLicenseCommandHandler(ILicenseService service) : IRequestHandler<DeleteLicenseCommand, Result<bool>>
{
    private readonly ILicenseService _service = service;

    public async Task<Result<bool>> Handle(DeleteLicenseCommand request, CancellationToken cancellationToken)
    {
        return await _service.DeleteLicenseAsync(request.LicenseId);
    }
}
