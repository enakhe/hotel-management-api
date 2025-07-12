using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Common.Interfaces.Auth;
public interface IAuthService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequest);

    Task<Guid> RegisterAsync(RegisterUserDto registerUserDto);

    Task LogoutAsync();

    Task<TokenResponseDto> RefreshTokenAsync(string refreshToken);

    Task ChangePasswordAsync(ChangePasswordDto changePasswordDto);

    Task RequestPasswordResetAsync(ResetPasswordRequestDto resetPasswordRequestDto);

    Task ConfirmPasswordResetAsync(ResetPasswordConfirmDto resetPasswordDto);

    Task<bool> IsEmailTakenAsync(string email);

    string? ValidateJWTToken(string token, out bool isExpired);
}
