using HotelManagement.Application.Common.DTOs.Role;
using HotelManagement.Domain.Entities.Data;
using HotelManagement.Infrastructure.Services.Administrator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HotelManagement.Application.UnitTests.Core.Role;
public class CreateRoleTests
{
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly RoleService _roleService;

    public CreateRoleTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();

        _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object,
            new List<IRoleValidator<ApplicationRole>>(),
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<ILogger<RoleManager<ApplicationRole>>>().Object
        );

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<ApplicationUser>>(),
            new List<IUserValidator<ApplicationUser>>(),
            new List<IPasswordValidator<ApplicationUser>>(),
            Mock.Of<ILookupNormalizer>(),
            Mock.Of<IdentityErrorDescriber>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<ApplicationUser>>>()
        );

        _roleService = new RoleService(_roleManagerMock.Object, _userManagerMock.Object);
    }

    [Fact]
    public async Task CreateRole_ShouldSucceed_WhenValidInputProvided()
    {
        // Arrange
        var expectedRoleId = Guid.NewGuid();

        var dto = new CreateRoleDto
        {
            Name = "Manager",
            Description = "Manages hotel operations"
        };

        _roleManagerMock.Setup(rm => rm.RoleExistsAsync(dto.Name))
            .ReturnsAsync(false);

        _roleManagerMock.Setup(rm => rm.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationRole>(r => r.Id = expectedRoleId);

        // Act
        var result = await _roleService.CreateRoleAsync(dto);

        // Assert
        Assert.Equal(expectedRoleId, result);

        _roleManagerMock.Verify(rm => rm.RoleExistsAsync(dto.Name), Times.Once);
        _roleManagerMock.Verify(rm => rm.CreateAsync(It.Is<ApplicationRole>(r =>
            r.Name == dto.Name &&
            r.Description == dto.Description)));

    }
}
