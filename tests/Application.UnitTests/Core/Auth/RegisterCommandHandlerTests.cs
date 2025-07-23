using AutoMapper;
using FluentAssertions;
using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Application.Common.Interfaces.Auth;
using HotelManagement.Application.Core.Auth.Commands;
using Moq;
using Xunit;

namespace HotelManagement.Application.UnitTests.Core.Auth;
public class RegisterCommandHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _mapperMock = new Mock<IMapper>();
        _handler = new RegisterCommandHandler(_authServiceMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCallRegisterAndReturnUserId()
    {
        // Arrange
        var command = new RegisterCommand
        {
            FirstName = "John",
            MiddleName = "A.",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Test@1234",
            PhoneNumber = "1234567890",
            BranchId = Guid.NewGuid(),
            RoleIds = [Guid.NewGuid()]
        };

        var mappedDto = new RegisterUserDto
        {
            FirstName = command.FirstName,
            MiddleName = command.MiddleName,
            LastName = command.LastName,
            Email = command.Email,
            Password = command.Password,
            PhoneNumber = command.PhoneNumber,
            BranchId = command.BranchId,
            RoleIds = command.RoleIds
        };

        var expectedUserId = Guid.NewGuid();

        _mapperMock.Setup(m => m.Map<RegisterUserDto>(command)).Returns(mappedDto);
        _authServiceMock.Setup(s => s.RegisterAsync(mappedDto)).ReturnsAsync(expectedUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedUserId);
        _mapperMock.Verify(m => m.Map<RegisterUserDto>(command), Times.Once);
        _authServiceMock.Verify(s => s.RegisterAsync(mappedDto), Times.Once);
    }
}
