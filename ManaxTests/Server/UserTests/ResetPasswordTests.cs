using ManaxServer.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.Server.UserTests;

[TestClass]
public class ResetPasswordTests : UserTestsSetup
{
    [TestMethod]
    public async Task ResetPasswordWithValidIdResetsPasswordAndReturnsNewPassword()
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
    public async Task ResetPasswordWithInvalidIdReturnsNotFound()
    {
        ActionResult<string> result = await Controller.ResetPassword(999999);

        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        MockHashService.VerifyHashPasswordNotCalled();
    }
}