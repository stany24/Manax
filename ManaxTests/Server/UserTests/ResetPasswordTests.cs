using ManaxLibrary;
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

        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        string? newPassword = okResult.Value as string;
        Assert.IsNotNull(newPassword);
        Assert.AreEqual("MockPassword123!", newPassword);

        User? updatedUser = await Context.Users.FindAsync(user.Id);
        Assert.IsNotNull(updatedUser);
        Assert.AreNotEqual(originalPasswordHash, updatedUser.PasswordHash);
        MockHashService.VerifyHashPasswordCalled("MockPassword123!");
    }

    [TestMethod]
    public async Task ResetPasswordWithInvalidIdReturnsNotFound()
    {
        ActionResult<string> result = await Controller.ResetPassword(999999);

        TestSetup.CheckTypeAndErrorCode<NotFoundObjectResult>(result.Result, ErrorCode.UserDoesNotExist);
        MockHashService.VerifyHashPasswordNotCalled();
    }
}