using ManaxLibrary.DTO.User;
using ManaxServer.Models.Claim;
using ManaxServer.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.UserTests;

[TestClass]
public class ClaimUserTests: UserTestsSetup
{
    [TestMethod]
    public async Task Claim_WithNoExistingUsers_CreatesOwnerUser()
    {
        _context.Users.RemoveRange(_context.Users);
        await _context.SaveChangesAsync();

        ClaimRequest claimRequest = new()
        {
            Username = "FirstOwner",
            Password = "ownerPassword"
        };

        ActionResult<UserLoginResultDto> result = _controller.Claim(claimRequest);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNull(okResult);

        UserLoginResultDto? claimResult = result.Value;
        Assert.IsNotNull(claimResult);

        User? createdUser = _context.Users.FirstOrDefault(u => u.Username == "FirstOwner");
        Assert.IsNotNull(createdUser);
        Assert.AreEqual($"{createdUser.Id}-jwt-token", claimResult.Token);
        Assert.IsNotNull(claimResult.User);
        Assert.AreEqual(UserRole.Owner, claimResult.User.Role);
        Assert.AreEqual(UserRole.Owner, createdUser.Role);
    }

    [TestMethod]
    public Task Claim_WithExistingUsers_ReturnsUnauthorized()
    {
        ClaimRequest claimRequest = new()
        {
            Username = "SomeUser",
            Password = "password"
        };

        ActionResult<UserLoginResultDto> result = _controller.Claim(claimRequest);

        Assert.IsInstanceOfType(result.Result, typeof(UnauthorizedObjectResult));

        LoginAttempt? claimAttempt = _context.LoginAttempts.FirstOrDefault(la => la.Type == "Claim");
        Assert.IsNotNull(claimAttempt);
        Assert.IsFalse(claimAttempt.Success);
        return Task.CompletedTask;
    }

    [TestMethod]
    public async Task Claim_CreatesClaimLoginAttempt()
    {
        _context.Users.RemoveRange(_context.Users);
        await _context.SaveChangesAsync();

        ClaimRequest claimRequest = new()
        {
            Username = "FirstOwner",
            Password = "ownerPassword"
        };

        ActionResult<UserLoginResultDto> result = _controller.Claim(claimRequest);

        UserLoginResultDto? claimResult = result.Value;
        Assert.IsNotNull(claimResult);

        LoginAttempt? claimAttempt = _context.LoginAttempts.FirstOrDefault(la => la.Type == "Claim");
        Assert.IsNotNull(claimAttempt);
        Assert.IsTrue(claimAttempt.Success);
        Assert.AreEqual("FirstOwner", claimAttempt.Username);
    }

    [TestMethod]
    public async Task Claim_VerifyPasswordIsHashed()
    {
        _context.Users.RemoveRange(_context.Users);
        await _context.SaveChangesAsync();

        ClaimRequest claimRequest = new()
        {
            Username = "HashedPasswordTest",
            Password = "plainTextPassword"
        };

        ActionResult<UserLoginResultDto> result = _controller.Claim(claimRequest);

        UserLoginResultDto? claimResult = result.Value;
        Assert.IsNotNull(claimResult);

        _mockHashService.VerifyHashPasswordCalled("plainTextPassword");

        User? createdUser = _context.Users.FirstOrDefault(u => u.Username == "HashedPasswordTest");
        Assert.IsNotNull(createdUser);
        Assert.AreEqual("plainTextPasswordhashed", createdUser.PasswordHash);
    }
}