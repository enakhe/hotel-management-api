using AutoMapper;
using FluentValidation;
using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace HotelManagement.Infrastructure.Services;

public class BranchService(IHttpContextAccessor httpContextAccessor, IBranchRepository branchRepository, IUserRepository userRepository, IValidator<CreateBranchDto> _branchValidator, IMapper mapper, ICacheService cache, ITenantContext tenantContext) : IBranchService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IBranchRepository _branchRepository = branchRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IValidator<CreateBranchDto> _branchValidator = _branchValidator;
    private readonly IMapper _mapper = mapper;
    private readonly ICacheService _cache = cache;
    private readonly ITenantContext _tenantContext = tenantContext;
    private const string BranchClaimType = "branchId";

    /// <summary>
    /// Invalidates cache for a specific branch and tenant branches list
    /// </summary>
    private async Task InvalidateBranchCacheAsync(Guid branchId)
    {
        await _cache.RemoveAsync(CacheKeys.Branch(branchId));
        
        // Invalidate tenant branches cache if tenant context is resolved
        if (_tenantContext.IsResolved && _tenantContext.TenantId.HasValue)
        {
            await _cache.RemoveAsync(CacheKeys.TenantBranches(_tenantContext.TenantId.Value));
        }
    }

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

        // Invalidate branch caches
        await InvalidateBranchCacheAsync(branch.Id);

        return branch.Id;
    }

    public async Task UpdateBranchAsync(Guid id, CreateBranchDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        await _branchValidator.ValidateAndThrowAsync(dto);

        var branch = await _branchRepository.GetByIdAsync(id) ?? throw new Application.Common.Exceptions.NotFoundException("Branch not found");

        _mapper.Map(dto, branch);

        await _branchRepository.UpdateAsync(branch);

        // Invalidate branch caches
        await InvalidateBranchCacheAsync(id);
    }

    public async Task DeleteBranchAsync(Guid id)
    {
        var branch = await _branchRepository.GetByIdAsync(id) ?? throw new Application.Common.Exceptions.NotFoundException("Branch not found");

        await _branchRepository.DeleteAsync(branch);

        // Invalidate branch caches
        await InvalidateBranchCacheAsync(id);
    }

    public async Task<BranchDto> GetBranchByIdAsync(Guid id)
    {
        // Try to get from cache (30 minutes)
        var cacheKey = CacheKeys.Branch(id);
        var cached = await _cache.GetAsync<BranchDto>(cacheKey);
        
        if (cached != null)
        {
            return cached;
        }

        var branch = await _branchRepository.GetByIdAsync(id);
        if (branch == null) throw new Application.Common.Exceptions.NotFoundException("Branch not found");

        var dto = _mapper.Map<BranchDto>(branch);

        // Cache for 30 minutes
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30));

        return dto;
    }

    public async Task<IEnumerable<BranchDto>> GetAllBranchesAsync()
    {
        // Try to get from cache (30 minutes)
        string cacheKey;
        if (_tenantContext.IsResolved && _tenantContext.TenantId.HasValue)
        {
            cacheKey = CacheKeys.TenantBranches(_tenantContext.TenantId.Value);
        }
        else
        {
            cacheKey = "branches:all";
        }
        
        var cached = await _cache.GetAsync<List<BranchDto>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var branches = await _branchRepository.GetAllAsync();
        var result = _mapper.Map<List<BranchDto>>(branches);

        // Cache for 30 minutes
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));

        return result;
    }

    public async Task<IEnumerable<UserDto>> GetUsersByBranchIdAsync(Guid branchId)
    {
        var users = await _userRepository.GetUsersByBranchAsync(branchId);
        return _mapper.Map<List<UserDto>>(users);
    }
}
