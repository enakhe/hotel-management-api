using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Application.Common.Services.Auth;
using HotelManagement.Domain.Entities.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;

namespace HotelManagement.Application.UnitTests.Core.Auth;
public class LoginTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<IUserClaimsPrincipalFactory<ApplicationUser>> _claimsPrincipalFactoryMock;

    private readonly AuthService _authService;

    public LoginTests()
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
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "testuser",
            IsActive = true,
            BranchId = Guid.NewGuid()
        };

        var loginDto = new LoginRequestDto
        {
            Email = user.Email,
            Password = "TestPassword123"
        };

        _userManagerMock.Setup(u => u.Users).Returns(new[] { user }.AsQueryable());
        _signInManagerMock.Setup(s => s.CheckPasswordSignInAsync(user, loginDto.Password, false))
                         .ReturnsAsync(SignInResult.Success);
        _userManagerMock.Setup(u => u.GetRolesAsync(user))
                        .ReturnsAsync(["User"]);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        result.ShouldNotBeNull();
        result.AccessToken.ShouldNotBeNullOrEmpty();
        result.RefreshToken.ShouldNotBeNullOrEmpty();
        result.UserId.ShouldBe(user.Id.ToString());
        result.UserName.ShouldBe(user.UserName);
        result.Email.ShouldBe(user.Email);
    }

    [Fact]
    public async Task Login_ShouldThrow_WhenEmailOrPasswordIsWrong()
    {
        // Arrange
        var loginDto = new LoginRequestDto
        {
            Email = "wronguser@example.com",
            Password = "WrongPassword123!"
        };

        // No user with matching email exists in the mocked Users collection
        _userManagerMock.Setup(u => u.Users)
            .Returns(Enumerable.Empty<ApplicationUser>()
            .AsQueryable());

        // Act & Assert
        var exception = await Should.ThrowAsync<UnauthorizedAccessException>(
            async () => await _authService.LoginAsync(loginDto));

        exception.Message.ShouldBe("Invalid credentials.");
    }

    [Fact]
    public async Task Login_ShouldThrow_WhenPasswordIsInvalidButEmailExists()
    {
        // Arrange
        var loginDto = new LoginRequestDto
        {
            Email = "validuser@example.com",
            Password = "WrongPassword123!"
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = loginDto.Email,
            UserName = loginDto.Email,
            IsActive = true,
            BranchId = Guid.NewGuid()
        };

        var users = new List<ApplicationUser> { user }.AsQueryable();

        _userManagerMock.Setup(u => u.Users)
        .Returns(users);

        _signInManagerMock.Setup(s => s.CheckPasswordSignInAsync(user, loginDto.Password, false))
            .ReturnsAsync(SignInResult.Failed);

        // Act & Assert
        var exception = await Should.ThrowAsync<UnauthorizedAccessException>(async () =>
        {
            await _authService.LoginAsync(loginDto);
        });

        exception.Message.ShouldBe("Invalid credentials.");
    }
}
