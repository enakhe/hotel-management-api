using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Common.Interfaces.Auth;
public interface IAuthService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto loginRequest);

    Task<Result> RegisterAsync(RegisterUserDto dto);

    Task LogoutAsync();

    Task<Result<TokenResponseDto>> RefreshTokenAsync(string refreshToken);

    Task<Result> ChangePasswordAsync(ChangePasswordDto changePasswordDto);

    Task<Result<string>> RequestPasswordResetAsync(ResetPasswordRequestDto resetPasswordRequestDto);

    Task<Result> ConfirmPasswordResetAsync(ResetPasswordConfirmDto resetPasswordConfirmDto);

    Task<bool> IsEmailTakenAsync(string email);

    string? ValidateJWTToken(string token, out bool isExpired);
}
