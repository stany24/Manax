using ManaxServer.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.UserTests;

[TestClass]
public class ResetPasswordTests : UserTestsSetup
{
    [TestMethod]
    public async Task ResetPassword_WithValidId_ResetsPasswordAndReturnsNewPassword()
    {
        User user = Context.Users.First();
        string originalPasswordHash = user.PasswordHash;

        ActionResult<string> result = await Controller.ResetPassword(user.Id);

        Assert.IsNotNull(result.Value);
        Assert.AreEqual("MockPassword123!", result.Value);

        User? updatedUser = await Context.Users.FindAsync(user.Id);
        Assert.IsNotNull(updatedUser);
        Assert.AreNotEqual(originalPasswordHash, updatedUser.PasswordHash);
        MockHashService.VerifyHashPasswordCalled("MockPassword123!");
    }

    [TestMethod]
    public async Task ResetPassword_WithInvalidId_ReturnsNotFound()
    {
        ActionResult<string> result = await Controller.ResetPassword(999999);

        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        MockHashService.VerifyHashPasswordNotCalled();
    }
}