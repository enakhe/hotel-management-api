using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Auth;
using Moq;
using NUnit.Framework;

namespace HotelManagement.Application.UnitTests.Common.Behaviours;
public class RequestLoggerTests
{
    private Mock<IUser> _user = null!;
    private Mock<IAuthService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _user = new Mock<IUser>();
        _identityService = new Mock<IAuthService>();
    }

}
