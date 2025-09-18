using System.Net;
using ManaxLibrary.DTO.User;
using ManaxServer.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.UserTests;

[TestClass]
public class LoginUserTests : UserTestsSetup
{
    [TestMethod]
    public async Task LoginWithValidCredentialsReturnsToken()
    {
        User user = Context.Users.First();
        user.PasswordHash = "correctPasswordhashed";
        await Context.SaveChangesAsync();

        UserLoginDto loginDto = new()
        {
            Username = user.Username,
            Password = "correctPassword"
        };

        ActionResult<UserLoginResultDto> result = await Controller.Login(loginDto);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        UserLoginResultDto? loginResult = result.Value;
        Assert.IsNotNull(loginResult);
        Assert.AreEqual($"{user.Id}-jwt-token", loginResult.Token);
        Assert.IsNotNull(loginResult.User);
        Assert.AreEqual(user.Id, loginResult.User.Id);

        MockHashService.VerifyVerifyPasswordCalled("correctPassword");
    }

    [TestMethod]
    public async Task LoginWithInvalidCredentialsReturnsUnauthorized()
    {
        UserLoginDto loginDto = new()
        {
            Username = "testuser",
            Password = "wrongPassword"
        };

        ActionResult<UserLoginResultDto> result = await Controller.Login(loginDto);

        Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));

        LoginAttempt? loginAttempt = Context.LoginAttempts.FirstOrDefault(la => la.Username == "testuser");
        Assert.IsNotNull(loginAttempt);
        Assert.IsFalse(loginAttempt.Success);
        Assert.AreEqual("Login", loginAttempt.Type);
    }

    [TestMethod]
    public async Task LoginWithNonExistentUserReturnsUnauthorized()
    {
        UserLoginDto loginDto = new()
        {
            Username = "nonexistent",
            Password = "password"
        };

        ActionResult<UserLoginResultDto> result = await Controller.Login(loginDto);

        Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));

        LoginAttempt? loginAttempt = Context.LoginAttempts.FirstOrDefault(la => la.Username == "nonexistent");
        Assert.IsNotNull(loginAttempt);
        Assert.IsFalse(loginAttempt.Success);
    }

    [TestMethod]
    public async Task LoginUpdatesLastLoginTime()
    {
        User user = Context.Users.First();
        user.PasswordHash = "correctPasswordhashed";
        await Context.SaveChangesAsync();

        DateTime originalLastLogin = user.LastLogin;
        UserLoginDto loginDto = new()
        {
            Username = user.Username,
            Password = "correctPassword"
        };

        await Task.Delay(10);

        ActionResult<UserLoginResultDto> result = await Controller.Login(loginDto);

        UserLoginResultDto? loginResult = result.Value;
        Assert.IsNotNull(loginResult);

        User? updatedUser = await Context.Users.FindAsync(user.Id);
        Assert.IsNotNull(updatedUser);
        Assert.IsTrue(updatedUser.LastLogin > originalLastLogin);
    }

    [TestMethod]
    public async Task LoginCreatesSuccessfulLoginAttempt()
    {
        User user = Context.Users.First();
        user.PasswordHash = "correctPasswordhashed";
        await Context.SaveChangesAsync();

        UserLoginDto loginDto = new()
        {
            Username = user.Username,
            Password = "correctPassword"
        };

        ActionResult<UserLoginResultDto> result = await Controller.Login(loginDto);

        UserLoginResultDto? loginResult = result.Value;
        Assert.IsNotNull(loginResult);

        LoginAttempt? loginAttempt = Context.LoginAttempts.FirstOrDefault(la => la.Username == user.Username);
        Assert.IsNotNull(loginAttempt);
        Assert.IsTrue(loginAttempt.Success);
        Assert.AreEqual("Login", loginAttempt.Type);
    }

    [TestMethod]
    public async Task LoginWithEmptyDatabaseRedirectsToClaim()
    {
        Context.Users.RemoveRange(Context.Users);
        await Context.SaveChangesAsync();

        UserLoginDto loginDto = new()
        {
            Username = "testuser",
            Password = "password"
        };

        ActionResult<UserLoginResultDto> result = await Controller.Login(loginDto);

        UserLoginResultDto? loginResult = result.Value;
        Assert.IsNotNull(loginResult);

        User? createdUser = Context.Users.FirstOrDefault(u => u.Username == "testuser");
        Assert.IsNotNull(createdUser);
        Assert.AreEqual($"{createdUser.Id}-jwt-token", loginResult.Token);
        Assert.AreEqual(UserRole.Owner, loginResult.User.Role);
    }

    [TestMethod]
    public async Task LoginVerifyLoginAttemptHasCorrectOrigin()
    {
        User user = Context.Users.First();
        user.PasswordHash = "correctPasswordhashed";
        await Context.SaveChangesAsync();

        UserLoginDto loginDto = new()
        {
            Username = user.Username,
            Password = "correctPassword"
        };

        Controller.ControllerContext.HttpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");

        ActionResult<UserLoginResultDto> result = await Controller.Login(loginDto);

        UserLoginResultDto? loginResult = result.Value;
        Assert.IsNotNull(loginResult);

        LoginAttempt? loginAttempt = Context.LoginAttempts.FirstOrDefault(la => la.Username == user.Username);
        Assert.IsNotNull(loginAttempt);
        Assert.AreEqual("127.0.0.1", loginAttempt.Origin);
    }
}