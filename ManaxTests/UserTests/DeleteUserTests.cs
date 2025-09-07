using System.Security.Claims;
using ManaxLibrary.DTO.User;
using ManaxServer.Models.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ManaxTests.UserTests;

[TestClass]
public class DeleteUserTests : UserTestsSetup
{
    [TestMethod]
    public async Task DeleteUser_WithValidId_RemovesUser()
    {
        User user = _context.Users.First();
        IActionResult result = await _controller.DeleteUser(user.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        User? deletedUser = await _context.Users.FindAsync(user.Id);
        Assert.IsNull(deletedUser);
        _mockNotificationService.Verify(x => x.NotifyUserDeletedAsync(user.Id), Times.Once);
    }

    [TestMethod]
    public async Task DeleteUser_WithInvalidId_ReturnsNotFound()
    {
        IActionResult result = await _controller.DeleteUser(999999);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task DeleteUser_TryingToDeleteSelf_ReturnsForbid()
    {
        ClaimsPrincipal selfUser = new(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "TestUser1"),
            new Claim(ClaimTypes.Role, "User")
        ], "test"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = selfUser
            }
        };

        IActionResult result = await _controller.DeleteUser(1);

        Assert.IsInstanceOfType(result, typeof(ForbidResult));
    }

    [TestMethod]
    public async Task DeleteUser_AdminTryingToDeleteOwner_ReturnsForbid()
    {
        User ownerUser = new()
        {
            Id = 100,
            Username = "OwnerUser",
            Role = UserRole.Owner,
            Creation = DateTime.UtcNow
        };
        _context.Users.Add(ownerUser);
        await _context.SaveChangesAsync();

        IActionResult result = await _controller.DeleteUser(100);

        Assert.IsInstanceOfType(result, typeof(ForbidResult));
    }

    [TestMethod]
    public async Task DeleteUser_AdminTryingToDeleteAnotherAdmin_ReturnsForbid()
    {
        User anotherAdmin = new()
        {
            Id = 101,
            Username = "AnotherAdmin",
            Role = UserRole.Admin,
            Creation = DateTime.UtcNow
        };
        _context.Users.Add(anotherAdmin);
        await _context.SaveChangesAsync();

        IActionResult result = await _controller.DeleteUser(101);

        Assert.IsInstanceOfType(result, typeof(ForbidResult));
    }

    [TestMethod]
    public async Task DeleteUser_WithoutAuthentication_ReturnsUnauthorized()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        IActionResult result = await _controller.DeleteUser(1);

        Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
    }

    [TestMethod]
    public async Task DeleteUser_VerifyUserCountDecreases()
    {
        int initialCount = _context.Users.Count();
        User user = _context.Users.First();

        IActionResult result = await _controller.DeleteUser(user.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));

        int finalCount = _context.Users.Count();
        Assert.AreEqual(initialCount - 1, finalCount);
    }
    
    [TestMethod]
    public async Task DeleteUser_WithNonExistentCurrentUser_ReturnsUnauthorized()
    {
        ClaimsPrincipal nonExistentUser = new(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "999"),
            new Claim(ClaimTypes.Name, "NonExistent"),
            new Claim(ClaimTypes.Role, "Admin")
        ], "test"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = nonExistentUser
            }
        };

        IActionResult result = await _controller.DeleteUser(1);

        Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
    }

    [TestMethod]
    public async Task DeleteUser_OwnerCanDeleteAdmin()
    {
        ClaimsPrincipal ownerUser = new(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "3"),
            new Claim(ClaimTypes.Name, "OwnerUser"),
            new Claim(ClaimTypes.Role, "Owner")
        ], "test"));

        User ownerUserEntity = new()
        {
            Id = 3,
            Username = "OwnerUser",
            Role = UserRole.Owner,
            Creation = DateTime.UtcNow
        };
        _context.Users.Add(ownerUserEntity);
        await _context.SaveChangesAsync();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = ownerUser
            }
        };

        User adminUser = _context.Users.First(u => u.Role == UserRole.Admin);
        IActionResult result = await _controller.DeleteUser(adminUser.Id);

        Assert.IsInstanceOfType(result, typeof(OkResult));
    }
}