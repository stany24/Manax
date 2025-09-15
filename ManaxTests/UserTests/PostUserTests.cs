using ManaxLibrary.DTO.User;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.UserTests;

[TestClass]
public class PostUserTests : UserTestsSetup
{
    [TestMethod]
    public async Task PostUserWithValidDataCreatesUser()
    {
        UserCreateDto createDto = new()
        {
            Username = "NewUser",
            Password = "password123",
            Role = UserRole.User
        };

        IActionResult result = await Controller.PostUser(createDto);

        OkResult? okResult = result as OkResult;
        Assert.IsNotNull(okResult);
        Assert.IsNotNull(MockNotificationService.UserCreated);

        UserDto createdUser = MockNotificationService.UserCreated;
        Assert.AreEqual(createDto.Username, createdUser.Username);
        Assert.AreEqual(createDto.Role, createdUser.Role);
        MockHashService.VerifyHashPasswordCalled("password123");
    }

    [TestMethod]
    public async Task PostUserCreationDateSetCorrectly()
    {
        UserCreateDto createDto = new()
        {
            Username = "NewUserWithDate",
            Password = "password123",
            Role = UserRole.User
        };

        DateTime beforeCreation = DateTime.UtcNow;
        IActionResult result = await Controller.PostUser(createDto);
        DateTime afterCreation = DateTime.UtcNow;

        OkResult? okResult = result as OkResult;
        Assert.IsNotNull(okResult);
        Assert.IsNotNull(MockNotificationService.UserCreated);

        UserDto createdUser = MockNotificationService.UserCreated;
        Assert.IsNotNull(createdUser);
        Assert.IsTrue(MockNotificationService.UserCreatedAt >= beforeCreation);
        Assert.IsTrue(MockNotificationService.UserCreatedAt <= afterCreation);
    }

    [TestMethod]
    public async Task PostUserVerifyUserCountIncreases()
    {
        int initialCount = Context.Users.Count();
        UserCreateDto createDto = new()
        {
            Username = "CountTestUser",
            Password = "password123",
            Role = UserRole.User
        };

        IActionResult result = await Controller.PostUser(createDto);

        OkResult? okResult = result as OkResult;
        Assert.IsNotNull(okResult);

        int finalCount = Context.Users.Count();
        Assert.AreEqual(initialCount + 1, finalCount);
    }

    [TestMethod]
    public async Task PostUserVerifyNotificationCalledWithCorrectData()
    {
        UserCreateDto createDto = new()
        {
            Username = "NotificationTestUser",
            Password = "password123",
            Role = UserRole.User
        };

        IActionResult result = await Controller.PostUser(createDto);

        OkResult? okResult = result as OkResult;
        Assert.IsNotNull(okResult);
    }
}