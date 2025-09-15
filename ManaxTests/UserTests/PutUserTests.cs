using ManaxLibrary.DTO.User;
using ManaxServer.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.UserTests;

[TestClass]
public class PutUserTests : UserTestsSetup
{
    [TestMethod]
    public async Task PutUser_WithValidPassword_UpdatesCurrentUserPassword()
    {
        User user = Context.Users.First(u => u.Id == 2);
        string originalPasswordHash = user.PasswordHash;
        const string newPassword = "newValidPassword123!";
        UserUpdateDto userUpdate = new()
        {
            Username = user.Username,
            Password = newPassword
        };

        IActionResult result = await Controller.PutUser(userUpdate);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        User? updatedUser = await Context.Users.FindAsync(2L);
        Assert.IsNotNull(updatedUser);
        Assert.AreNotEqual(originalPasswordHash, updatedUser.PasswordHash);
        MockHashService.VerifyHashPasswordCalled(newPassword);
    }

    [TestMethod]
    public async Task PutUser_WithInvalidPassword_ReturnsBadRequest()
    {
        User user = Context.Users.First(u => u.Id == 2);
        UserUpdateDto userUpdate = new()
        {
            Username = user.Username,
            Password = ""
        };

        IActionResult result = await Controller.PutUser(userUpdate);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        MockHashService.VerifyHashPasswordNotCalled();
    }

    [TestMethod]
    public async Task PutUser_WithoutAuthenticatedUser_ReturnsUnauthorized()
    {
        Controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal();
        User user = Context.Users.First(u => u.Id == 2);
        UserUpdateDto userUpdate = new()
        {
            Username = user.Username,
            Password = "newValidPassword123!"
        };

        IActionResult result = await Controller.PutUser(userUpdate);

        Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        MockHashService.VerifyHashPasswordNotCalled();
    }

    [TestMethod]
    public async Task ResetPassword_WithValidId_ResetsPasswordAndReturnsNewPassword()
    {
        User user = Context.Users.First(u => u.Id == 2);
        string originalPasswordHash = user.PasswordHash;

        ActionResult<string> result = await Controller.ResetPassword(2);

        string? newPassword = result.Value;
        Assert.IsNotNull(newPassword);

        User? updatedUser = await Context.Users.FindAsync(2L);
        Assert.IsNotNull(updatedUser);
        Assert.AreNotEqual(originalPasswordHash, updatedUser.PasswordHash);
        MockHashService.VerifyHashPasswordCalled(newPassword);
    }

    [TestMethod]
    public async Task ResetPassword_WithInvalidId_ReturnsNotFound()
    {
        ActionResult<string> result = await Controller.ResetPassword(999);

        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        MockHashService.VerifyHashPasswordNotCalled();
    }
}