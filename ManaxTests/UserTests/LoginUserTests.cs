using System.Net;
using ManaxLibrary.DTO.User;
using ManaxServer.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.UserTests;

[TestClass]
public class LoginUserTests : UserTestsSetup
{
    [TestMethod]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        User user = _context.Users.First();
        user.PasswordHash = "correctPasswordhashed";
        await _context.SaveChangesAsync();

        UserLoginDto loginDto = new()
        {
            Username = user.Username,
            Password = "correctPassword"
        };

        ActionResult<UserLoginResultDto> result = await _controller.Login(loginDto);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        UserLoginResultDto? loginResult = result.Value;
        Assert.IsNotNull(loginResult);
        Assert.AreEqual($"{user.Id}-jwt-token", loginResult.Token);
        Assert.IsNotNull(loginResult.User);
        Assert.AreEqual(user.Id, loginResult.User.Id);

        _mockHashService.VerifyVerifyPasswordCalled("correctPassword");
    }

    [TestMethod]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        UserLoginDto loginDto = new()
        {
            Username = "testuser",
            Password = "wrongPassword"
        };

        ActionResult<UserLoginResultDto> result = await _controller.Login(loginDto);

        Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));

        LoginAttempt? loginAttempt = _context.LoginAttempts.FirstOrDefault(la => la.Username == "testuser");
        Assert.IsNotNull(loginAttempt);
        Assert.IsFalse(loginAttempt.Success);
        Assert.AreEqual("Login", loginAttempt.Type);
    }

    [TestMethod]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        UserLoginDto loginDto = new()
        {
            Username = "nonexistent",
            Password = "password"
        };

        ActionResult<UserLoginResultDto> result = await _controller.Login(loginDto);

        Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));

        LoginAttempt? loginAttempt = _context.LoginAttempts.FirstOrDefault(la => la.Username == "nonexistent");
        Assert.IsNotNull(loginAttempt);
        Assert.IsFalse(loginAttempt.Success);
    }

    [TestMethod]
    public async Task Login_UpdatesLastLoginTime()
    {
        User user = _context.Users.First();
        user.PasswordHash = "correctPasswordhashed";
        await _context.SaveChangesAsync();

        DateTime originalLastLogin = user.LastLogin;
        UserLoginDto loginDto = new()
        {
            Username = user.Username,
            Password = "correctPassword"
        };

        await Task.Delay(10);

        ActionResult<UserLoginResultDto> result = await _controller.Login(loginDto);

        UserLoginResultDto? loginResult = result.Value;
        Assert.IsNotNull(loginResult);

        User? updatedUser = await _context.Users.FindAsync(user.Id);
        Assert.IsNotNull(updatedUser);
        Assert.IsTrue(updatedUser.LastLogin > originalLastLogin);
    }

    [TestMethod]
    public async Task Login_CreatesSuccessfulLoginAttempt()
    {
        User user = _context.Users.First();
        user.PasswordHash = "correctPasswordhashed";
        await _context.SaveChangesAsync();

        UserLoginDto loginDto = new()
        {
            Username = user.Username,
            Password = "correctPassword"
        };

        ActionResult<UserLoginResultDto> result = await _controller.Login(loginDto);

        UserLoginResultDto? loginResult = result.Value;
        Assert.IsNotNull(loginResult);

        LoginAttempt? loginAttempt = _context.LoginAttempts.FirstOrDefault(la => la.Username == user.Username);
        Assert.IsNotNull(loginAttempt);
        Assert.IsTrue(loginAttempt.Success);
        Assert.AreEqual("Login", loginAttempt.Type);
    }

    [TestMethod]
    public async Task Login_WithEmptyDatabase_RedirectsToClaim()
    {
        _context.Users.RemoveRange(_context.Users);
        await _context.SaveChangesAsync();

        UserLoginDto loginDto = new()
        {
            Username = "testuser",
            Password = "password"
        };

        ActionResult<UserLoginResultDto> result = await _controller.Login(loginDto);

        UserLoginResultDto? loginResult = result.Value;
        Assert.IsNotNull(loginResult);

        User? createdUser = _context.Users.FirstOrDefault(u => u.Username == "testuser");
        Assert.IsNotNull(createdUser);
        Assert.AreEqual($"{createdUser.Id}-jwt-token", loginResult.Token);
        Assert.AreEqual(UserRole.Owner, loginResult.User.Role);
    }

    [TestMethod]
    public async Task Login_VerifyLoginAttemptHasCorrectOrigin()
    {
        User user = _context.Users.First();
        user.PasswordHash = "correctPasswordhashed";
        await _context.SaveChangesAsync();

        UserLoginDto loginDto = new()
        {
            Username = user.Username,
            Password = "correctPassword"
        };

        _controller.ControllerContext.HttpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");

        ActionResult<UserLoginResultDto> result = await _controller.Login(loginDto);

        UserLoginResultDto? loginResult = result.Value;
        Assert.IsNotNull(loginResult);

        LoginAttempt? loginAttempt = _context.LoginAttempts.FirstOrDefault(la => la.Username == user.Username);
        Assert.IsNotNull(loginAttempt);
        Assert.AreEqual("127.0.0.1", loginAttempt.Origin);
    }
}