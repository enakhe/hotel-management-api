using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace HotelManagement.Infrastructure.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IHttpContextAccessor httpContextAccessor,
    ILogger<SuperAdminService> logger,
    IMapper mapper,
    IConfiguration configuration,
    IAuthorizationService authorizationService,
    ITenantService tenantService,
    IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILogger<SuperAdminService> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IConfiguration _configuration = configuration;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ITenantService _tenantService = tenantService;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory = userClaimsPrincipalFactory;

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto loginRequest)
    {
        try
        {
            var user = _userManager.Users
                .SingleOrDefault(u => u.UserName == loginRequest.Email || u.Email == loginRequest.Email);

            if (user == null || !user.IsActive)
                return Result<AuthResponseDto>.Failure("Email or password is incorrect", 401);

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, false);
            if (!signInResult.Succeeded)
                return Result<AuthResponseDto>.Failure("Email or password is incorrect", 401);

            var token = await GeneratJwtToken(user);
            return Result<AuthResponseDto>.Success(token, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user: {Email}", loginRequest.Email);
            return Result<AuthResponseDto>.Failure("An error occurred during authentication", 500);
        }
    }

    public async Task<Result> RegisterAsync(RegisterUserDto dto)
    {
        try
        {
            if (await IsEmailTakenAsync(dto.Email))
                return Result.Failure("Email is already registered", 400);

            var user = await CreateUserFromDtoAsync(dto);
            var createResult = await _userManager.CreateAsync(user, GenerateInitialPassword());

            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return Result.Failure($"Failed to create user: {errors}", 400);
            }

            if (dto.Roles.Count > 0)
            {
                var roleResult = await AssignRolesToUserAsync(user, dto.Roles);
                if (!roleResult.Succeeded)
                    return roleResult;
            }

            return Result.Success("User registered successfully", 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user: {Email}", dto?.Email);
            return Result.Failure("An error occurred during registration", 500);
        }
    }

    public Task LogoutAsync() => Task.CompletedTask;

    public async Task<Result<TokenResponseDto>> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var userId = ValidateJWTToken(refreshToken, out bool isExpired);
            if (isExpired || userId == null)
                return Result<TokenResponseDto>.Failure("Invalid or expired refresh token", 401);

            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return Result<TokenResponseDto>.Failure("User not found", 404);

            var authResponse = await GeneratJwtToken(user);

            return Result<TokenResponseDto>.Success(MapToTokenResponseDto(authResponse), 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return Result<TokenResponseDto>.Failure("Failed to refresh token", 500);
        }
    }

    public async Task<Result> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(changePasswordDto.UserId);
            if (user == null)
                return Result.Failure("User not found", 404);

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

            return !result.Succeeded
                ? Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)))
                : Result.Success("Password changed successfully", 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", changePasswordDto.UserId);
            throw new Exception("An error occurred while changing the password");
        }
    }

    public async Task<Result<string>> RequestPasswordResetAsync(ResetPasswordRequestDto resetPasswordRequestDto)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordRequestDto.Email);
            if (user == null || !user.IsActive)
                return Result<string>.Failure("User not found or inactve", 404);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            return Result<string>.Success(token, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting password reset for email: {Email}", resetPasswordRequestDto.Email);
            throw new Exception("An error occurred while requesting password reset");
        }
    }

    public async Task<Result> ConfirmPasswordResetAsync(ResetPasswordConfirmDto resetPasswordConfirmDto)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordConfirmDto.Email);
            if (user == null || !user.IsActive)
                return Result.Failure("User not found or inactve", 404);

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordConfirmDto.Token, resetPasswordConfirmDto.Password);

            if (!result.Succeeded)
                return Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

            return Result.Success("Password has been reset successfully", 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming password reset for email: {Email}", resetPasswordConfirmDto.Email);
            throw new Exception("An error occurred while confirming password reset");
        }
    }

    public async Task<bool> IsEmailTakenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user != null;
    }

    private async Task<AuthResponseDto> GeneratJwtToken(ApplicationUser user)
    {
        var authClaims = new List<Claim>
        {
            new(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Email, user.Email!),
            new(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("branchId", user.BranchId.ToString() ?? ""),
            new("preferred_username", user.UserName ?? user.Email!),
            new("username", user.UserName ?? user.Email!),
            new("session_id", Guid.NewGuid().ToString()),
        };

        var userRoles = await _userManager.GetRolesAsync(user);
        authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Add SuperAdmin specific claims
        if (userRoles.Contains("SuperAdmin"))
        {
            authClaims.Add(new Claim("superadmin_id", user.Id.ToString()));
            authClaims.Add(new Claim("mfa_verified", "true"));
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: authClaims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        var refreshToken = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: authClaims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new AuthResponseDto
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            AccessTokenExpiration = token.ValidTo,
            UserId = user.Id.ToString(),
            UserName = user.UserName,
            Email = user.Email,
            RefreshToken = new JwtSecurityTokenHandler().WriteToken(refreshToken),
            RefreshTokenExpiration = refreshToken.ValidTo,
        };
    }

    public string? ValidateJWTToken(string token, out bool isExpired)
    {
        isExpired = false;
        if (string.IsNullOrEmpty(token))
            return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

        try
        {
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Exp)?.Value;
            if (expClaim != null && long.TryParse(expClaim, out var exp))
            {
                var expirationDate = DateTimeOffset.FromUnixTimeSeconds(exp);
                if (expirationDate < DateTimeOffset.UtcNow)
                {
                    isExpired = true;
                    return null;
                }
            }

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            jwtToken = (JwtSecurityToken)validatedToken;

            return jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        }
        catch (SecurityTokenExpiredException)
        {
            isExpired = true;
            return null;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    private async Task<ApplicationUser> CreateUserFromDtoAsync(RegisterUserDto dto)
    {
        var user = _mapper.Map<ApplicationUser>(dto);
        var tenantId = await _tenantService.GetTenantIdByIdentifierAsync(dto.Tenant);

        user.Id = Guid.NewGuid();
        user.UserName = dto.Email;
        user.FullName = $"{dto.FirstName} {dto.LastName}";
        user.TenantId = tenantId;

        return user;
    }

    private async Task<Result> AssignRolesToUserAsync(ApplicationUser user, List<string> roles)
    {
        var roleNames = roles.Select(roleId => roleId.ToString()).ToList();
        var roleResult = await _userManager.AddToRolesAsync(user, roleNames);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            return Result.Failure($"Failed to assign roles: {errors}", 400);
        }

        return Result.Success();
    }

    private async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    private static TokenResponseDto MapToTokenResponseDto(AuthResponseDto authResponse)
    {
        return new TokenResponseDto
        {
            AccessToken = authResponse.AccessToken,
            AccessTokenExpiration = authResponse.AccessTokenExpiration,
            RefreshToken = authResponse.RefreshToken,
            RefreshTokenExpiration = authResponse.RefreshTokenExpiration,
            UserId = authResponse.UserId,
            UserName = authResponse.UserName,
            Email = authResponse.Email,
        };
    }

    private static string GenerateInitialPassword()
    {
        const int length = 12;
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%^&*";
        var allChars = upper + lower + digits + special;

        var passwordChars = new List<char>(length);

        passwordChars.Add(upper[System.Security.Cryptography.RandomNumberGenerator.GetInt32(upper.Length)]);
        passwordChars.Add(digits[System.Security.Cryptography.RandomNumberGenerator.GetInt32(digits.Length)]);
        passwordChars.Add(special[System.Security.Cryptography.RandomNumberGenerator.GetInt32(special.Length)]);

        while (passwordChars.Count < length)
        {
            passwordChars.Add(allChars[System.Security.Cryptography.RandomNumberGenerator.GetInt32(allChars.Length)]);
        }

        for (int i = passwordChars.Count - 1; i > 0; i--)
        {
            int j = System.Security.Cryptography.RandomNumberGenerator.GetInt32(i + 1);
            (passwordChars[i], passwordChars[j]) = (passwordChars[j], passwordChars[i]);
        }

        return new string(passwordChars.ToArray());
    }
}
