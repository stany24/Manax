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
        User user = _context.Users.First();
        UserUpdateDto updateDto = new()
        {
            Username = "UpdatedUsername",
            Role = UserRole.Admin,
            Password = "newPassword"
        };

        IActionResult result = await _controller.PutUser(user.Id, updateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        User? updatedUser = await _context.Users.FindAsync(user.Id);
        Assert.IsNotNull(updatedUser);
        Assert.AreEqual(updateDto.Username, updatedUser.Username);
        Assert.AreEqual(updateDto.Role, updatedUser.Role);
        _mockHashService.VerifyHashPasswordCalled("newPassword");
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

        IActionResult result = await _controller.PutUser(999999, updateDto);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task PutUser_WithoutPassword_DoesNotUpdatePassword()
    {
        User user = _context.Users.First();
        string originalPasswordHash = user.PasswordHash;
        UserUpdateDto updateDto = new()
        {
            Username = "UpdatedUsername",
            Role = UserRole.Admin,
            Password = ""
        };

        IActionResult result = await _controller.PutUser(user.Id, updateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        User? updatedUser = await _context.Users.FindAsync(user.Id);
        Assert.IsNotNull(updatedUser);
        Assert.AreEqual(originalPasswordHash, updatedUser.PasswordHash);
        _mockHashService.VerifyHashPasswordNotCalled();
    }

    [TestMethod]
    public async Task PutUser_WithEmptyPassword_DoesNotCallHashService()
    {
        User user = _context.Users.First();
        UserUpdateDto updateDto = new()
        {
            Username = "UpdatedUser",
            Role = UserRole.User,
            Password = ""
        };

        IActionResult result = await _controller.PutUser(user.Id, updateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        _mockHashService.VerifyHashPasswordNotCalled();
    }

    [TestMethod]
    public async Task PutUser_WithNullPassword_DoesNotCallHashService()
    {
        User user = _context.Users.First();
        UserUpdateDto updateDto = new()
        {
            Username = "UpdatedUser",
            Role = UserRole.User,
            Password = null!
        };

        IActionResult result = await _controller.PutUser(user.Id, updateDto);

        Assert.IsInstanceOfType(result, typeof(OkResult));
        _mockHashService.VerifyHashPasswordNotCalled();
    }
}