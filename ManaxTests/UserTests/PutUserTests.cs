using ManaxLibrary.DTO.User;
using ManaxServer.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace ManaxTests.UserTests;

[TestClass]
public class PutUserTests : UserTestsSetup
{
    [TestMethod]
    public async Task PutUser_WithValidId_UpdatesUser()
    {
        User user = Context.Users.First();
        UserUpdateDto updateDto = new()
        {
            Username = "UpdatedUsername",
            Role = UserRole.Admin,
            Password = "newPassword"
        };

        IActionResult result = await Controller.PutUser(user.Id, updateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        User? updatedUser = await Context.Users.FindAsync(user.Id);
        Assert.IsNotNull(updatedUser);
        Assert.AreEqual(updateDto.Username, updatedUser.Username);
        Assert.AreEqual(updateDto.Role, updatedUser.Role);
        MockHashService.VerifyHashPasswordCalled("newPassword");
    }

    [TestMethod]
    public async Task PutUser_WithInvalidId_ReturnsNotFound()
    {
        UserUpdateDto updateDto = new()
        {
            Username = "UpdatedUsername",
            Role = UserRole.Admin,
            Password = "newPassword"
        };

        IActionResult result = await Controller.PutUser(999999, updateDto);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task PutUser_WithoutPassword_DoesNotUpdatePassword()
    {
        User user = Context.Users.First();
        string originalPasswordHash = user.PasswordHash;
        UserUpdateDto updateDto = new()
        {
            Username = "UpdatedUsername",
            Role = UserRole.Admin,
            Password = ""
        };

        IActionResult result = await Controller.PutUser(user.Id, updateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        User? updatedUser = await Context.Users.FindAsync(user.Id);
        Assert.IsNotNull(updatedUser);
        Assert.AreEqual(originalPasswordHash, updatedUser.PasswordHash);
        MockHashService.VerifyHashPasswordNotCalled();
    }

    [TestMethod]
    public async Task PutUser_WithEmptyPassword_DoesNotCallHashService()
    {
        User user = Context.Users.First();
        UserUpdateDto updateDto = new()
        {
            Username = "UpdatedUser",
            Role = UserRole.User,
            Password = ""
        };

        IActionResult result = await Controller.PutUser(user.Id, updateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        MockHashService.VerifyHashPasswordNotCalled();
    }

    [TestMethod]
    public async Task PutUser_WithNullPassword_DoesNotCallHashService()
    {
        User user = Context.Users.First();
        UserUpdateDto updateDto = new()
        {
            Username = "UpdatedUser",
            Role = UserRole.User,
            Password = null!
        };

        IActionResult result = await Controller.PutUser(user.Id, updateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        MockHashService.VerifyHashPasswordNotCalled();
    }
}