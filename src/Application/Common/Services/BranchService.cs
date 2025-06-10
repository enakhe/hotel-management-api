using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManagement.Application.Common.DTOs.Administrator;
using HotelManagement.Application.Common.Interfaces.Administrator;
using HotelManagement.Domain.Entities.Configuration;
using Microsoft.AspNetCore.Http;

namespace HotelManagement.Application.Common.Services;
public class BranchService(IHttpContextAccessor httpContextAccessor, IBranchRepository branchRepository, IUserRepository userRepository, IValidator<CreateBranchDto> _branchValidator, IMapper mapper) : IBranchService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IBranchRepository _branchRepository = branchRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IValidator<CreateBranchDto> _branchValidator = _branchValidator;
    private readonly IMapper _mapper = mapper;
    private const string BranchClaimType = "branchId";

    public Guid CurrentBranchId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User
                ?? throw new InvalidOperationException("No HTTP context available.");

            var claim = user.FindFirst(BranchClaimType)
                ?? throw new UnauthorizedAccessException("BranchId claim is missing.");

            return !Guid.TryParse(claim.Value, out var branchId)
                ? throw new UnauthorizedAccessException("Invalid BranchId claim value.")
                : branchId;
        }
    }

    public async Task<Guid> CreateBranchAsync(CreateBranchDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        await _branchValidator.ValidateAndThrowAsync(dto);

        var branch = _mapper.Map<Branch>(dto);

        branch.Id = Guid.NewGuid();
        branch.IsActive = true;

        await _branchRepository.AddAsync(branch);

        return branch.Id;
    }

    public async Task UpdateBranchAsync(Guid id, CreateBranchDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        await _branchValidator.ValidateAndThrowAsync(dto);

        var branch = await _branchRepository.GetByIdAsync(id) ?? throw new Exceptions.NotFoundException("Branch not found");

        _mapper.Map(dto, branch);

        await _branchRepository.UpdateAsync(branch);
    }

    public async Task DeleteBranchAsync(Guid id)
    {
        var branch = await _branchRepository.GetByIdAsync(id) ?? throw new Exceptions.NotFoundException("Branch not found");

        await _branchRepository.DeleteAsync(branch);
    }

    public async Task<BranchDto> GetBranchByIdAsync(Guid id)
    {
        var branch = await _branchRepository.GetByIdAsync(id);

        return branch == null ? throw new Exceptions.NotFoundException("Branch not found") : _mapper.Map<BranchDto>(branch);
    }

    public async Task<IEnumerable<BranchDto>> GetAllBranchesAsync()
    {
        var branches = await _branchRepository.GetAllAsync();
        return _mapper.Map<List<BranchDto>>(branches);
    }

    public async Task<IEnumerable<UserDto>> GetUsersByBranchIdAsync(Guid branchId)
    {
        var users = await _userRepository.GetUsersByBranchAsync(branchId);
        return _mapper.Map<List<UserDto>>(users);
    }
}
