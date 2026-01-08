using ManaxLibrary;
using ManaxLibrary.DTO.User;
using ManaxServer.Models.Claim;
using ManaxServer.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.UserTests;

[TestClass]
public class ClaimUserTests : UserTestsSetup
{
    [TestMethod]
    public async Task ClaimWithNoExistingUsersCreatesOwnerUser()
    {
        Context.Users.RemoveRange(Context.Users);
        await Context.SaveChangesAsync();

        ClaimRequest claimRequest = new()
        {
            Username = "FirstOwner",
            Password = "ownerPassword"
        };

        ActionResult<UserLoginResultDto> result = Controller.Claim(claimRequest);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        UserLoginResultDto? claimResult = okResult.Value as UserLoginResultDto;
        Assert.IsNotNull(claimResult);

        User? createdUser = Context.Users.FirstOrDefault(u => u.Username == "FirstOwner");
        Assert.IsNotNull(createdUser);
        Assert.AreEqual($"{createdUser.Id}-jwt-token", claimResult.Token);
        Assert.IsNotNull(claimResult.User);
        Assert.AreEqual(UserRole.Owner, claimResult.User.Role);
        Assert.AreEqual(UserRole.Owner, createdUser.Role);
    }

    [TestMethod]
    public Task ClaimWithExistingUsersReturnsUnauthorized()
    {
        ClaimRequest claimRequest = new()
        {
            Username = "SomeUser",
            Password = "password"
        };

        ActionResult<UserLoginResultDto> result = Controller.Claim(claimRequest);
        
        CheckTypeAndErrorCode<UnauthorizedObjectResult>(result.Result, ErrorCode.ServerAlreadyClaimed);
        LoginAttempt? claimAttempt = Context.LoginAttempts.FirstOrDefault(la => la.Type == "Claim");
        Assert.IsNotNull(claimAttempt);
        Assert.IsFalse(claimAttempt.Success);
        return Task.CompletedTask;
    }

    [TestMethod]
    public async Task ClaimCreatesClaimLoginAttempt()
    {
        Context.Users.RemoveRange(Context.Users);
        await Context.SaveChangesAsync();

        ClaimRequest claimRequest = new()
        {
            Username = "FirstOwner",
            Password = "ownerPassword"
        };

        ActionResult<UserLoginResultDto> result = Controller.Claim(claimRequest);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        
        UserLoginResultDto? claimResult = okResult.Value as UserLoginResultDto;
        Assert.IsNotNull(claimResult);

        LoginAttempt? claimAttempt = Context.LoginAttempts.FirstOrDefault(la => la.Type == "Claim");
        Assert.IsNotNull(claimAttempt);
        Assert.IsTrue(claimAttempt.Success);
        Assert.AreEqual("FirstOwner", claimAttempt.Username);
    }

    [TestMethod]
    public async Task ClaimVerifyPasswordIsHashed()
    {
        Context.Users.RemoveRange(Context.Users);
        await Context.SaveChangesAsync();

        ClaimRequest claimRequest = new()
        {
            Username = "HashedPasswordTest",
            Password = "plainTextPassword"
        };

        ActionResult<UserLoginResultDto> result = Controller.Claim(claimRequest);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        
        UserLoginResultDto? claimResult = okResult.Value as UserLoginResultDto;
        Assert.IsNotNull(claimResult);

        MockHashService.VerifyHashPasswordCalled("plainTextPassword");

        User? createdUser = Context.Users.FirstOrDefault(u => u.Username == "HashedPasswordTest");
        Assert.IsNotNull(createdUser);
        Assert.AreEqual("plainTextPasswordhashed", createdUser.PasswordHash);
    }
}