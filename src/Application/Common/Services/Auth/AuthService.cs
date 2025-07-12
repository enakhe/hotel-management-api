using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Application.Common.Exceptions;
using HotelManagement.Application.Common.Interfaces.Auth;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace HotelManagement.Application.Common.Services.Auth;
public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IHttpContextAccessor httpContextAccessor,
    IMapper mapper,
    IConfiguration configuration,
    IAuthorizationService authorizationService,
IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IMapper _mapper = mapper;
    private readonly IConfiguration _configuration = configuration;
    private readonly IAuthorizationService _authorizationService = authorizationService;
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

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequest)
    {
        var user = _userManager.Users.SingleOrDefault(u => u.UserName == loginRequest.Email || u.Email == loginRequest.Email);

        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid credentials");

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, false);

        if (!result.Succeeded)
            throw new UnauthorizedAccessException("Invalid credentials");
        
        var token = await GeneratJwtToken(user);

        return token;
    }

    public async Task<Guid> RegisterAsync(RegisterUserDto registerUserDto)
    {
        var user = _mapper.Map<ApplicationUser>(registerUserDto);

        user.UserName = registerUserDto.Email;
        user.EmailConfirmed = false;
        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;

        var result = await _userManager.CreateAsync(user, registerUserDto.Password);

        if (!result.Succeeded)
            throw new ConflictException(string.Join(", ", result.Errors.Select(e => e.Description)));

        if (registerUserDto.RoleIds != null && registerUserDto.RoleIds.Count > 0)
        {
            var roles = await _userManager.GetRolesAsync(user);
            await _userManager.AddToRolesAsync(user, roles);
        }

        return user.Id;
    }

    public Task LogoutAsync() => Task.CompletedTask;

    public async Task<TokenResponseDto> RefreshTokenAsync(string refreshToken)
    {
        bool isExpired;
        var userId = ValidateJWTToken(refreshToken, out isExpired);

        if (isExpired)
            throw new UnauthorizedAccessException("Refresh token is expired");

        if (userId == null) throw new UnauthorizedAccessException("User not authenticated");

        var user = await _userManager.FindByIdAsync(userId) ?? throw new NotFoundException("User not found");

        var token = await GeneratJwtToken(user);

        return new TokenResponseDto
        {
            AccessToken = token.AccessToken,
            AccessTokenExpiration = token.AccessTokenExpiration,
            RefreshToken = token.RefreshToken,
            RefreshTokenExpiration = token.RefreshTokenExpiration
        };
    }

    public async Task ChangePasswordAsync(ChangePasswordDto changePasswordDto)
    {
        var userId = _userManager.GetUserId(_httpContextAccessor.HttpContext?.User!) ?? throw new UnauthorizedAccessException("User not authenticated");

        var user = await _userManager.FindByIdAsync(userId) ?? throw new NotFoundException("User not found");

        var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

        if (!result.Succeeded)
            throw new FluentValidation.ValidationException(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task RequestPasswordResetAsync(ResetPasswordRequestDto resetPasswordRequestDto)
    {
        var user = await _userManager.FindByEmailAsync(resetPasswordRequestDto.Email) ?? throw new NotFoundException("User not found");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("User is not active");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // Send email with token (not implemented here)
    }

    public async Task ConfirmPasswordResetAsync(ResetPasswordConfirmDto resetPasswordConfirmDto)
    {
        var user = await _userManager.FindByEmailAsync(resetPasswordConfirmDto.Email) ?? throw new NotFoundException("User not found");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("User is not active");

        var result = await _userManager.ResetPasswordAsync(user, resetPasswordConfirmDto.Token, resetPasswordConfirmDto.Password);

        if (!result.Succeeded)
            throw new FluentValidation.ValidationException(string.Join(", ", result.Errors.Select(e => e.Description)));
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
        };

        var userRoles = await _userManager.GetRolesAsync(user);
        authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

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
}
