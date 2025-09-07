using System.Net;
using System.Security.Claims;
using ManaxLibrary.DTO.User;
using ManaxServer.Models.Claim;
using ManaxServer.Models.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ManaxTests.UserTests;

[TestClass]
public class PostUserTests : UserTestsSetup
{
    [TestMethod]
    public async Task PostUser_WithValidData_CreatesUser()
    {
        UserCreateDto createDto = new()
        {
            Username = "NewUser",
            Password = "password123",
            Role = UserRole.User
        };

        ActionResult<long> result = await _controller.PostUser(createDto);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        long? userId = result.Value;
        Assert.IsNotNull(userId);

        User? createdUser = await _context.Users.FindAsync(userId);
        Assert.IsNotNull(createdUser);
        Assert.AreEqual(createDto.Username, createdUser.Username);
        Assert.AreEqual(createDto.Role, createdUser.Role);
        _mockHashService.VerifyHashPasswordCalled("password123");
        _mockNotificationService.Verify(x => x.NotifyUserCreatedAsync(It.IsAny<UserDto>()), Times.Once);
    }

    [TestMethod]
    public async Task PostUser_CreationDateSetCorrectly()
    {
        UserCreateDto createDto = new()
        {
            Username = "NewUserWithDate",
            Password = "password123",
            Role = UserRole.User
        };

        DateTime beforeCreation = DateTime.UtcNow;
        ActionResult<long> result = await _controller.PostUser(createDto);
        DateTime afterCreation = DateTime.UtcNow;

        long? userId = result.Value;
        Assert.IsNotNull(userId);

        User? createdUser = await _context.Users.FindAsync(userId);
        Assert.IsNotNull(createdUser);
        Assert.IsTrue(createdUser.Creation >= beforeCreation);
        Assert.IsTrue(createdUser.Creation <= afterCreation);
    }

    [TestMethod]
    public async Task PostUser_VerifyUserCountIncreases()
    {
        int initialCount = _context.Users.Count();
        UserCreateDto createDto = new()
        {
            Username = "CountTestUser",
            Password = "password123",
            Role = UserRole.User
        };

        ActionResult<long> result = await _controller.PostUser(createDto);

        long? userId = result.Value;
        Assert.IsNotNull(userId);

        int finalCount = _context.Users.Count();
        Assert.AreEqual(initialCount + 1, finalCount);
    }
    
    [TestMethod]
    public async Task PostUser_VerifyNotificationCalledWithCorrectData()
    {
        UserCreateDto createDto = new()
        {
            Username = "NotificationTestUser",
            Password = "password123",
            Role = UserRole.User
        };

        ActionResult<long> result = await _controller.PostUser(createDto);

        long? userId = result.Value;
        Assert.IsNotNull(userId);

        _mockNotificationService.Verify(x => x.NotifyUserCreatedAsync(It.Is<UserDto>(dto =>
            dto.Username == "NotificationTestUser" &&
            dto.Role == UserRole.User
        )), Times.Once);
    }
}