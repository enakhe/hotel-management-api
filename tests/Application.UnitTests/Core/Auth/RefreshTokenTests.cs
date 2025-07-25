using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using HotelManagement.Application.Common.Services.Auth;
using HotelManagement.Domain.Entities.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Shouldly;
using Xunit;

namespace HotelManagement.Application.UnitTests.Core.Auth;
public class RefreshTokenTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<IUserClaimsPrincipalFactory<ApplicationUser>> _claimsPrincipalFactoryMock;

    private readonly AuthService _authService;

    public RefreshTokenTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<ApplicationUser>>(),
            new List<IUserValidator<ApplicationUser>>(),
            new List<IPasswordValidator<ApplicationUser>>(),
            Mock.Of<ILookupNormalizer>(),
            Mock.Of<IdentityErrorDescriber>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<ApplicationUser>>>());

        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<ILogger<SignInManager<ApplicationUser>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<ApplicationUser>>());

        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _mapperMock = new Mock<IMapper>();
        _configurationMock = new Mock<IConfiguration>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _claimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

        // JWT settings mock
        _configurationMock.Setup(c => c["Jwt:Key"]).Returns("ABCd1234!@#$EFGh5678!@#$IJKl890MNOPRBACAPIAfrica!");
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

        _authService = new AuthService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _httpContextAccessorMock.Object,
            _mapperMock.Object,
            _configurationMock.Object,
            _authorizationServiceMock.Object,
            _claimsPrincipalFactoryMock.Object);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnNewAccessToken_WhenTokenIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var email = "user@example.com";

        var user = new ApplicationUser
        {
            Id = Guid.Parse(userId),
            Email = email,
            UserName = "testuser",
            IsActive = true,
            BranchId = Guid.NewGuid()
        };

        var refreshToken = GenerateValidRefreshToken(userId, email);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(["User"]);

        // Act
        var result = await _authService.RefreshTokenAsync(refreshToken);

        // Assert
        result.ShouldNotBeNull();
        result.AccessToken.ShouldNotBeNullOrEmpty();
        result.UserId.ShouldBe(userId);
        result.Email.ShouldBe(email);
    }

    [Fact]
    public async Task RefreshToken_ShouldThrow_WhenTokenIsExpired()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var email = "user@example.com";
        var expiredToken = GenerateExpiredRefreshToken(userId, email);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _authService.RefreshTokenAsync(expiredToken));

        Assert.Equal("Refresh token is expired", exception.Message);
    }

    [Fact]
    public async Task RefreshToken_ShouldThrow_WhenTokenIsInvalid()
    {
        // Arrange
        var invaiToken = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE3NTI1MzcyNDIsInN1YiI6IjY3Njg1YjhlODNlOTJiMDc4MzdjNGQ3MyIsInRvdHAiOmZhbHNlfQ.5aEskLEg4uSCyrS9YrhCti6qeMhrZ2kwT4oyLE5qxARdGcZVn17VMQ0PdnGIKiPz9FxBL3X6j4hu7n2Qu07NgQ";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _authService.RefreshTokenAsync(invaiToken));

        Assert.Equal("Refresh token is expired", exception.Message);
    }

    private string GenerateValidRefreshToken(string userId, string email)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, userId)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configurationMock.Object["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configurationMock.Object["Jwt:Issuer"],
            audience: _configurationMock.Object["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateExpiredRefreshToken(string userId, string email)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, userId)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configurationMock.Object["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configurationMock.Object["Jwt:Issuer"],
            audience: _configurationMock.Object["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(-10), // expired
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
