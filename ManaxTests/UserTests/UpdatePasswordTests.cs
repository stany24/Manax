using ManaxServer.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.UserTests;

[TestClass]
public class UpdatePasswordTests : UserTestsSetup
{
    [TestMethod]
    public async Task UpdatePassword_WithValidPassword_UpdatesCurrentUserPassword()
    {
        User user = Context.Users.First(u => u.Id == 2);
        string originalPasswordHash = user.PasswordHash;
        const string newPassword = "newValidPassword123!";

        IActionResult result = await Controller.UpdatePassword(newPassword);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        User? updatedUser = await Context.Users.FindAsync(2L);
        Assert.IsNotNull(updatedUser);
        Assert.AreNotEqual(originalPasswordHash, updatedUser.PasswordHash);
        MockHashService.VerifyHashPasswordCalled(newPassword);
    }

    [TestMethod]
    public async Task UpdatePassword_WithInvalidPassword_ReturnsBadRequest()
    {
        const string invalidPassword = "";

        IActionResult result = await Controller.UpdatePassword(invalidPassword);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        MockHashService.VerifyHashPasswordNotCalled();
    }

    [TestMethod]
    public async Task UpdatePassword_WithoutAuthenticatedUser_ReturnsUnauthorized()
    {
        Controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal();
        const string newPassword = "newValidPassword123!";

        IActionResult result = await Controller.UpdatePassword(newPassword);

        Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        MockHashService.VerifyHashPasswordNotCalled();
    }
}